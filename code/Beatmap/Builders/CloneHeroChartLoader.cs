using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox;

namespace Rhythm4K;

public class CloneHeroChartLoader : IChartLoader
{
    public CloneHeroChartLoader() { }

    public bool CanLoad( List<string> files )
    {
        foreach ( var file in files )
        {
            if ( file.EndsWith( ".chart" ) )
            {
                return true;
            }
        }
        return false;
    }

    public async Task<BeatmapSet> Load( string path, BaseFileSystem fileSystem = null )
    {
        return null;

        if ( fileSystem is null ) fileSystem = FileSystem.Data;
        BeatmapSet set = new();
        set.Path = path;
        set.Beatmaps = new();
        set.DateAdded = DateTime.Now;
        set.Version = BeatmapSet.LatestVersion;

        var iniFile = fileSystem.FindFile( path, "*.ini" ).FirstOrDefault();
        var iniString = await fileSystem.ReadAllTextAsync( path + "/" + iniFile );
        var ini = ParseIni( iniString );

        string charterName = ini["Charter"] ?? "";
        float previewStartTime = float.Parse( ini["PreviewStartTime"] ?? "0" ) / 1000f;
        set.Name = ini["Name"];
        set.Artist = ini["Artist"];


        var chartFiles = fileSystem.FindFile( path, "*.chart" );
        var chartFile = chartFiles.FirstOrDefault();
        if ( chartFile is not null )
        {
            foreach ( var newChartFile in chartFiles )
            {
                if ( newChartFile == "notes.chart" )
                {
                    chartFile = newChartFile;
                    break;
                }
                if ( newChartFile.Contains( "[Y]" ) || newChartFile.Contains( "[F]" ) )
                {
                    chartFile = newChartFile;
                    break;
                }
            }
        }
        string text = fileSystem.ReadAllText( path + "/" + chartFile );
        if ( string.IsNullOrEmpty( text ) ) return null;

        // TODO: Parse clone hero charts. Seems like a lot of work at the moment.

        return null;
    }

    Dictionary<string, string> ParseIni( string text )
    {
        Dictionary<string, string> result = new();
        string[] lines = text.Split( '\n' );
        foreach ( string line in lines )
        {
            string[] split = line.Split( '=' );
            if ( split.Length < 2 ) continue;
            string str = "";
            for ( int i = 1; i < split.Length; i++ )
            {
                str += split[i];
                if ( i < split.Length - 1 ) str += "=";
            }
            result[split[0].Trim()] = str.Trim();
        }
        return result;
    }
}