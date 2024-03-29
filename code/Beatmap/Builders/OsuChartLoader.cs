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
            var beatmap = LoadChart( file );
            if ( beatmap is null ) continue;
            if ( beatmap.Notes.Count == 0 ) continue;
            if ( set.Beatmaps.Count == 1 )
            {

            }
            set.Beatmaps.Add( beatmap );
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
        Beatmap beatmap = new();
        return beatmap;
    }
}