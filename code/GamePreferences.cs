using System.Text.Json.Serialization;
using Sandbox;

namespace Rhythm4K;

public static class GamePreferences
{

    public static RhythmSettings Settings
    {
        get
        {
            if ( _settings is null )
            {
                var file = "/settings.json";
                _settings = FileSystem.Data.ReadJson( file, new RhythmSettings() );
            }
            return _settings;
        }
    }
    static RhythmSettings _settings;

    public static void SaveSettings()
    {
        var file = "/settings.json";
        FileSystem.Data.WriteJson( file, Settings );
    }

}

public class RhythmSettings
{
    public float MasterVolume { get; set; } = 50f;
    public float MusicVolume { get; set; } = 80f;
    public float SoundVolume { get; set; } = 80f;
    public float UnfocusedVolume { get; set; } = 40f;

    public bool Downscroll { get; set; } = true;
    public float ScrollSpeedMultiplier { get; set; } = 1f;
    public float AudioLatency { get; set; } = 0f;

    public int GameStyle { get; set; } = 1;
    public NoteStyle NoteStyle { get; set; } = 0;

    public bool BackgroundEffects { get; set; } = true;
    public bool HitEffects { get; set; } = true;
    public bool LightUpLanes { get; set; } = true;

    public Dictionary<string, Color> LaneColors { get; set; } = new();

    public bool DoneFirstTimeSetup { get; set; } = false;

    int _noteStyle = -1;
    NoteTheme _noteTheme;
    public NoteTheme GetNoteTheme()
    {
        if ( _noteStyle == (int)NoteStyle ) return _noteTheme;
        _noteStyle = (int)NoteStyle;
        _noteTheme = NoteTheme.GetFromResourceName( NoteStyle.ToString().ToLower() );
        return _noteTheme;
    }

    public Color GetLaneColor( string laneKey )
    {
        if ( LaneColors is null ) LaneColors = new();
        if ( LaneColors.ContainsKey( laneKey ) )
        {
            return LaneColors[laneKey];
        }
        return Color.Black;
    }

    public void SetLaneColor( string laneKey, Color color )
    {
        LaneColors[laneKey] = color;
    }

    [JsonIgnore]
    public Dictionary<string, float> ColorHue
    {
        get
        {
            var dict = new Dictionary<string, float>();
            foreach ( var key in LaneColors.Keys )
            {
                var hsv = LaneColors[key].ToHsv();
                dict[key] = hsv.Hue;
            }
            return dict;
        }
        set
        {
            foreach ( var key in value.Keys )
            {
                var color = GetLaneColor( key );
                var hsv = color.ToHsv();
                Log.Info( $"{key} {value[key]}" );
                hsv.WithHue( value[key] / 255f );
                SetLaneColor( key, hsv );
            }
        }
    }

    [JsonIgnore]
    public Dictionary<string, float> ColorValue
    {
        get
        {
            var dict = new Dictionary<string, float>();
            foreach ( var key in LaneColors.Keys )
            {
                var hsv = LaneColors[key].ToHsv();
                dict[key] = hsv.Value;
            }
            return dict;
        }
        set
        {
            foreach ( var key in value.Keys )
            {
                var color = GetLaneColor( key );
                var hsv = color.ToHsv();
                hsv.WithValue( value[key] );
                SetLaneColor( key, hsv );
            }
        }
    }

    public void SetLaneHue( string laneKey, float hue )
    {
        var color = GetLaneColor( laneKey );
        var hsv = color.ToHsv();
        hsv = hsv.WithHue( hue );
        SetLaneColor( laneKey, hsv );
    }

    public void SetLaneValue( string laneKey, float value )
    {
        var color = GetLaneColor( laneKey );
        var hsv = color.ToHsv();
        hsv = hsv.WithValue( value ).WithSaturation( 1f );
        SetLaneColor( laneKey, hsv );
    }

}
