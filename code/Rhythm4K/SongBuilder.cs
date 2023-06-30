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
	    // if you setup file incorrectly (like i did it myself)
	    // it will not load the char and throw error about source is null
	    
	    // took me a while to figure it out
	    if ( file is null )
	    {
		    // so let user know something is not right with .rhythm
		    Log.Error("The .rhythm file is broken!");
		    throw new ArgumentNullException();
	    }
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
        else if(file.EndsWith(".osu"))
        {
            // Load osu chart
            song = LoadFromOSU(file);
        }

        // Calculate baked values
        for(int i=0; i<song.Charts.Count; i++)
        {
            song.Charts[i].BakeValues();
        }

        return song;
    }

    // Stepmania Chart Loading
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

    public static Song LoadFromOSU(string rootFile)
    {
        if(!rootFile.EndsWith(".osu")) return new Song();

        Song song = new Song();
        song.Charts = new List<Chart>();
        song.Charts = new List<Chart>();
        string charterName = "";

        List<string> files = new();
        files.Add(rootFile);
        if(rootFile.EndsWith("-0.osu"))
        {
            int i = 1;
            while(FileSystem.Mounted.FileExists(rootFile.Replace("-0.osu", "-" + i + ".osu")))
            {
                files.Add(rootFile.Replace("-0.osu", "-" + i + ".osu"));
                i++;
            }
        }

        foreach(var file in files)
        {
            string text = FileSystem.Mounted.ReadAllText(file);
            string[] sections = text.Split('[');

            Chart chart = new Chart();
            chart.Notes = new List<Note>();
            chart.BpmChanges = new List<BpmChange>();
            chart.Charter = charterName;
        
            foreach(var section in sections)
            {
                string[] lines = section.Split('\n');
                if(lines[0].StartsWith("General"))
                {
                    foreach (var line in lines)
                    {
                        var split = line.Split(':');
                        if(split.Length < 2) continue;
                        var key = split[0].Trim();
                        var value = split[1].Trim();
                        switch(key)
                        {
                            case "PreviewTime":
                                song.SampleStart = float.Parse(value);
                                song.SampleLength = 6f;
                                break;
                            case "Mode":
                                if(value != "3")
                                {
                                    Log.Warning("Osu chart " + file + " is not an osu!mania chart, skipping");
                                    return new Song();
                                }
                                break;
                        }
                    }
                }
                else if(lines[0].StartsWith("Metadata"))
                {
                    foreach (var line in lines)
                    {
                        var split = line.Split(':');
                        if(split.Length < 2) continue;
                        var key = split[0].Trim();
                        var value = split[1].Trim();
                        switch(key)
                        {
                            case "Title":
                                if(file == rootFile) song.Name = value;
                                break;
                            case "TitleUnicode":
                                if(file == rootFile) song.Name = value;
                                break;
                            case "Artist":
                                if(file == rootFile) song.Artist = value;
                                break;
                            case "ArtistUnicode":
                                if(file == rootFile) song.Artist = value;
                                break;
                            case "Creator":
                                chart.Charter = value;
                                break;
                            case "Version":
                                chart.DifficultyName = value;
                                break;
                        }
                    }
                }
                else if(lines[0].StartsWith("Difficulty"))
                {
                    foreach (var line in lines)
                    {
                        var split = line.Split(':');
                        if(split.Length < 2) continue;
                        var key = split[0].Trim();
                        var value = split[1].Trim();
                        switch(key)
                        {
                            case "OverallDifficulty":
                                chart.Difficulty = int.Parse(value);
                                break;
                        }
                    }
                }
                else if(lines[0].StartsWith("TimingPoints"))
                {
                    for(int i=1; i<lines.Length; i++)
                    {
                        var line = lines[i];
                        var split = line.Split(',');
                        if(split.Length < 8) continue;
                        var offset = float.Parse(split[0]);
                        var beatLength = float.Parse(split[1]);
                        var bpm = 60000f / beatLength;
                        var meter = int.Parse(split[2]);
                        var sampleSet = int.Parse(split[3]);
                        var sampleIndex = int.Parse(split[4]);
                        var volume = int.Parse(split[5]);
                        var uninherited = int.Parse(split[6]);
                        var effects = int.Parse(split[7]);

                        if(uninherited == 1)
                        {
                            chart.BpmChanges.Add(new BpmChange(offset, bpm));
                        }
                    }
                }
                else if(lines[0].StartsWith("HitObjects"))
                {
                    for(int i=1; i<lines.Length; i++)
                    {
                        var line = lines[i];
                        var split = line.Split(',');
                        if(split.Length < 6) continue;
                        var x = int.Parse(split[0]);
                        var y = int.Parse(split[1]);
                        float time = float.Parse(split[2]) / 1000f;
                        var type = int.Parse(split[3]);
                        var hitsound = int.Parse(split[4]);
                        var objectParams = "";
                        for (int j = 5; j < split.Length; j++)
                        {
                            if(j != 5) objectParams += ",";
                            objectParams += split[j];
                        }

                        // TODO: Remove the hard-coded lanecount if we ever support non-4k
                        var laneCount = 4;
                        int lane = (int)Math.Clamp(Math.Floor(x * laneCount / 512f), 0, laneCount - 1);
                        var offset = chart.GetOffsetFromTime(time);

                        switch(type)
                        {
                            case 1:
                                chart.Notes.Add(new Note(offset, lane, NoteType.Normal));
                                break;
                            case 128:
                                var endTime = float.Parse(objectParams.Split(':')[0]) / 1000f;
                                var endOffset = chart.GetOffsetFromTime(endTime);
                                var note = new Note(offset, lane, 0, endOffset - offset);
                                break;
                        }
                    }
                }

            }

            song.Charts.Add(chart);
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
