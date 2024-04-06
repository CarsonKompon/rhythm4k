using Sandbox;

public sealed class DestroyAfter : Component
{
	[Property] float Time { get; set; } = 1f;

	protected override void OnUpdate()
	{
		Time -= Sandbox.Time.Delta;
		if ( Time <= 0f )
		{
			GameObject.Destroy();
		}
	}
}