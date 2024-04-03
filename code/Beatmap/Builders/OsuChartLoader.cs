using System;
using System.Threading.Tasks;
using Sandbox;

namespace Rhythm4K;

public class OsuChartLoader : IChartLoader
{
    public OsuChartLoader() { }

    public bool CanLoad( List<string> files )
    {
        foreach ( var file in files )
        {
            if ( file.EndsWith( ".osu" ) )
            {
                return true;
            }
        }
        return false;
    }

    public async Task<BeatmapSet> Load( string path )
    {
        BeatmapSet set = new();
        set.Path = path;
        set.Beatmaps = new();

        var files = FileSystem.Data.FindFile( path, "*.osu" );
        foreach ( var file in files )
        {
            var beatmap = LoadChart( path + "/" + file );
            if ( beatmap is null ) continue;
            if ( beatmap.Notes.Count == 0 ) continue;
            set.Beatmaps.Add( beatmap );
            if ( set.Beatmaps.Count == 1 )
            {
                set.Name = beatmap.Name;
                set.Artist = beatmap.Artist;
            }
        }

        if ( set.Beatmaps.Count == 0 )
        {
            return null;
        }

        // Calculate baked values
        for ( int i = 0; i < set.Beatmaps.Count; i++ )
        {
            set.Beatmaps[i]?.BakeValues();
        }

        // Get Cover Art
        string coverPath = path + "/cover-image.txt";
        if ( FileSystem.Data.FileExists( coverPath ) )
        {
            set.CoverArt = await FileSystem.Data.ReadAllTextAsync( coverPath );
        }

        return set;
    }

    Beatmap LoadChart( string path )
    {
        string text = FileSystem.Data.ReadAllText( path );
        if ( string.IsNullOrEmpty( text ) ) return null;
        string[] sections = text.Split( '[' );

        Beatmap beatmap = new();
        beatmap.FilePath = path;

        foreach ( var section in sections )
        {
            string[] lines = section.Split( '\n' );
            if ( lines[0].StartsWith( "General" ) )
            {
                foreach ( var line in lines )
                {
                    var split = line.Split( ':' );
                    if ( split.Length < 2 ) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    switch ( key )
                    {
                        case "AudioFilename":
                            beatmap.AudioFilename = value;
                            break;
                        case "PreviewTime":
                            beatmap.SampleStart = float.Parse( value ) / 1000f;
                            beatmap.SampleLength = 6f;
                            break;
                        case "Mode":
                            if ( value != "3" )
                            {
                                Log.Warning( "Osu chart " + path + " is not an osu!mania chart, skipping..." );
                                return null;
                            }
                            break;
                    }
                }
            }
            else if ( lines[0].StartsWith( "Metadata" ) )
            {
                foreach ( var line in lines )
                {
                    var split = line.Split( ':' );
                    if ( split.Length < 2 ) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    switch ( key )
                    {
                        case "TitleUnicode":
                            beatmap.Name = value;
                            break;
                        case "ArtistUnicode":
                            beatmap.Artist = value;
                            break;
                        case "Creator":
                            beatmap.Charter = value;
                            break;
                        case "Version":
                            beatmap.DifficultyName = value;
                            break;
                    }
                }
            }
            else if ( lines[0].StartsWith( "Difficulty" ) )
            {
                foreach ( var line in lines )
                {
                    var split = line.Split( ':' );
                    if ( split.Length < 2 ) continue;
                    var key = split[0].Trim();
                    var value = split[1].Trim();
                    switch ( key )
                    {
                        case "OverallDifficulty":
                            beatmap.Difficulty = float.Parse( value );
                            break;
                        case "CircleSize":
                            beatmap.Lanes = (int)MathF.Round( float.Parse( value ) );
                            break;
                        case "SliderMultiplier":
                            beatmap.ScrollSpeed = float.Parse( value );
                            break;
                    }
                }
            }
            else if ( lines[0].StartsWith( "TimingPoints" ) )
            {
                foreach ( var line in lines )
                {
                    var split = line.Split( ',' );
                    if ( split.Length < 8 ) continue;
                    var offset = float.Parse( split[0] );
                    var beatLength = float.Parse( split[1] );
                    if ( beatLength <= 0 ) continue;
                    var bpm = 60000f / beatLength;
                    var meter = int.Parse( split[2] );
                    var sampleSet = int.Parse( split[3] );
                    var sampleIndex = int.Parse( split[4] );
                    var volume = int.Parse( split[5] );
                    var uninherited = int.Parse( split[6] );
                    var effects = int.Parse( split[7] );

                    if ( uninherited == 1 )
                    {
                        beatmap.BpmChanges.Add( new BpmChange( offset, bpm ) );
                    }
                }
            }
            else if ( lines[0].StartsWith( "HitObjects" ) )
            {
                foreach ( var line in lines )
                {
                    var split = line.Split( ',' );
                    if ( split.Length < 6 ) continue;
                    var x = int.Parse( split[0] );
                    var y = int.Parse( split[1] );
                    float time = float.Parse( split[2] ) / 1000f;
                    var type = int.Parse( split[3] );
                    var hitSound = int.Parse( split[4] );
                    var objectParams = "";
                    for ( int j = 5; j < split.Length; j++ )
                    {
                        if ( j != 5 ) objectParams += ",";
                        objectParams += split[j];
                    }

                    int lane = (int)Math.Clamp( Math.Floor( x * beatmap.Lanes / 512f ), 0, beatmap.Lanes - 1 );

                    switch ( type )
                    {
                        case 128:
                            var endTime = float.Parse( objectParams.Split( ':' )[0] ) / 1000f;
                            var note = new Note( time, lane, 0, endTime - time, true );
                            beatmap.Notes.Add( note );
                            break;
                        default:
                            beatmap.Notes.Add( new Note( time, lane, NoteType.Normal, 0, true ) );
                            break;
                    }
                }
                beatmap.Notes = beatmap.Notes.OrderBy( o => o.BakedTime ).ToList();
                var lastNote = beatmap.Notes.LastOrDefault();
                beatmap.Length = lastNote.BakedTime + lastNote.BakedLength;
            }
        }

        return beatmap;
    }
}