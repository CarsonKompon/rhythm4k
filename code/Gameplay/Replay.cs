using System;
using Sandbox;

namespace Rhythm4K;

public class Replay
{
    public Beatmap Beatmap { get; set; }
    public int MaxCombo { get; set; } = 0;
    public List<HitInfo> Hits = new();
    public int Score { get; set; } = 0;

    public Replay( Beatmap beatmap )
    {
        Beatmap = beatmap;
    }

    public float GetAccuracy()
    {
        var judgementTimes = Judgement.GetJudgementTimes( Beatmap.Difficulty );
        var total = 0f;
        foreach ( var hit in Hits )
        {
            var diff = MathF.Abs( hit.Offset );
            var score = 0f;
            for ( var i = 0; i < judgementTimes.Count; i++ )
            {
                if ( diff < judgementTimes[i] )
                {
                    score = Judgement.Scores[i];
                    break;
                }
            }
            total += score;
        }
        return total / (300f * Beatmap.Notes.Count) * 100f;
    }

    public int[] GetJudgements()
    {
        int[] judgements = new int[Judgement.Names.Count];
        foreach ( var hit in Hits )
        {
            var diff = MathF.Abs( hit.Offset );
            for ( var i = 0; i < Judgement.Names.Count; i++ )
            {
                if ( i == Judgement.Names.Count - 1 || diff < Judgement.GetJudgementTimes( Beatmap.Difficulty )[i] )
                {
                    judgements[i]++;
                    break;
                }
            }
        }
        return judgements;
    }

    public int[] GetTimingDistribution( int divisions, float difficulty )
    {
        var times = Judgement.GetJudgementTimes( difficulty ).LastOrDefault();
        var distribution = new int[divisions];

        foreach ( var hit in Hits )
        {
            var mappedOffset = (int)MathF.Round( MathX.Remap( hit.Offset, -times, times, 0, divisions ) );
            if ( mappedOffset < 0 || mappedOffset >= divisions ) continue;
            distribution[mappedOffset]++;
        }

        return distribution;
    }
}