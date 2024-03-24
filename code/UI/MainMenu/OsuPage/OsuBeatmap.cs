using Sandbox;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Rhythm4K.Osu;

public class OsuBeatmap
{
    [JsonPropertyName( "id" )]
    public long Id { get; set; }

    [JsonPropertyName( "mode" )]
    public object Mode { get; set; }

    [JsonPropertyName( "mode_int" )]
    public object ModeInt { get; set; }

    [JsonPropertyName( "difficulty" )]
    public object Difficulty { get; set; }

    [JsonPropertyName( "version" )]
    public object Version { get; set; }

    [JsonPropertyName( "total_length" )]
    public object TotalLength { get; set; }

    [JsonPropertyName( "cs" )]
    public object CS { get; set; }

    [JsonPropertyName( "drain" )]
    public object Drain { get; set; }

    [JsonPropertyName( "accuracy" )]
    public object Accuracy { get; set; }

    [JsonPropertyName( "ar" )]
    public object AR { get; set; }

    [JsonPropertyName( "count_circles" )]
    public object CircleCount { get; set; }

    [JsonPropertyName( "count_sliders" )]
    public object SliderCount { get; set; }

    [JsonPropertyName( "count_spinners" )]
    public object SpinnerCount { get; set; }

    [JsonPropertyName( "count_total" )]
    public object TotalCount { get; set; }

    [JsonPropertyName( "last_updated" )]
    public object LastUpdated { get; set; }

    [JsonPropertyName( "status" )]
    public object Status { get; set; }

    [JsonPropertyName( "url" )]
    public object Url { get; set; }

}