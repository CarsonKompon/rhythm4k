using Sandbox;

public sealed class MusicPlayer : Component
{
	[Property] SoundEvent Sound { get; set; }
	SoundHandle Handle { get; set; }

	protected override void OnUpdate()
	{
		if ( Handle is null || Handle.IsStopped )
		{
			Handle = Sandbox.Sound.Play( Sound );
		}
	}
}