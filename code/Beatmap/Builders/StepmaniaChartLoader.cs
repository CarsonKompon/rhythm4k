using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox;

namespace Rhythm4K;

public class StepmaniaChartLoader : IChartLoader
{
    public StepmaniaChartLoader() { }

    public bool CanLoad( List<string> files )
    {
        foreach ( var file in files )
        {
            if ( file.EndsWith( ".sm" ) )
            {
                return true;
            }
        }
        return false;
    }

    public async Task<BeatmapSet> Load( string path, BaseFileSystem fileSystem = null )
    {
        if ( fileSystem is null ) fileSystem = FileSystem.Data;
        BeatmapSet set = new();
        set.Path = path;
        set.Beatmaps = new();
        set.DateAdded = DateTime.Now;
        set.Version = BeatmapSet.LatestVersion;

        var file = fileSystem.FindFile( path, "*.sm" ).FirstOrDefault();
        string text = fileSystem.ReadAllText( path + "/" + file );
        if ( string.IsNullOrEmpty( text ) ) return null;
        string[] properties = text.Split( '#' );
        string charterName = "";
        string audioFile = "";
        float offset = 0f;
        float sampleStart = 0f;
        List<BpmChange> bpmChanges = new();

        foreach ( string prop in properties )
        {
            string tag = GetTag( prop );
            if ( string.IsNullOrEmpty( tag ) ) continue;
            string value = GetValue( prop );
            if ( string.IsNullOrEmpty( value ) ) continue;
            switch ( tag )
            {
                case "TITLE":
                    set.Name = value;
                    break;
                case "ARTIST":
                    set.Artist = value;
                    break;
                case "CREDIT":
                    charterName = value;
                    break;
                case "BANNER":
                    set.CoverArt = "/" + path + "/" + value;
                    break;
                case "MUSIC":
                    audioFile = value;
                    break;
                case "BPMS":
                    string[] changes = value.Trim().Split( ',' );
                    foreach ( string change in changes )
                    {
                        var vals = change.Split( '=' );
                        bpmChanges.Add( new BpmChange( float.Parse( vals[0] ), float.Parse( vals[1] ) ) );
                    }
                    continue;
                case "OFFSET":
                    offset = float.Parse( value );
                    break;
                case "SAMPLESTART":
                    sampleStart = float.Parse( value );
                    break;
                case "NOTES":
                    string[] noteSplit = value.Split( '\n' );

                    // If the chart is a valid singleplayer chart
                    if ( noteSplit[1].Contains( "-single:" ) )
                    {
                        float[] holding = { -1, -1, -1, -1 };
                        Beatmap beatmap = new();
                        beatmap.Notes = new();
                        beatmap.BpmChanges = bpmChanges;
                        beatmap.Name = noteSplit[3].Trim().Replace( ":", "" );
                        beatmap.Difficulty = float.Parse( noteSplit[4].Trim().Replace( ":", "" ) );
                        beatmap.DifficultyName = noteSplit[3].Trim().Replace( ":", "" );
                        beatmap.Charter = charterName;
                        beatmap.FilePath = file;
                        beatmap.AudioFilename = audioFile;
                        beatmap.Offset = offset;
                        beatmap.SampleStart = sampleStart;
                        beatmap.Lanes = 4;

                        // Remove header and useless characters from the string
                        for ( int i = 0; i < 6; i++ )
                        {
                            value = value.Substring( value.IndexOf( '\n' ) + 1 );
                        }
                        value = Regex.Replace( value, "//.*", "" );
                        value = Regex.Replace( value, "\\;", "" );

                        // Re-split into arrays that contain each measure
                        noteSplit = Regex.Split( value, "\\," );
                        int measure = 0;
                        foreach ( string note in noteSplit )
                        {
                            var lines = note.Trim().Split( '\n' );
                            var division = 1f / (lines.Length == 96 ? 192 : lines.Length);
                            int beat = 0;

                            // Loop through each line in the measure
                            foreach ( var line in lines )
                            {
                                float time = (measure * 1000f) + (beat * division * 1000f);
                                for ( int i = 0; i < 4; i++ )
                                {
                                    switch ( line[i] )
                                    {
                                        case '1':
                                            beatmap.Notes.Add( new Note( time, i, NoteType.Normal ) );
                                            break;
                                        case '2':
                                            holding[i] = time;
                                            break;
                                        case '3':
                                            if ( holding[i] != -1 )
                                            {
                                                beatmap.Notes.Add( new Note( holding[i], i, 0, time - holding[i] ) );
                                                holding[i] = -1;
                                            }
                                            break;
                                            // TODO: Support Mines?
                                            // TODO: Option to disable mines
                                            // case 'M':
                                            //     beatmap.Notes.Add(new Note(time, i, NoteType.Mine));
                                            //     break;
                                    }
                                }
                                beat++;
                            }
                            measure++;
                        }

                        if ( string.IsNullOrEmpty( set.CoverArt ) )
                        {
                            foreach ( var imageFile in fileSystem.FindFile( path, "*" ) )
                            {
                                var lastDot = imageFile.LastIndexOf( '.' );
                                if ( lastDot == -1 ) continue;
                                var fileNameWithoutExtension = imageFile.Substring( 0, lastDot );
                                if ( fileNameWithoutExtension.EndsWith( "-bn" ) )
                                {
                                    set.CoverArt = "/" + path + "/" + imageFile;
                                    break;
                                }
                            }
                        }

                        set.Beatmaps.Add( beatmap );
                    }
                    break;
            }
        }

        if ( set.Beatmaps.Count == 0 )
        {
            return null;
        }

        // Calculate baked values
        for ( int i = 0; i < set.Beatmaps.Count; i++ )
        {
            if ( set.Beatmaps[i] is null ) continue;
            set.Beatmaps[i].BakeValues();
            var lastNote = set.Beatmaps[i].Notes.OrderBy( x => x.BakedTime + x.BakedLength ).LastOrDefault();
            set.Beatmaps[i].Length = lastNote.BakedTime + lastNote.BakedLength;
        }

        set.Save( fileSystem );

        await GameTask.Delay( 1 );

        return set;
    }

