using System.Text.RegularExpressions;
using System.Net.Mime;
using System;
using System.Collections.Generic;
using Sandbox;

namespace Rhythm4K;

public static class SongBuilder
{
    public static Song Load(string file)
    {
        var song = new Song();

        if(!FileSystem.Mounted.FileExists(file))
        {
            Log.Warning($"Chart file {file} does not exist");
            return song;
        }

        if(file.EndsWith(".sm"))
        {
            // Load stepmania chart
            song =  LoadFromSM(file);
        }

        // Calculate baked values
        for(int i=0; i<song.Charts.Count; i++)
        {
            song.Charts[i].BakeValues();
        }

        return song;
    }

    public static Song LoadFromSM(string file)
    {
        if(!file.EndsWith(".sm")) return new Song();

        string text = FileSystem.Mounted.ReadAllText(file);
        string[] properties = text.Split('#');
        Song song = new Song();
        List<BpmChange> bpmChanges = new List<BpmChange>();
        song.Charts = new List<Chart>();
        string charterName = "";

        string value;
        foreach(string prop in properties)
        {
            value = SMGetValue(prop, "TITLE");
            if(value != "")
            {
                song.Name = value;
                continue;
            }
            value = SMGetValue(prop, "ARTIST");
            if(value != "")
            {
                song.Artist = value;
                continue;
            }
            value = SMGetValue(prop, "CREDIT");
            if(value != "")
            {
                charterName = value;
                continue;
            }
            // value = SMGetValue(prop, "MUSIC");
            // if(value != "")
            // {
            //     song.Sound = value;
            //     continue;
            // }
            value = SMGetValue(prop, "BPMS");
            if(value != "")
            {
                string[] changes = value.Trim().Split(',');
                foreach(string change in changes)
                {
                    var vals = change.Split('=');
                    bpmChanges.Add(new BpmChange(float.Parse(vals[0]), float.Parse(vals[1])));
                }
                song.BPM = bpmChanges[0].BPM;
                continue;
            }
            value = SMGetValue(prop, "OFFSET");
            if(value != "")
            {
                song.Offset = float.Parse(value);
                continue;
            }
            value = SMGetValue(prop, "SAMPLESTART");
            if(value != "")
            {
                song.SampleStart = float.Parse(value);
                continue;
            }
            value = SMGetValue(prop, "SAMPLELENGTH");
            if(value != "")
            {
                song.SampleLength = float.Parse(value);
                continue;
            }
            value = SMGetValue(prop, "NOTES");
            if(value != "")
            {
                string[] noteSplit = value.Split('\n');

                // If the chart is a valid single player chart
                if(noteSplit[1].Contains("-single:"))
                {
                    float[] holding = { -1, -1, -1, -1 };
                    Chart chart = new Chart();
                    chart.Notes = new List<Note>();
                    chart.BpmChanges = bpmChanges;
                    chart.Name = noteSplit[3].Trim().Replace(":", "");
                    chart.Difficulty = int.Parse(noteSplit[4].Trim().Replace(":", ""));
                    chart.DifficultyName = noteSplit[3].Trim().Replace(":", "");
                    chart.Charter = charterName;

                    // Remove header and useless characters from the string
                    for(int i=0; i<6; i++)
                    {
                        value = value.Substring(value.IndexOf('\n') + 1);
                    }
                    value = Regex.Replace(value, "//.*", "");
                    value = Regex.Replace(value, "\\;", "");

                    // Re-split into arrays that contain each measure
                    noteSplit = Regex.Split(value, "\\,");
                    int measure = 0;
                    foreach(string note in noteSplit)
                    {
                        var lines = note.Trim().Split('\n');
                        var division = 1f / (lines.Length == 96 ? 192 : lines.Length);
                        int beat = 0;

                        // Loop through each line in the measure
                        foreach(var line in lines)
                        {
                            float time = (measure * 1000f) + (beat * division * 1000f);
                            for(int i=0; i<4; i++)
                            {
                                switch(line[i])
                                {
                                    case '1':
                                        chart.Notes.Add(new Note(time, i, NoteType.Normal));
                                        break;
                                    case '2':
                                        holding[i] = time;
                                        break;
                                    case '3':
                                        if(holding[i] != -1)
                                        {
                                            chart.Notes.Add(new Note(holding[i], i, 0, time - holding[i]));
                                            holding[i] = -1;
                                        }
                                        break;
                                    case 'M':
                                        chart.Notes.Add(new Note(time, i, NoteType.Mine));
                                        break;
                                }
                            }
                            beat++;
                        }
                        measure++;
                    }
                    song.Charts.Add(chart);
                }
                continue;
            }
        }

        return song;
    }

    private static string SMGetValue(string text, string tag)
    {
        var split = text.Split(':');
        if(split.Length >= 2)
        {
            var tagToCheck = split[0].Replace("#", "").Replace(";", "");
            if(tagToCheck.ToLower() == tag.ToLower())
            {
                if(split.Length > 2)
                {
                    return text.Remove(0, 6);
                }
                return split[1].Replace(";", "").Trim();
            }
        }
        return "";
    }
}