using Sandbox;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Rhythm4K.Osu;

public class OsuSearch
{
    [JsonPropertyName( "beatmaps" )]
    public List<OsuBeatmapSet> BeatmapSets { get; set; }

    [JsonPropertyName( "max_page" )]
    public int MaxPage { get; set; }

}