using System.Text.RegularExpressions;
using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

namespace Rhythm4K;

public static class SongBuilder
{
    public static async Task<BeatmapSet> Load( string path, BaseFileSystem fileSystem = null )
    {
        if ( fileSystem is null ) fileSystem = FileSystem.Data;
        var files = fileSystem.FindFile( path ).ToList();

        // Try to load baked chart
        foreach ( var file in files )
        {
            if ( file.EndsWith( ".r4k" ) )
            {
                var jsonString = await fileSystem.ReadAllTextAsync( path + "/" + file );
                var set = Json.Deserialize<BeatmapSet>( jsonString );
                if ( set is null ) continue;
                if ( BeatmapSet.LatestVersion != set.Version ) continue;
                // continue;
                // Log.Info( $"Loaded {set.Name} from {file}" );
                return set;
            }
        }

        // Try each of the chart loaders
        var loaders = TypeLibrary.GetTypes<IChartLoader>();
        foreach ( var loaderType in loaders )
        {
            if ( loaderType.IsInterface ) continue;
            var loader = TypeLibrary.Create<IChartLoader>( loaderType.TargetType );
            if ( loader.CanLoad( files ) )
            {
                return await loader.Load( path, fileSystem );
            }
        }

        // Return null
        return null;
    }

    // Stepmania Chart Loading
    public static BeatmapSet LoadFromSM( string file )
    {
        if ( !file.EndsWith( ".sm" ) ) return new BeatmapSet();

        string text = FileSystem.Mounted.ReadAllText( file );
        string[] properties = text.Split( '#' );
        BeatmapSet beatmapSet = new BeatmapSet();
        List<BpmChange> bpmChanges = new List<BpmChange>();
        beatmapSet.Beatmaps = new List<Beatmap>();
        string charterName = "";

        string value;
        foreach ( string prop in properties )
        {
            value = SMGetValue( prop, "TITLE" );
            if ( value != "" )
            {
                beatmapSet.Name = value;
                continue;
            }
            value = SMGetValue( prop, "ARTIST" );
            if ( value != "" )
            {
                beatmapSet.Artist = value;
                continue;
            }
            value = SMGetValue( prop, "CREDIT" );
            if ( value != "" )
            {
                charterName = value;
                continue;
            }
            // value = SMGetValue(prop, "MUSIC");
            // if(value != "")
            // {
            //     beatmapSet.Sound = value;
            //     continue;
            // }
            value = SMGetValue( prop, "BPMS" );
            if ( value != "" )
            {
                string[] changes = value.Trim().Split( ',' );
                foreach ( string change in changes )
                {
                    var vals = change.Split( '=' );
                    bpmChanges.Add( new BpmChange( float.Parse( vals[0] ), float.Parse( vals[1] ) ) );
                }
                // beatmapSet.BPM = bpmChanges[0].BPM;
                continue;
            }
            value = SMGetValue( prop, "OFFSET" );
            if ( value != "" )
            {
                // beatmapSet.Offset = float.Parse( value );
                continue;
            }
            value = SMGetValue( prop, "SAMPLESTART" );
            if ( value != "" )
            {
                // beatmapSet.SampleStart = float.Parse( value );
                continue;
            }
            value = SMGetValue( prop, "SAMPLELENGTH" );
            if ( value != "" )
            {
                // beatmapSet.SampleLength = float.Parse( value );
                continue;
            }
            value = SMGetValue( prop, "NOTES" );
            if ( value != "" )
            {
                string[] noteSplit = value.Split( '\n' );

                // If the chart is a valid single player chart
                if ( noteSplit[1].Contains( "-single:" ) )
                {
                    float[] holding = { -1, -1, -1, -1 };
                    Beatmap beatmap = new Beatmap();
                    beatmap.Notes = new List<Note>();
                    beatmap.BpmChanges = bpmChanges;
                    beatmap.Name = noteSplit[3].Trim().Replace( ":", "" );
                    beatmap.Difficulty = float.Parse( noteSplit[4].Trim().Replace( ":", "" ) );
                    beatmap.DifficultyName = noteSplit[3].Trim().Replace( ":", "" );
                    beatmap.Charter = charterName;
                    beatmap.FilePath = file;

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
                                    case 'M':
                                        beatmap.Notes.Add( new Note( time, i, NoteType.Mine ) );
                                        break;
                                }
                            }
                            beat++;
                        }
                        measure++;
                    }
                    beatmapSet.Beatmaps.Add( beatmap );
                }
                continue;
            }
        }

        return beatmapSet;
    }

    private static string SMGetValue( string text, string tag )
    {
        var split = text.Split( ':' );
        if ( split.Length >= 2 )
        {
            var tagToCheck = split[0].Replace( "#", "" ).Replace( ";", "" );
            if ( tagToCheck.ToLower() == tag.ToLower() )
            {
                if ( split.Length > 2 )
                {
                    return text.Remove( 0, 6 );
                }
                return split[1].Replace( ";", "" ).Trim();
            }
        }
        return "";
    }

}
