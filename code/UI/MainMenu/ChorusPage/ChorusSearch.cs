using Sandbox;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Rhythm4K.Chorus;

public class ChorusSearch
{
    [JsonPropertyName( "found" )]
    public int Found { get; set; }

    [JsonPropertyName( "out_of" )]
    public int OutOfTotal { get; set; }

    [JsonPropertyName( "page" )]
    public int Page { get; set; }

    [JsonPropertyName( "search_time_ms" )]
    public int SearchTimeMs { get; set; }

    [JsonPropertyName( "data" )]
    public List<ChorusBeatmap> Beatmaps { get; set; }

}