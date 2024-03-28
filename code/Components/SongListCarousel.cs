using System;
using System.Reflection.Metadata.Ecma335;
using Sandbox;
using Sandbox.UI;

namespace Rhythm4K;

public sealed class SongListCarousel : Component
{
	public static SongListCarousel Instance { get; private set; }

	[Property] GameObject SongPanelPrefab { get; set; }
	[Property] public int SongPanelCount { get; set; } = 16;
	[Property] float SongXSpread { get; set; } = 100f;
	[Property] float SongYSpread { get; set; } = 100f;
	[Property] float TargetAngle { get; set; } = 0f;
	public float AngleOffset { get; set; } = 0f;
	public float LastOffset { get; set; } = 0f;
	bool firstLoad = true;

	public int SelectedIndex
	{
		get => _selectedIndex;
		set
		{
			_selectedIndex = value;
			var panelIndex = value;
			var all = BeatmapSet.All;
			int totalAm = all.Count();
			if ( totalAm <= 0 ) return;
			while ( panelIndex < 0 )
			{
				panelIndex += SongPanelCount;
			}
			for ( int i = panelIndex - 4; i <= panelIndex + 4; i++ )
			{
				int offset = i - panelIndex;
				var panel = SongPanels[(i + SongPanelCount * 2) % SongPanelCount];
				var panelScript = panel.Components.Get<SongListPanel>();
				int ind = _selectedIndex;
				while ( ind < 0 )
				{
					ind += totalAm;
				}
				panelScript.Index = (ind + totalAm + offset) % totalAm;
			}
			SongListInfoPanel.SelectedIndex = -1;
		}
	}
	private int _selectedIndex = 0;
	float CurrentAngle { get; set; } = 0f;
	List<GameObject> SongPanels { get; set; } = new();

	int Moving { get; set; } = 0;
	TimeSince TimeSinceHeld { get; set; } = 0f;
	TimeSince TimeSinceMoved { get; set; } = 0f;
	WorldInput worldInput;
	public TimeSince Timer { get; set; } = 0f;

	protected override void OnAwake()
	{
		Instance = this;
		TimeSinceMoved = 0f;

		float angle = 0f;
		for ( int i = 0; i < SongPanelCount; i++ )
		{
			var panel = SongPanelPrefab.Clone( Transform.World );
			panel.SetParent( GameObject );
			panel.Transform.LocalPosition = new Vector3( MathF.Cos( angle ) * SongXSpread, 0f, MathF.Sin( angle ) * SongYSpread );
			angle -= MathF.PI * 2f / (float)SongPanelCount;
			SongPanels.Add( panel );
			var panelScript = panel.Components.Get<SongListPanel>();
		}
	}

	protected override void OnStart()
	{
		SelectedIndex = 0;

		worldInput = new WorldInput();
		worldInput.Enabled = true;
	}

	protected override void OnDestroy()
	{
		worldInput.Enabled = false;
	}

	protected override void OnUpdate()
	{
		TargetAngle = SelectedIndex * MathF.PI * 2f / (float)SongPanelCount;
		CurrentAngle = MathX.LerpDegrees( CurrentAngle, TargetAngle + AngleOffset, Time.Delta * 10f );

		if ( worldInput is not null )
		{
			var camRay = Scene.Camera.ScreenPixelToRay( Mouse.Position );
			worldInput.Ray = new Ray( camRay.Position, camRay.Forward );
			worldInput.MouseLeftPressed = Input.Down( "click" );
			worldInput.MouseWheel = -Input.MouseWheel;
		}

		var mouseWheel = Input.MouseWheel.y;
		if ( mouseWheel != 0 && worldInput.Hovered is null )
		{
			Sound.Play( "ui_hover" );
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
			panel.Transform.LocalRotation = Rotation.From( 0f, 90f, 0f );
			angle -= MathF.PI * 2f / (float)SongPanelCount;
		}
	}

	public void LockOffset()
	{
		var indexIncrement = (int)MathF.Round( AngleOffset / (MathF.PI * 2f / (float)SongPanelCount) );
		SelectedIndex += indexIncrement;
		AngleOffset = 0f;
	}

	public void Refresh()
	{
		if ( firstLoad )
		{
			SelectedIndex = 0;
			firstLoad = false;
		}
		Timer = 0f;
	}
}