using Sandbox;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Rhythm4K;

public class BeatmapSet
{
    public static int LatestVersion = 1;

    public int Version { get; set; }
    public string Path { get; set; }
    public string CoverArt { get; set; }

    public string Name { get; set; }
    public string Artist { get; set; }
    public List<Beatmap> Beatmaps { get; set; }

    public DateTime DateAdded { get; set; }
    public Dictionary<string, object> Metadata { get; set; }

    public static List<BeatmapSet> All { get; set; } = new();
    public static int BeatmapsToLoad { get; private set; } = 0;

    public static async Task LoadAll()
    {
        BeatmapsToLoad = 0;
        All.Clear();
        if ( FileSystem.Data.DirectoryExists( "beatmaps" ) )
        {
            await LoadFolder( "beatmaps" );
        }
    }

    public static async Task<BeatmapSet> Load( string path )
    {
        var set = await SongBuilder.Load( path );
        if ( set is null ) return null;
        All.Add( set );
        return set;
    }

    private static async Task LoadFolder( string path )
    {
        var folders = FileSystem.Data.FindDirectory( path );
        BeatmapsToLoad += folders.Count();
        foreach ( var directory in folders )
        {
            var set = await Load( path + "/" + directory );
            if ( set is null )
            {
                await LoadFolder( path + "/" + directory );
            }
        }
    }

    public void Save( string path = "" )
    {
        if ( string.IsNullOrEmpty( path ) )
        {
            path = Path + "/beatmap.r4k";
        }
        if ( !path.EndsWith( ".r4k" ) ) path += ".r4k";
        FileSystem.Data.WriteJson( path, this );
    }

    [JsonIgnore] Texture _coverTexture = null;
    public Texture GetCoverTexture()
    {
        if ( _coverTexture is not null ) return _coverTexture;
        if ( string.IsNullOrEmpty( CoverArt ) ) return null;
        if ( CoverArt.StartsWith( "http" ) )
        {
            _coverTexture = Texture.Load( CoverArt );
            return _coverTexture;
        }
        _coverTexture = Texture.LoadAsync( FileSystem.Data, CoverArt ).Result;
        return _coverTexture;
    }
}