    private static string GetValue( string text )
    {
        var split = text.Split( ':' );
        if ( split.Length >= 2 )
        {
            if ( split.Length > 2 )
            {
                return text.Remove( 0, 6 );
            }
            return split[1].Replace( ";", "" ).Trim();
        }
        return "";
    }

    private static string GetTag( string text )
    {
        var split = text.Split( ':' );
        if ( split.Length >= 2 )
        {
            return split[0].Replace( "#", "" ).Replace( ";", "" );
        }
        return "";
    }

    // Beatmap LoadChart( string path, BeatmapSet set, BaseFileSystem fileSystem = null )
    // {
    //     if ( fileSystem is null ) fileSystem = FileSystem.Data;
    //     string text = fileSystem.ReadAllText( path );
    //     if ( string.IsNullOrEmpty( text ) ) return null;
    //     string[] properties = text.Split( '#' );

    //     Beatmap beatmap = new();
    //     beatmap.FilePath = path;
    //     beatmap.FileSystem = fileSystem;

    //     foreach ( var section in sections )
    //     {
    //         string[] lines = section.Split( '\n' );
    //         if ( lines[0].StartsWith( "General" ) )
    //         {
    //             foreach ( var line in lines )
    //             {
    //                 var split = line.Split( ':' );
    //                 if ( split.Length < 2 ) continue;
    //                 var key = split[0].Trim();
    //                 var value = split[1].Trim();
    //                 switch ( key )
    //                 {
    //                     case "AudioFilename":
    //                         beatmap.AudioFilename = value;
    //                         break;
    //                     case "PreviewTime":
    //                         beatmap.SampleStart = float.Parse( value ) / 1000f;
    //                         beatmap.SampleLength = 6f;
    //                         break;
    //                     case "Mode":
    //                         if ( value != "3" )
    //                         {
    //                             Log.Warning( "Osu chart " + path + " is not an osu!mania chart, skipping..." );
    //                             return null;
    //                         }
    //                         break;
    //                 }
    //             }
    //         }
    //         else if ( lines[0].StartsWith( "Metadata" ) )
    //         {
    //             foreach ( var line in lines )
    //             {
    //                 var split = line.Split( ':' );
    //                 if ( split.Length < 2 ) continue;
    //                 var key = split[0].Trim();
    //                 var value = split[1].Trim();
    //                 switch ( key )
    //                 {
    //                     case "TitleUnicode":
    //                         beatmap.Name = value;
    //                         break;
    //                     case "ArtistUnicode":
    //                         beatmap.Artist = value;
    //                         break;
    //                     case "Creator":
    //                         beatmap.Charter = value;
    //                         break;
    //                     case "Version":
    //                         beatmap.DifficultyName = value;
    //                         break;
    //                 }
    //             }
    //         }
    //         else if ( lines[0].StartsWith( "Difficulty" ) )
    //         {
    //             foreach ( var line in lines )
    //             {
    //                 var split = line.Split( ':' );
    //                 if ( split.Length < 2 ) continue;
    //                 var key = split[0].Trim();
    //                 var value = split[1].Trim();
    //                 switch ( key )
    //                 {
    //                     case "OverallDifficulty":
    //                         beatmap.Difficulty = float.Parse( value );
    //                         break;
    //                     case "CircleSize":
    //                         beatmap.Lanes = (int)MathF.Round( float.Parse( value ) );
    //                         break;
    //                     case "SliderMultiplier":
    //                         beatmap.ScrollSpeed = float.Parse( value );
    //                         break;
    //                 }
    //             }
    //         }
    //         else if ( lines[0].StartsWith( "TimingPoints" ) )
    //         {
    //             foreach ( var line in lines )
    //             {
    //                 var split = line.Split( ',' );
    //                 if ( split.Length < 8 ) continue;
    //                 var offset = float.Parse( split[0] );
    //                 var beatLength = float.Parse( split[1] );
    //                 if ( beatLength <= 0 ) continue;
    //                 var bpm = 60000f / beatLength;
    //                 var meter = int.Parse( split[2] );
    //                 var sampleSet = int.Parse( split[3] );
    //                 var sampleIndex = int.Parse( split[4] );
    //                 var volume = int.Parse( split[5] );
    //                 var uninherited = int.Parse( split[6] );
    //                 var effects = int.Parse( split[7] );

