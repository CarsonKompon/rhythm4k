using System;
using Sandbox;
using Sandbox.Audio;
using Sandbox.UI;

namespace Rhythm4K;

public sealed class GameManager : Component, IMusicPlayer
{
	public static GameManager Instance { get; private set; }
	public static bool IsCalibrating = false;
	public static int RetryCount = 0;

	[Property] public GameObject ResultsScreen { get; set; }
	[Property] public SceneFile MenuScene { get; set; }
	[Property] public GameObject LanePrefab { get; set; }
	[Property] public GameObject NotePrefab { get; set; }
	[Property] float LaneSpacing { get; set; } = 64f;

	[Property] public float PeakThreshold { get; set; } = 1.08f;
	public float AdjustedPeakThreshold { get; private set; } = 0f;

	public Beatmap Beatmap { get; set; }
	public BeatmapSet BeatmapSet { get; set; }
	public List<Lane> Lanes { get; set; } = new();
	public List<NoteComponent> Notes { get; set; } = new();

	public bool IsPlaying { get; private set; } = false;
	public bool IsPaused { get; private set; } = false;
	public bool IsFinished { get; private set; } = false;

	public float CurrentBPM { get; set; } = 120f;
	public float BeatLength => 60f / CurrentBPM;
	public float ScreenTime => BaseScreenTime / GamePreferences.Settings.ScrollSpeedMultiplier;
	float BaseScreenTime = 1f;
	public float CurrentTime => _currentTime - AudioLatency;
	public float CurrentTimeNoLatency => _currentTime;
	TimeSince _currentTime { get; set; } = 0f;
	public float SongTime => Music?.PlaybackTime ?? 0f;

	public MusicPlayer Music { get; set; }

	public bool IsPeaking { get; set; }
	public float Energy { get; set; }
	public float EnergyHistoryAverage { get; set; }
	public float PeakKickVolume { get; set; }
	List<float> EnergyHistory = new();

	public float Score { get; set; } = 0;
	public int Combo { get; set; } = 0;
	public Replay Replay { get; set; }
	List<float> JudgementTimes = new();
	List<float> JudgementScores = new();
	List<string> JudgementNames = new();
	float TotalScore { get; set; } = 0;
	float AudioLatency { get; set; } = 0;

	public Action OnBeat { get; set; }

	List<Note> NotesToSpawn = new();
	List<BpmChange> BpmChanges = new();

	protected override void OnStart()
	{
		Beatmap = Beatmap.Loaded;
		BeatmapSet = Beatmap?.GetBeatmapSet();
		if ( Beatmap is null )
		{
			Scene.Load( MenuScene );
			return;
		}
		Replay = new Replay( Beatmap );
		AudioLatency = GamePreferences.Settings.AudioLatency / 1000f;

		InitLanes();
		StartSong();

		Instance = this;
	}

	protected override void OnUpdate()
	{
		if ( !IsPlaying ) return;

		if ( !IsFinished && CurrentTime >= Beatmap.Length + 3f )
		{
			ResultsScreen.Enabled = true;
			IsFinished = true;
			IsPlaying = false;
		}

		CalculateMusic();

		if ( !IsPaused )
		{
			UpdateBpm();
			SpawnNextNotes();
			UpdateNotes();
		}
		UpdatePause();

		if ( Input.Pressed( "ScrollSpeedUp" ) )
		{
			GamePreferences.Settings.ScrollSpeedMultiplier *= 1.1f;
			GamePreferences.SaveSettings();
		}
		else if ( Input.Pressed( "ScrollSpeedDown" ) )
		{
			GamePreferences.Settings.ScrollSpeedMultiplier *= 0.9f;
			GamePreferences.SaveSettings();
		}
	}

	public async void StartSong()
	{
		if ( IsPlaying ) return;

		Music?.Stop();
		Music?.Dispose();

		JudgementTimes = Judgement.GetJudgementTimes( Beatmap.Difficulty );
		JudgementScores = Judgement.Scores;
		JudgementNames = Judgement.Names;
		TotalScore = JudgementScores.First() * Beatmap.Notes.Count;

		NotesToSpawn = Beatmap.Notes.ToList();
		BpmChanges = Beatmap.BpmChanges.ToList();

		CurrentBPM = Beatmap.BpmChanges[0].BPM; // TODO: Move BPM from BeatmapSet to Beatmap
		var scrollSpeed = (Beatmap.ScrollSpeed <= 0) ? 1f : Beatmap.ScrollSpeed;
		BaseScreenTime = 120f / CurrentBPM * scrollSpeed / 2f;
		_currentTime = Beatmap.Offset - ScreenTime;

		IsPlaying = true;
		Score = 0;
		Combo = 0;
		Replay.Hits.Clear();

		if ( CurrentTime < 0 )
		{
			await Task.DelaySeconds( -CurrentTime );
		}
		Log.Info( $"{Beatmap.FileSystem} {BeatmapSet.Path}/{Beatmap.AudioFilename}" );
		Music = MusicPlayer.Play( Beatmap.FileSystem, BeatmapSet.Path + "/" + Beatmap.AudioFilename );
		// Log.Info( "this log is necessary lmfao" );
		Music.Seek( MathF.Max( CurrentTime, 0f ) );
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
		float[] noteTimes = new float[Lanes.Count];
		for ( int i = 0; i < Lanes.Count; i++ )
		{
			pressed[i] = Input.Pressed( $"{Lanes.Count}KeyButton{i + 1}" );
			noteTimes[i] = 1000;
		}

		List<NoteComponent> notesToHit = new();
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
				BreakCombo();
				continue;
			}

