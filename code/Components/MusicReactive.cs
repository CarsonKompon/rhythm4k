using Sandbox;

namespace Rhythm4K;

public class MusicReactive : Component
{
    [Property] GameObject PlayerObject { get; set; }
    [Property] public float Sensitivity { get; set; } = 1f;

    IMusicPlayer Player;

    protected override void OnStart()
    {
        var player = PlayerObject.Components.GetInDescendantsOrSelf<IMusicPlayer>();
        if ( player is not null )
        {
            Player = player;
        }
    }

    protected override void OnUpdate()
    {
        Transform.LocalScale = 0.9f + Player.Energy / 20f;
    }
}