using Sandbox;

public sealed class RotateConstantly : Component
{
	[Property] Angles RotationSpeed { get; set; } = new Angles( 0, 0, 0 );

	protected override void OnUpdate()
	{
		Transform.LocalRotation *= RotationSpeed * Time.Delta;
	}
}