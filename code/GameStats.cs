using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace Rhythm4K;

public static class GameStats
{
    public static Dictionary<string, RhythmStats> Stats
    {
        get
        {
            if ( _stats is null )
            {
                var file = "/stats.json";
                _stats = FileSystem.Data.ReadJson( file, new Dictionary<string, RhythmStats>() );
            }
            return _stats;
        }
    }
    static Dictionary<string, RhythmStats> _stats;

    public static void SaveStats()
    {
        var file = "/stats.json";
        FileSystem.Data.WriteJson( file, Stats );
    }

    public static void SetReplay( Beatmap beatmap, Replay replay )
    {
        if ( !Stats.ContainsKey( beatmap.FilePath ) )
        {
            Stats[beatmap.FilePath] = new RhythmStats();
        }
        Stats[beatmap.FilePath].Replay = replay;
    }

    public static void AddScore( Beatmap beatmap, int score, int maxCombo, float accuracy )
    {
        if ( !Stats.ContainsKey( beatmap.FilePath ) )
        {
            Stats[beatmap.FilePath] = new RhythmStats();
        }
        Stats[beatmap.FilePath].Scores.Add( new RhythmScore( score, maxCombo, accuracy ) );
    }

    public static RhythmStats GetStats( Beatmap beatmap )
    {
        if ( !Stats.ContainsKey( beatmap.FilePath ) )
        {
            Stats[beatmap.FilePath] = new RhythmStats();
        }
        return Stats[beatmap.FilePath];
    }
}

public class RhythmStats
{
    public Replay Replay { get; set; }
    public List<RhythmScore> Scores { get; set; } = new();

    [JsonIgnore] public DateTime LastPlayed => Scores.Count > 0 ? Scores.Last().Date : DateTime.MinValue;
}

public class RhythmScore
{
    public DateTime Date { get; set; }
    public int Score { get; set; }
    public int MaxCombo { get; set; }
    public float Accuracy { get; set; }

    public RhythmScore( int score, int maxCombo, float accuracy )
    {
        Date = DateTime.Now;
        Score = score;
        MaxCombo = maxCombo;
        Accuracy = accuracy;
    }
}