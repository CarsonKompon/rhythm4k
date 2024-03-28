using System;
using Sandbox;

namespace Rhythm4K;

public sealed class GameManager : Component, IMusicPlayer
{
	[Property] public SceneFile MenuScene { get; set; }
	[Property] public GameObject LanePrefab { get; set; }
	[Property] public GameObject NotePrefab { get; set; }
	[Property] float LaneSpacing { get; set; } = 64f;

	public Beatmap Beatmap { get; set; }
	public BeatmapSet BeatmapSet { get; set; }
	public List<Lane> Lanes { get; set; } = new();
	public List<NoteComponent> Notes { get; set; } = new();

	public bool IsPlaying { get; private set; } = false;

	public float CurrentBPM { get; set; } = 120f;
	public float BeatLength => 60f / CurrentBPM;
	public float ScreenTime = 1f;
	public RealTimeSince CurrentTime = 0f;

	public MusicPlayer Music { get; set; }

	public bool IsPeaking { get; set; }
	public float Energy { get; set; }
	public float EnergyHistoryAverage { get; set; }
	public float PeakKickVolume { get; set; }

	public Action OnBeat { get; set; }

	List<Note> NotesToSpawn = new();
	List<BpmChange> BpmChanges = new();

	protected override void OnStart()
	{
		Beatmap = Beatmap.Loaded;
		BeatmapSet = Beatmap?.GetSet();
		if ( Beatmap is null )
		{
			Scene.Load( MenuScene );
		}

		InitLanes();

		StartSong();
	}

	protected override void OnUpdate()
	{
		if ( !IsPlaying ) return;

		if ( Music is not null )
			Music.Position = Scene.Camera.Transform.Position;

		UpdateBpm();
		SpawnNextNotes();
		UpdateNotes();
	}

	public async void StartSong()
	{
		if ( IsPlaying ) return;

		Music?.Stop();
		Music?.Dispose();

		NotesToSpawn = Beatmap.Notes.ToList();
		BpmChanges = Beatmap.BpmChanges.ToList();

		CurrentBPM = Beatmap.BpmChanges[0].BPM; // TODO: Move BPM from BeatmapSet to Beatmap
		var scrollSpeed = (Beatmap.ScrollSpeed <= 0) ? 1f : Beatmap.ScrollSpeed;
		ScreenTime = 120f / CurrentBPM * scrollSpeed;
		CurrentTime = BeatmapSet.Offset - ScreenTime;

		IsPlaying = true;

		if ( CurrentTime < 0 )
		{
			await Task.DelaySeconds( -CurrentTime );
		}
		Music = MusicPlayer.Play( FileSystem.Data, BeatmapSet.GetFullPath() + BeatmapSet.AudioFilename );
		Music.Seek( CurrentTime );
	}

	void UpdateBpm()
	{
		if ( BpmChanges.Count == 0 ) return;
		foreach ( BpmChange bpmChange in BpmChanges )
		{
			if ( CurrentTime >= bpmChange.Offset )
			{
				CurrentBPM = bpmChange.BPM;
				BpmChanges.Remove( bpmChange );
				break;
			}
		}
	}

	void UpdateNotes()
	{
		if ( Notes.Count == 0 ) return;

		bool[] pressed = new bool[Lanes.Count];
		for ( int i = 0; i < Lanes.Count; i++ )
		{
			pressed[i] = Input.Pressed( $"{Lanes.Count}KeyButton{i + 1}" );
		}

		List<NoteComponent> notesToHit = new();
		float[] noteTimes = { 1000, 1000, 1000, 1000 };
		foreach ( NoteComponent note in Notes )
		{
			if ( !note.IsValid() ) continue;
			note.Transform.Position = note.Lane.StartPosition.Transform.Position.LerpTo( note.Lane.EndPosition.Transform.Position, 1f - (note.Note.BakedTime - CurrentTime) / ScreenTime, false );

			float timing = 0.15f;
			var noteTime = note.Note.BakedTime;
			// Check if we missed the note
			if ( CurrentTime - timing > noteTime )
			{
				note.GameObject.Destroy();
				continue;
			}

			// Check if we can hit the note.
			var laneIndex = note.Note.Lane;
			var distance = MathF.Abs( CurrentTime - noteTime );
			if ( distance < timing && pressed[laneIndex] && noteTime < noteTimes[laneIndex] )
			{
				notesToHit.Add( note );
				noteTimes[laneIndex] = noteTime;
			}
		}

		// Check if we hit any notes.
		foreach ( NoteComponent note in notesToHit )
		{
			Notes.Remove( note );
			note.GameObject.Destroy();
		}
	}

	void SpawnNextNotes()
	{
		if ( NotesToSpawn.Count == 0 ) return;

		List<Note> notes = GetNextNotes();
		for ( int i = 0; i < notes.Count; i++ )
		{
			Note note = notes[i];
			NotesToSpawn.Remove( note );

			NoteComponent spawnedNote;
			if ( note.Type == (int)NoteType.Hold )
			{
				// TODO: Remove these probably
				continue;
			}
			spawnedNote = CreateNote( note );
			Notes.Add( spawnedNote );

			if ( note.Length > 0 )
			{
				// TODO: Create trails
			}
		}
	}

	List<Note> GetNextNotes()
	{
		List<Note> notes = new();
		foreach ( Note note in NotesToSpawn )
		{
			if ( CurrentTime >= note.BakedTime - ScreenTime )
			{
				notes.Add( note );
			}
		}
		return notes;
	}

	NoteComponent CreateNote( Note note )
	{
		var noteObject = NotePrefab.Clone( Lanes[note.Lane].StartPosition.Transform.Position );
		noteObject.SetParent( GameObject );
		var noteScript = noteObject.Components.Get<NoteComponent>();
		noteScript.Note = note;
		noteScript.Lane = Lanes[note.Lane];
		return noteScript;
	}

	void InitLanes()
	{
		var laneCount = Beatmap.Lanes;
		var laneOffset = LaneSpacing * (laneCount - 1) / 2;

		for ( var i = 0; i < laneCount; i++ )
		{
			var lane = LanePrefab.Clone();
			lane.SetParent( GameObject );
			lane.Transform.LocalPosition = new Vector3( 0, laneOffset - i * LaneSpacing, 0 );
			var laneScript = lane.Components.Get<Lane>();
			laneScript.SetLane( i );
			Lanes.Add( laneScript );
		}
	}
}