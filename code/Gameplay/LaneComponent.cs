using System;
using Sandbox;

namespace Rhythm4K;

public sealed class Lane : Component
{
	[Property] public ModelRenderer LaneModel { get; set; }
	[Property] public GameObject StartPosition { get; set; }
	[Property] public GameObject EndPosition { get; set; }
	[Property] public LaneKeyScreen LaneKeyScreen { get; set; }
	[Property] public GameObject Receptor { get; set; }

	public int LaneIndex { get; set; }
	public string LaneKey = "";
	Color StartingColor = Color.White;
	GameObject BurstPrefab;
	ModelRenderer LaneHitHighlight;

	protected override void OnStart()
	{
		var theme = GamePreferences.Settings.GetNoteTheme();
		var receptor = SceneUtility.GetPrefabScene( theme.ReceptorPrefab ).Clone( Receptor.Transform.World.Position );
		BurstPrefab = SceneUtility.GetPrefabScene( theme.BurstPrefab );
		LaneHitHighlight = receptor.Children.Where( x => x.Tags.Has( "highlight" ) ).FirstOrDefault()?.Components?.Get<ModelRenderer>();

		StartingColor = GamePreferences.Settings.GetLaneColor( Beatmap.Loaded.Lanes + "K" + (LaneIndex + 1) ).WithAlpha( 0.7f );
		Log.Info( $"{LaneIndex} {StartingColor}" );
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
		if ( Input.Down( LaneKey ) && GamePreferences.Settings.LightUpLanes )
		{
			LaneModel.Tint = Color.Lerp( StartingColor, Color.White, 0.4f ).WithAlpha( StartingColor.a + MathF.Sin( Time.Now * 20f ) / 50f );
		}

		LaneModel.Tint = Color.Lerp( LaneModel.Tint, StartingColor, Time.Delta * 20f );
		LaneHitHighlight.Tint = Color.Lerp( LaneHitHighlight.Tint, LaneHitHighlight.Tint.WithAlpha( 0f ), Time.Delta * 5f );
		// Log.Info( LaneHitHighlight.Tint );
	}

	public void HighlightHit()
	{
		if ( !GamePreferences.Settings.HitEffects ) return;
		LaneHitHighlight.Tint = Color.White;
		var pos = EndPosition.Transform.World.Position;
		if ( GamePreferences.Settings.GameStyle == 1 )
		{
			pos += Vector3.Up * 20f + Vector3.Backward * 20f;
		}
		BurstPrefab.Clone( pos );
	}
}