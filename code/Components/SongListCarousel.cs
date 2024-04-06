using System;
using System.Reflection.Metadata.Ecma335;
using Sandbox;
using Sandbox.UI;

namespace Rhythm4K;

public sealed class SongListCarousel : Component
{
	public static SongListCarousel Instance { get; private set; }

	[Property] GameObject SongPanelPrefab { get; set; }
	[Property] public int SongPanelCount { get; set; } = 32;
	[Property] float SongXSpread { get; set; } = 100f;
	[Property] float SongYSpread { get; set; } = 100f;
	float TargetAngle => SelectedIndex * MathF.PI * 2f / SongPanelCount;
	public float AngleOffset { get; set; } = 0f;
	public float LastOffset { get; set; } = 0f;
	public float Zoom { get; set; } = 0.7f;
	bool firstLoad = true;
	public int SortOrder
	{
		get => _sortOrder;
		set
		{
			var oldList = CurrentSetList;
			CurrentSetList = GetCurrentSetList();
			BeatmapSet selected;
			try
			{
				selected = oldList[SongListInfoPanel.Instance.Index];
			}
			catch
			{
				selected = null;
			}
			_sortOrder = value;
			if ( selected is not null )
			{
				SelectedIndex = CurrentSetList.IndexOf( selected );
			}
			CurrentAngle = TargetAngle + AngleOffset;
			Cookie.Set( "sortOrder", _sortOrder );
		}

	}
	int _sortOrder = Cookie.Get( "sortOrder", 0 );

	public int SelectedIndex
	{
		get => _selectedIndex;
		set
		{
			_selectedIndex = value;
			ResetButtons();
		}
	}
	private int _selectedIndex = 0;
	public List<BeatmapSet> CurrentSetList { get; set; } = new();

	float CurrentAngle { get; set; } = 0f;
	List<SongListPanel> SongPanels { get; set; } = new();

	int Moving { get; set; } = 0;
	TimeSince TimeSinceHeld { get; set; } = 0f;
	TimeSince TimeSinceMoved { get; set; } = 0f;
	Sandbox.UI.WorldInput worldInput;
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
			var panelScript = panel.Components.Get<SongListPanel>();
			SongPanels.Add( panelScript );
		}
	}

	protected override void OnStart()
	{
		CurrentSetList = GetCurrentSetList();
		if ( Beatmap.Loaded is null )
		{
			SelectedIndex = Random.Shared.Int( 0, CurrentSetList.Count - 1 );
		}
		else
		{
			var set = Beatmap.Loaded.GetBeatmapSet();
			SelectedIndex = GetCurrentSetList().IndexOf( set );
			SongListInfoPanel.SelectedIndex = set.Beatmaps.OrderBy( x => x.Difficulty ).ToList().IndexOf( Beatmap.Loaded );
			Beatmap.Loaded = null;
		}
		CurrentAngle = SelectedIndex * MathF.PI * 2f / (float)SongPanelCount;

		worldInput = new Sandbox.UI.WorldInput();
		worldInput.Enabled = true;
	}

	protected override void OnDestroy()
	{
		if ( worldInput is not null )
			worldInput.Enabled = false;
	}

	protected override void OnUpdate()
	{
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
		float diff = CurrentAngle - TargetAngle;
		int index = 0;
		var panelIndex = _selectedIndex % SongPanelCount;
		while ( panelIndex < 0 )
		{
			panelIndex += SongPanelCount;
		}
		foreach ( var panel in SongPanels )
		{
			var xSpread = SongXSpread;
			if ( index == panelIndex )
			{
				xSpread *= 1.05f;
			}
			panel.Transform.LocalPosition = panel.Transform.LocalPosition.LerpTo( new Vector3( (xSpread / 2f) + (MathF.Cos( angle ) * xSpread * Zoom / 2f), 0f, MathF.Sin( angle ) * SongYSpread * Zoom ), 20f * Time.Delta );
			panel.Transform.LocalRotation = Rotation.From( 0f, 90f, 0f );
			panel.Transform.LocalScale = Zoom;
			angle -= MathF.PI * 2f / (float)SongPanelCount;
			index++;
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
			firstLoad = false;
		}
		Timer = 0f;
	}

	void ResetButtons()
	{
		var panelIndex = _selectedIndex;
		var all = GetCurrentSetList();
		int totalAm = all.Count();
		if ( totalAm <= 0 ) return;
		while ( panelIndex < 0 )
		{
			panelIndex += SongPanelCount;
		}
		for ( int i = panelIndex - 6; i <= panelIndex + 6; i++ )
		{
			int offset = i - panelIndex;
			var panel = SongPanels[(i + SongPanelCount * 2) % SongPanelCount];
			int ind = _selectedIndex;
			while ( ind < 0 )
			{
				ind += totalAm;
			}
			panel.Index = (ind + totalAm + offset) % totalAm;
		}
		SongListInfoPanel.Select( 0 );
	}

	List<BeatmapSet> GetCurrentSetList()
	{
		// Get stack trace
		List<BeatmapSet> list = new();
		switch ( _sortOrder )
		{
			case 0:
				list.AddRange( BeatmapSet.All.OrderBy( x => x.DateAdded ).Reverse() );
				break;
			case 1:
				list.AddRange( BeatmapSet.All.OrderBy( x => x.Artist ) );
				break;
			case 2:
				list.AddRange( BeatmapSet.All.OrderBy( x => x.Name ) );
				break;
			case 3:
				list.AddRange( BeatmapSet.All.OrderBy( x =>
				{
					var lastPlayed = DateTime.MinValue;
					foreach ( var beatmap in x.Beatmaps )
					{
						var stats = GameStats.GetStats( beatmap );
						if ( stats.LastPlayed > lastPlayed )
						{
							lastPlayed = stats.LastPlayed;
						}
					}
					return lastPlayed;
				} ).Reverse() );
				break;
			case 4:
				list.AddRange( BeatmapSet.All.OrderBy( x =>
				{
					var highscore = x.Beatmaps.OrderBy( b => b.Difficulty * 1000 + b.GetHighscore() ).Last().GetHighscore();
					return highscore;
				} ).Reverse() );
				break;
			case 5:
				list.AddRange( BeatmapSet.All.OrderBy( x =>
				{
					var time = 0f;
					foreach ( var beatmap in x.Beatmaps )
					{
						if ( beatmap.Length > time )
						{
							time = beatmap.Length;
						}
					}
					return time;
				} ) );
				break;
		}

		return list;
	}
}