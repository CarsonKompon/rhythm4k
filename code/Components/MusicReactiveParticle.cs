using System;
using Sandbox;

namespace Rhythm4K;

public class MusicReactiveParticle : Component
{
    [Property] GameObject PlayerObject { get; set; }
    [Property] List<ParticleEffect> Particles { get; set; }

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
        foreach ( var particle in Particles )
        {
            particle.TimeScale = 0.25f + Player.Energy / 1.5f;
        }
    }
}