using System;
using Sandbox;

namespace Rhythm4K;

public sealed class Lane : Component
{
	[Property] public ModelRenderer LaneModel { get; set; }
	[Property] public GameObject StartPosition { get; set; }
	[Property] public GameObject EndPosition { get; set; }
	[Property] public LaneKeyScreen LaneKeyScreen { get; set; }

	public int LaneIndex { get; set; }
	public string LaneKey = "";
	Color StartingColor = Color.White;

	protected override void OnStart()
	{
		StartingColor = LaneModel.Tint;
	}

	public void SetLane( int index )
	{
		LaneIndex = index;
		var laneCount = Beatmap.Loaded.Lanes;
		LaneKey = $"{laneCount}KeyButton{index + 1}";
		LaneKeyScreen.InputName = LaneKey;
	}

	protected override void OnUpdate()
	{
		if ( Input.Down( LaneKey ) )
		{
			LaneModel.Tint = new Color( 0xFF333333 ).WithAlpha( StartingColor.a + MathF.Sin( Time.Now * 20f ) / 50f );
		}

		LaneModel.Tint = Color.Lerp( LaneModel.Tint, StartingColor, Time.Delta * 20f );
	}
}