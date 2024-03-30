using System;
using Sandbox;

namespace Rhythm4K;

public class Replay
{
    public Beatmap Beatmap { get; set; }
    public int MaxCombo { get; set; } = 0;
    public List<HitInfo> Hits = new();

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
            var diff = MathF.Abs( hit.Time - hit.Offset );
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
}