			// Check if we can hit the note.
			var laneIndex = note.Note.Lane;
			var distance = MathF.Abs( CurrentTime - noteTime );
			if ( distance < timing && noteTime < noteTimes[laneIndex] )
			{
				bool canAdd = true;
				foreach ( var otherNote in notesToHit )
				{
					if ( otherNote.Note.Lane == laneIndex )
					{
						if ( otherNote.Note.BakedTime < noteTime )
						{
							notesToHit.Remove( otherNote );
						}
						else
						{
							canAdd = false;
						}
					}
				}
				if ( canAdd )
				{
					notesToHit = notesToHit.Where( x => x.Note.Lane != laneIndex ).ToList();
					notesToHit.Add( note );
					noteTimes[laneIndex] = noteTime;
				}
			}
		}

		List<NoteComponent> missedNotes = new();
		List<NoteComponent> hitNotes = new();
		// Check if we hit any notes.
		foreach ( NoteComponent note in notesToHit )
		{
			if ( pressed[note.Note.Lane] )
			{
				Notes.Remove( note );
				note.GameObject.Destroy();
				HitNote( note );
				hitNotes.Add( note );
			}
			else
			{
				missedNotes.Add( note );
			}
		}

		// Check if any of the missed notes were before the hit notes.
		foreach ( NoteComponent note in missedNotes )
		{
			if ( hitNotes.Any( x => x.Note.BakedTime < note.Note.BakedTime ) )
			{
				// If so, we missed the note.
			}
		}
	}

	void CalculateMusic()
	{
		Mixer.Master.Volume = GamePreferences.Settings.MasterVolume / 100f;

		if ( Music is null ) return;

		Music.Position = Scene.Camera.Transform.Position;
		Music.Paused = IsPaused;

		// if ( SongTime > 0 )
		// {
		// 	CurrentTime += MathF.Min( SongTime - CurrentTime, Time.Delta * 2 );
		// }

		var spectrum = Music.Spectrum;

		// Energy Calculations
		var energy = 0f;
		float length = spectrum.Length;
		for ( int i = 0; i < length; i++ )
		{
			energy += spectrum[i];
		}
		energy /= length;
		Energy = Energy.LerpTo( energy, Time.Delta * 30f );

		EnergyHistory.Add( energy );
		if ( EnergyHistory.Count > 64 ) EnergyHistory.RemoveAt( 0 );

		// Energy History Average
		EnergyHistoryAverage = 0f;
		for ( int i = 0; i < EnergyHistory.Count; i++ )
		{
			EnergyHistoryAverage += EnergyHistory[i];
		}
		EnergyHistoryAverage /= EnergyHistory.Count;

		// Beat Detection
		float energySum = 0f;
		foreach ( var energyValue in EnergyHistory )
		{
			energySum += energyValue;
		}
		float energyMean = energySum / EnergyHistory.Count;

		float variance = 0f;
		foreach ( var energyValue in EnergyHistory )
		{
			variance += (energyValue - energyMean) * (energyValue - energyMean);
		}
		float energyStdDev = (float)Math.Sqrt( variance / EnergyHistory.Count );

		// Adjusted Peak Threshold Calculation
		AdjustedPeakThreshold = PeakThreshold * energyStdDev;

		if ( EnergyHistoryAverage > 0.05f && Energy > EnergyHistoryAverage + AdjustedPeakThreshold )
		{
			if ( !IsPeaking )
			{
				OnBeat?.Invoke();
			}
			IsPeaking = true;
		}
		else
		{
			IsPeaking = false;
		}
	}

	void UpdatePause()
	{
		if ( Input.EscapePressed )
		{
			SetPause( !IsPaused );
		}
	}

	public void SetPause( bool pause )
	{
		IsPaused = pause;
		Scene.TimeScale = IsPaused ? 0 : 1;
		if ( !IsPaused && CurrentTime >= 0 )
		{
			Music?.Seek( CurrentTime );
		}
		Sound.Play( IsPaused ? "ui_pause" : "ui_unpause" );
	}

	void HitNote( NoteComponent note )
	{
		var difference = CurrentTime - note.Note.BakedTime;
		var absDifference = MathF.Abs( difference );
		var points = 0f;
		for ( int i = 0; i < JudgementTimes.Count; i++ )
		{
			if ( absDifference <= JudgementTimes[i] )
			{
				// Hit
				points = JudgementScores[i];
				GameHud.Instance?.SetJudgement( JudgementNames[i] );
				break;
			}
		}
		if ( points == 0f )
		{
			// Miss
			BreakCombo();
			return;
		}
		Score += points / TotalScore * 1000000f;
		Combo++;
		Replay.MaxCombo = Math.Max( Replay.MaxCombo, Combo );
		GameHud.Instance?.SetCombo( Combo );
		Replay.Hits.Add( new HitInfo( note.Note.Lane, IsCalibrating ? CurrentTimeNoLatency : CurrentTime, difference ) );
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

	public void BreakCombo()
	{
		Combo = 0;
		GameHud.Instance?.SetCombo( 0 );
		GameHud.Instance?.SetJudgement( "Miss" );
		Replay.Hits.Add( new HitInfo( -1, IsCalibrating ? CurrentTimeNoLatency : CurrentTime, 999 ) );
	}

	public static async void RunCalibrationWizard()
	{
		var wizardSet = await SongBuilder.Load( "beatmaps/calibration_wizard", FileSystem.Mounted );
		if ( wizardSet is not null )
		{
			GamePreferences.Settings.DoneFirstTimeSetup = true;
			GamePreferences.SaveSettings();
			Beatmap.Loaded = wizardSet.Beatmaps.FirstOrDefault();
			GameManager.IsCalibrating = true;
			MainMenuScreen.Instance.StartGame();
		}
	}
}