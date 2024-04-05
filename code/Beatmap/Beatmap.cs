using System.Text.Json.Serialization;
using Sandbox;

namespace Rhythm4K;

public class Beatmap
{
    public static Beatmap Loaded { get; set; }

    public string Name { get; set; }
    public string Artist { get; set; }
    public string Charter { get; set; }
    public float Difficulty { get; set; }
    public string DifficultyName { get; set; }
    public List<BpmChange> BpmChanges { get; set; } = new();
    public List<Note> Notes { get; set; } = new();
    public int Lanes { get; set; } = 4;
    public int TotalNotes { get; set; }
    public int TotalChain { get; set; }
    public float ScrollSpeed { get; set; } = 1f;

    public string AudioFilename { get; set; }
    public float Offset { get; set; }
    public float SampleStart { get; set; }
    public float SampleLength { get; set; }
    public float Length { get; set; }

    public string FilePath { get; set; }
    [JsonIgnore] public BaseFileSystem FileSystem { get; set; }

    [JsonIgnore] public BeatmapSet BeatmapSet { get; set; }

    public BeatmapSet GetBeatmapSet()
    {
        if ( BeatmapSet is not null )
        {
            return BeatmapSet;
        }
        return BeatmapSet.All.FirstOrDefault( x => x.Beatmaps.Contains( this ) );
    }

    public int GetHighscore()
    {
        var replay = GameStats.GetStats( this ).Replay;
        if ( replay is null ) return 0;
        return replay.Score;
    }

    /// <summary>
    /// Returns a baked time (in seconds) based on BPM changes given an offset in steps.
    /// </summary>
    public float GetTimeFromOffset( float offset )
    {
        float currentOffset = 0f;
        float currentTime = 0f;
        float bpm = BpmChanges[0].BPM;
        float offsetChange = 0f;
        foreach ( BpmChange bpmChange in BpmChanges.OrderBy( o => o.Offset ) )
        {
            if ( bpmChange.Offset > offset ) break;
            offsetChange = bpmChange.Offset - currentOffset;
            currentOffset += offsetChange;
            currentTime += (offsetChange / 1000f) * ((60f / bpm) * 4f);
            bpm = bpmChange.BPM;
        }
        offsetChange = offset - currentOffset;
        currentTime += (offsetChange / 1000f) * ((60f / bpm) * 4f);
        return currentTime;
    }

    /// <summary>
    /// Returns an offset in steps based on BPM changes given a time in the song (in seconds).
    /// </summary>
    public float GetOffsetFromTime( float time )
    {
        float currentOffset = 0f;
        float currentTime = 0f;
        float bpm = BpmChanges[0].BPM;
        float timeChange = 0f;
        foreach ( BpmChange bpmChange in BpmChanges.OrderBy( o => o.Offset ) )
        {
            if ( currentTime > time ) break;
            timeChange = bpmChange.Offset - currentOffset;
            currentOffset += timeChange;
            currentTime += (timeChange / 1000f) * ((60f / bpm) * 4f);
            bpm = bpmChange.BPM;
        }
        timeChange = time - currentTime;
        currentOffset += timeChange / ((60f / bpm) * 4f) * 1000f;
        return currentOffset;
    }


    /// <summary>
    /// Check if the chart is valid
    /// </summary>
    public bool IsValid()
    {
        if ( BpmChanges.Count == 0 ) return false;

        return true;
    }

    public string GetSongLength()
    {
        int minutes = (int)(Length / 60f);
        int seconds = (int)(Length % 60f);
        return $"{minutes}:{seconds:00}";
    }

    public void BakeValues()
    {
        TotalNotes = 0;
        TotalChain = 0;

        List<BpmChange> bpmChanges = new();
        foreach ( BpmChange bpmchange in BpmChanges.OrderBy( o => o.Offset ) )
        {
            bpmChanges.Add( bpmchange );
        }

        List<Note> holds = new();
        for ( int i = 0; i < Notes.Count; i++ )
        {
            Note note = Notes[i];
            if ( note.BakedTime >= 0f ) continue;
            note.BakedTime = GetTimeFromOffset( note.Offset );
            TotalNotes++;
            TotalChain++;
            if ( note.Length > 0f )
            {
                note.BakedLength = GetTimeFromOffset( note.Offset + note.Length ) - note.BakedTime;
                float length = note.Length;
                float offset = note.Offset;
                // TODO: This is a bit of a hack, but it works for now.
                // Should be re-written to support bpm and bpm changes mid-hold.
                while ( length >= 62.5f )
                {
                    offset += 62.5f;
                    length -= 62.5f;

                    float holdBakedTime = GetTimeFromOffset( offset );
                    float holdBakedLength = GetTimeFromOffset( offset + length ) - holdBakedTime;

                    var hold = new Note
                    {
                        Offset = offset,
                        Length = length,
                        Lane = note.Lane,
                        Type = (int)NoteType.Hold,
                        BakedTime = holdBakedTime,
                        BakedLength = holdBakedLength
                    };

                    Notes.Add( hold );
                    TotalChain++;
                }
            }
            Notes[i] = note;
        }
    }
}

public struct BpmChange
{
    public float Offset { get; set; }
    public float BPM { get; set; }

    public BpmChange( float offset, float bpm )
    {
        Offset = offset;
        BPM = bpm;
    }
}
