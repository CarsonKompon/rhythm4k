using Sandbox;

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

    public static async void LoadAll()
    {
        if ( FileSystem.Data.DirectoryExists( "beatmaps" ) )
        {
            foreach ( var directory in FileSystem.Data.FindDirectory( "beatmaps" ) )
            {
                await SongBuilder.LoadFromOsuFolder( directory );
            }
        }
    }
}