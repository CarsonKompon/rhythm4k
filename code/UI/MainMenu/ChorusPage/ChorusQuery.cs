using Sandbox;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rhythm4K.Chorus;

public class ChorusQuery
{
    [JsonPropertyName( "search" )]
    public string Search { get; set; } = null;

    [JsonPropertyName( "page" )]
    public int Page { get; set; } = 1;

    [JsonPropertyName( "instrument" )]
    public string Instrument { get; set; } = null;

    [JsonPropertyName( "difficulty" )]
    public string Difficulty { get; set; } = null;

    [JsonPropertyName( "name" )]
    public ChorusSearchBy Name { get; set; } = null;

    [JsonPropertyName( "artist" )]
    public ChorusSearchBy Artist { get; set; } = null;

    [JsonPropertyName( "album" )]
    public ChorusSearchBy Album { get; set; } = null;

    [JsonPropertyName( "genre" )]
    public ChorusSearchBy Genre { get; set; } = null;

    [JsonPropertyName( "year" )]
    public ChorusSearchBy Year { get; set; } = null;

    [JsonPropertyName( "charter" )]
    public ChorusSearchBy Charter { get; set; } = null;

    [JsonPropertyName( "minLength" )]
    public float? MinLengthMinutes { get; set; } = null;

    [JsonPropertyName( "maxLength" )]
    public float? MaxLengthMinutes { get; set; } = null;

    [JsonPropertyName( "minIntensity" )]
    public float? MinIntensity { get; set; } = null;

    [JsonPropertyName( "maxIntensity" )]
    public float? MaxIntensity { get; set; } = null;

    [JsonPropertyName( "minAverageNPS" )]
    public float? MinAverageNotesPerSecond { get; set; } = null;

    [JsonPropertyName( "maxAverageNPS" )]
    public float? MaxAverageNotesPerSecond { get; set; } = null;

    [JsonPropertyName( "minMaxNPS" )]
    public float? MinMaxNotesPerSecond { get; set; } = null;

    [JsonPropertyName( "maxMaxNPS" )]
    public float? MaxMaxNotesPerSecond { get; set; } = null;

    [JsonPropertyName( "hasSoloSections" )]
    public bool? HasSoloSections { get; set; } = null;

    [JsonPropertyName( "hasForcedNotes" )]
    public bool? HasForcedNotes { get; set; } = null;

    [JsonPropertyName( "hasOpenNotes" )]
    public bool? HasOpenNotes { get; set; } = null;

    [JsonPropertyName( "hasTapNotes" )]
    public bool? HasTapNotes { get; set; } = null;

    [JsonPropertyName( "hasLyrics" )]
    public bool? HasLyrics { get; set; } = null;

    [JsonPropertyName( "hasVocals" )]
    public bool? HasVocals { get; set; } = null;

    [JsonPropertyName( "hasRollLanes" )]
    public bool? HasRollLanes { get; set; } = null;

    [JsonPropertyName( "has2xKick" )]
    public bool? Has2xKick { get; set; } = null;

    [JsonPropertyName( "hasIssues" )]
    public bool? HasIssues { get; set; } = null;

    [JsonPropertyName( "hasVideoBackground" )]
    public bool? HasVideoBackground { get; set; } = null;

    [JsonPropertyName( "modchart" )]
    public bool? IsModchart { get; set; } = null;

    public override string ToString()
    {
        // Json stringify but don't serialize null values
        var str = JsonSerializer.Serialize( this, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull } );
        JsonObject obj = JsonNode.Parse( str ) as JsonObject;
        if ( !obj.ContainsKey( "difficulty" ) )
        {
            obj["difficulty"] = null;
        }
        if ( !obj.ContainsKey( "instrument" ) )
        {
            obj["instrument"] = null;
        }
        return obj.ToString();
    }
}

public class ChorusSearchBy
{
    [JsonPropertyName( "value" )]
    public string Value { get; set; }

    [JsonPropertyName( "exact" )]
    public bool Exact { get; set; }

    [JsonPropertyName( "exclude" )]
    public bool Exclude { get; set; }
}