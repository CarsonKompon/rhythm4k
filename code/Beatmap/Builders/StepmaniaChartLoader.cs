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
}