    //                 if ( uninherited == 1 )
    //                 {
    //                     beatmap.BpmChanges.Add( new BpmChange( offset, bpm ) );
    //                 }
    //             }
    //         }
    //         else if ( lines[0].StartsWith( "HitObjects" ) )
    //         {
    //             foreach ( var line in lines )
    //             {
    //                 var split = line.Split( ',' );
    //                 if ( split.Length < 6 ) continue;
    //                 var x = int.Parse( split[0] );
    //                 var y = int.Parse( split[1] );
    //                 float time = float.Parse( split[2] ) / 1000f;
    //                 var type = int.Parse( split[3] );
    //                 var hitSound = int.Parse( split[4] );
    //                 var objectParams = "";
    //                 for ( int j = 5; j < split.Length; j++ )
    //                 {
    //                     if ( j != 5 ) objectParams += ",";
    //                     objectParams += split[j];
    //                 }

    //                 int lane = (int)Math.Clamp( Math.Floor( x * beatmap.Lanes / 512f ), 0, beatmap.Lanes - 1 );

    //                 switch ( type )
    //                 {
    //                     case 128:
    //                         var endTime = float.Parse( objectParams.Split( ':' )[0] ) / 1000f;
    //                         var note = new Note( time, lane, 0, endTime - time, true );
    //                         beatmap.Notes.Add( note );
    //                         break;
    //                     default:
    //                         beatmap.Notes.Add( new Note( time, lane, NoteType.Normal, 0, true ) );
    //                         break;
    //                 }
    //             }
    //             beatmap.Notes = beatmap.Notes.OrderBy( o => o.BakedTime ).ToList();
    //             var lastNote = beatmap.Notes.LastOrDefault();
    //             beatmap.Length = lastNote.BakedTime + lastNote.BakedLength;
    //         }
    //     }

    //     return beatmap;
    // }
}