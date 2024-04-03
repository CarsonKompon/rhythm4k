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


    public bool Downscroll { get; set; } = true;
    public float ScrollSpeedMultiplier { get; set; } = 1f;
    public float AudioLatency { get; set; } = 0f;

    public int GameStyle { get; set; } = 1;
    public NoteStyle NoteStyle { get; set; } = 0;

    public bool BackgroundEffects { get; set; } = true;
    public bool HitEffects { get; set; } = true;
    public bool LightUpLanes { get; set; } = true;
}
