using Sandbox;
using System;
using System.Threading.Tasks;

namespace Rhythm4K;

public interface IChartLoader
{
    public bool CanLoad( List<string> files );
    public Task<BeatmapSet> Load( string path );
}

[AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
public class ChartLoaderAttribute : Attribute
{
    public string FileExtension { get; }

    public ChartLoaderAttribute( string fileExtension )
    {
        FileExtension = fileExtension;
    }
}