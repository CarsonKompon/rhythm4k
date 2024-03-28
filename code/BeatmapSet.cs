using Sandbox;
using System.Threading.Tasks;

namespace Rhythm4K;

public class BeatmapSet
{
    public string ChartPath { get; set; }
    public string CoverArt { get; set; }
    public string OsuId { get; set; }

    public string Name { get; set; }
    public string Artist { get; set; }
    public float Offset { get; set; }
    public float SampleStart { get; set; }
    public float SampleLength { get; set; }
    public float BPM { get; set; }
    public List<Beatmap> Beatmaps { get; set; }

    public string AudioFilename;

    public string GetFullPath()
    {
        return $"beatmaps/{ChartPath}/";
    }

    public static List<BeatmapSet> All { get; set; } = new();
    public static int BeatmapsToLoad { get; private set; } = 0;

    public static async Task LoadAll()
    {
        if ( FileSystem.Data.DirectoryExists( "beatmaps" ) )
        {
            var folders = FileSystem.Data.FindDirectory( "beatmaps" );
            BeatmapsToLoad = folders.Count();
            foreach ( var directory in folders )
            {
                await SongBuilder.LoadFromOsuFolder( directory );
            }
        }
    }
}