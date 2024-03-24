using Sandbox;

public sealed class FixTransparency : Component
{
	[Property] ModelRenderer modelRenderer { get; set; }

	protected override void OnUpdate()
	{
		if ( !modelRenderer.IsValid() ) return;
		modelRenderer.SceneObject.Flags.WantsFrameBufferCopy = true;
		modelRenderer.SceneObject.Flags.IsTranslucent = true;
		modelRenderer.SceneObject.Flags.IsOpaque = false;
	}
}