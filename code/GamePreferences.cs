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

    public float ScrollSpeedMultiplier { get; set; } = 1f;
}
