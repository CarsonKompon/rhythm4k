using System;
using Sandbox;

namespace Rhythm4K;

public sealed class Lane : Component
{
	[Property] public ModelRenderer LaneModel { get; set; }
	[Property] public GameObject StartPosition { get; set; }
	[Property] public GameObject EndPosition { get; set; }
	[Property] public ModelRenderer LaneHitHighlight { get; set; }
	[Property] public LaneKeyScreen LaneKeyScreen { get; set; }
	[Property] GameObject BurstPrefab { get; set; }

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
		if ( GameManager.Instance.IsPaused ) return;
		if ( Input.Down( LaneKey ) )
		{
			LaneModel.Tint = new Color( 0xFF333333 ).WithAlpha( StartingColor.a + MathF.Sin( Time.Now * 20f ) / 50f );
		}

		LaneModel.Tint = Color.Lerp( LaneModel.Tint, StartingColor, Time.Delta * 20f );
		LaneHitHighlight.Tint = Color.Lerp( LaneHitHighlight.Tint, LaneHitHighlight.Tint.WithAlpha( 0f ), Time.Delta * 5f );
		// Log.Info( LaneHitHighlight.Tint );
	}

	public void HighlightHit()
	{
		LaneHitHighlight.Tint = Color.White;
		BurstPrefab.Clone( EndPosition.Transform.World.Position );
	}
}