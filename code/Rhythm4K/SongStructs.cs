using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Rhythm4K;

public struct Song
{
    public string Name { get; set; }
    public string Artist { get; set; }
    public float Offset { get; set; }
    public float SampleStart { get; set; }
    public float SampleLength { get; set; }
    public float BPM { get; set; }
    public List<Chart> Charts { get; set; }
}

public class Chart
{
    public string Name { get; set; }
    public string Charter { get; set; }
    public int Difficulty { get; set; }
    public string DifficultyName { get; set; }
    public List<BpmChange> BpmChanges { get; set; }
    public List<Note> Notes { get; set; }
    public int TotalNotes { get; set; }
    public int TotalChain { get; set; }

    /// <summary>
    /// Returns the length of the song in seconds
    /// </summary>
    public float GetSongLength()
    {
        Note lastNote = Notes.OrderBy(o => -o.BakedTime).ToList()[0];
        return lastNote.BakedTime + lastNote.BakedLength;
    }

    /// <summary>
    /// Returns a baked time (in seconds) based on BPM changes given an offset in steps.
    /// </summary>
    public float GetTimeFromOffset(float offset)
    {
        float currentOffset = 0f;
        float currentTime = 0f;
        float bpm = BpmChanges[0].BPM;
        float offsetChange = 0f;
        foreach(BpmChange bpmChange in BpmChanges.OrderBy(o=>o.Offset))
        {
            if(bpmChange.Offset > offset) break;
            offsetChange = bpmChange.Offset - currentOffset;
            currentOffset += offsetChange;
            currentTime += (offsetChange/1000f) * ((60f/bpm)*4f);
            bpm = bpmChange.BPM;
        }
        offsetChange = offset - currentOffset;
        currentTime += (offsetChange/1000f) * ((60f/bpm)*4f);
        return currentTime;
    }

    /// <summary>
    /// Returns an offset in steps based on BPM changes given a time in the song (in seconds).
    /// </summary>
    public float GetOffsetFromTime(float time)
    {
        float currentOffset = 0f;
        float currentTime = 0f;
        float bpm = BpmChanges[0].BPM;
        float timeChange = 0f;
        foreach(BpmChange bpmChange in BpmChanges.OrderBy(o=>o.Offset))
        {
            if(currentTime > time) break;
            timeChange = bpmChange.Offset - currentOffset;
            currentOffset += timeChange;
            currentTime += (timeChange/1000f) * ((60f/bpm)*4f);
            bpm = bpmChange.BPM;
        }
        timeChange = time - currentTime;
        currentOffset += timeChange / ((60f/bpm)*4f) * 1000f;
        return currentOffset;
    }


    /// <summary>
    /// Check if the chart is valid
    /// </summary>
    public bool IsValid()
    {
        if(BpmChanges.Count == 0) return false;

        return true;
    }

    public void BakeValues()
    {
        TotalNotes = 0;
        TotalChain = 0;

        List<BpmChange> bpmChanges = new();
        foreach(BpmChange bpmchange in BpmChanges.OrderBy(o=>o.Offset))
        {
            bpmChanges.Add(bpmchange);
        }

        List<Note> holds = new();
        for(int i=0; i<Notes.Count; i++)
        {
            Note note = Notes[i];
            note.BakedTime = GetTimeFromOffset(note.Offset);
            TotalNotes++;
            TotalChain++;
            if(note.Length > 0f)
            {
                note.BakedLength = GetTimeFromOffset(note.Offset + note.Length) - note.BakedTime;
                float length = note.Length;
                float offset = note.Offset;
                // TODO: This is a bit of a hack, but it works for now.
                // Should be re-written to support bpm and bpm changes mid-hold.
                while(length >= 62.5f)
                {
                    offset += 62.5f;
                    length -= 62.5f;
                    var hold = new Note();
                    hold.Offset = offset;
                    hold.Length = 0f;
                    hold.Lane = note.Lane;
                    hold.Type = (int)NoteType.Hold;
                    hold.BakedTime = GetTimeFromOffset(offset);
                    Notes.Add(hold);
                    TotalChain++;
                }
            }
            Notes[i] = note;
        }
    }
}

public struct Note
{
    public float Offset { get; set; }
    public float Length { get; set; }
    public int Type { get; set; }
    public int Lane { get; set; }
    public float BakedTime { get; set; }
    public float BakedLength { get; set; }
    public int Points { get; set; }
    public Arrow Arrow;

    public Note(float offset, int lane, NoteType type, float length = 0f)
    {
        Offset = offset;
        Lane = lane;
        Type = (int)type;
        Length = length;
    }
}

public struct BpmChange
{
    public float Offset { get; set; }
    public float BPM { get; set; }

    public BpmChange(float offset, float bpm)
    {
        Offset = offset;
        BPM = bpm;
    }
}