using System;
using Sandbox;

namespace Rhythm4K;

public sealed class SongListCarousel : Component
{
	public static SongListCarousel Instance { get; private set; }

	[Property] GameObject SongPanelPrefab { get; set; }
	[Property] int SongPanelCount { get; set; } = 16;
	[Property] float SongXSpread { get; set; } = 100f;
	[Property] float SongYSpread { get; set; } = 100f;
	[Property] float TargetAngle { get; set; } = 0f;
	public float AngleOffset { get; set; } = 0f;

	int SelectedIndex { get; set; } = 0;
	float CurrentAngle { get; set; } = 0f;
	List<GameObject> SongPanels { get; set; } = new();

	int Moving { get; set; } = 0;
	TimeSince TimeSinceHeld { get; set; } = 0f;
	TimeSince TimeSinceMoved { get; set; } = 0f;

	protected override void OnAwake()
	{
		Instance = this;
		TimeSinceMoved = 0f;
	}

	protected override void OnStart()
	{
		float angle = 0f;
		for ( int i = 0; i < SongPanelCount; i++ )
		{
			var panel = SongPanelPrefab.Clone( Transform.World );
			panel.SetParent( GameObject );
			panel.Transform.LocalPosition = new Vector3( MathF.Cos( angle ) * SongXSpread, 0f, MathF.Sin( angle ) * SongYSpread );
			panel.Transform.Rotation = Rotation.From( 0f, 90f, 0f );
			var panelScript = panel.Components.Get<SongListPanel>();
			panelScript.Index = i;
			angle -= MathF.PI * 2f / (float)SongPanelCount;
			SongPanels.Add( panel );
		}
	}

	protected override void OnUpdate()
	{
		TargetAngle = SelectedIndex * MathF.PI * 2f / (float)SongPanelCount;
		CurrentAngle = MathX.Lerp( CurrentAngle, TargetAngle + AngleOffset, Time.Delta * 10f );

		var mouseWheel = Input.MouseWheel.y;
		if ( mouseWheel != 0 )
		{
			SelectedIndex -= Math.Sign( mouseWheel );
		}
		else if ( Input.Pressed( "Down" ) )
		{
			TimeSinceHeld = 0f;
			Moving = 1;
		}
		else if ( Input.Pressed( "Up" ) )
		{
			TimeSinceHeld = 0f;
			Moving = -1;
		}
		else if ( Input.Released( "Down" ) || Input.Released( "Up" ) )
		{
			Moving = 0;
			TimeSinceHeld = 0f;
			TimeSinceMoved = 2f;
		}

		if ( Input.Pressed( "click" ) )
		{
			var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );
			var tr = Scene.Trace.Ray( ray.Position, ray.Forward * 10000f )
				.WithTag( "panel" )
				.Run();

			if ( tr.Hit && tr.GameObject.Components.GetInParentOrSelf<SongListPanel>() is SongListPanel panel )
			{
				panel.Grab();
			}
		}

		if ( Moving != 0 && TimeSinceMoved > ((TimeSinceHeld > 0.4f) ? 0.05f : 2f) )
		{
			Sound.Play( "ui_hover" );
			SelectedIndex += Moving;
			TimeSinceMoved = 0;
		}

		float angle = CurrentAngle;
		foreach ( var panel in SongPanels )
		{
			panel.Transform.LocalPosition = new Vector3( MathF.Cos( angle ) * SongXSpread, 0f, MathF.Sin( angle ) * SongYSpread );
			angle -= MathF.PI * 2f / (float)SongPanelCount;
		}
	}

	public void LockOffset()
	{
		var indexIncrement = (int)MathF.Round( AngleOffset / (MathF.PI * 2f / (float)SongPanelCount) );
		SelectedIndex += indexIncrement;
		AngleOffset = 0f;
	}
}