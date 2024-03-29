using Sandbox;
using System.Threading.Tasks;

namespace Rhythm4K;

public class BeatmapSet
{
    public string Path { get; set; }
    public string CoverArt { get; set; }

    public string Name { get; set; }
    public string Artist { get; set; }
    public List<Beatmap> Beatmaps { get; set; }

    public static List<BeatmapSet> All { get; set; } = new();
    public static int BeatmapsToLoad { get; private set; } = 0;

    public static async Task LoadAll()
    {
        All.Clear();
        if ( FileSystem.Data.DirectoryExists( "beatmaps" ) )
        {
            var folders = FileSystem.Data.FindDirectory( "beatmaps" );
            BeatmapsToLoad = folders.Count();
            foreach ( var directory in folders )
            {
                var set = await SongBuilder.Load( "beatmaps/" + directory );
                if ( set is null ) continue;
                All.Add( set );
            }
        }
    }
}