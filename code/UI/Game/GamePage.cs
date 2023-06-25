using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Rhythm4K;

public static class NoteTimings{
    public const float Error = 0.150f;
    public const float Critical = 0.046f;
}

public partial class GamePage
{

    private float ScreenTime
    {
        get
        {
            if(_ScreenTime == -1f)
            {
                _ScreenTime = Cookie.Get<float>("rhythm4k.screenTime", 0.7f);
            }
            return _ScreenTime;
        }
        set
        {
            _ScreenTime = value;
        }
    }
    private float _ScreenTime = -1f;

    private bool Downscroll => Cookie.Get<bool>("rhythm4k.downscroll", false);

    [GameEvent.Client.Frame]
    private void OnFrame()
    {
        if(IsPlaying)
        {
            // Check for BPM Change
            if(BpmChanges.Count > 0)
            {
                foreach(BpmChange bpmChange in BpmChanges)
                {
                    if(CurrentTime >= bpmChange.Offset)
                    {
                        CurrentBPM = bpmChange.BPM;
                        BpmChanges.Remove(bpmChange);
                        break;
                    }
                }
            }

            // Instantiate new Notes
            if(Notes.Count > 0)
            {
                List<Note> notes = GetNextNotes();
                for(int i=0; i<notes.Count; i++)
                {
                    Note note = notes[i];
                    Notes.Remove(note);
                    if(note.Type == (int)NoteType.Hold) continue;
                    Lane lane = Lanes[note.Lane];
                    Arrow arrow = lane.AddChild<Arrow>();
                    arrow.SetNote(note);
                    Arrows.Add(arrow);
                    note.Arrow = arrow;

                    if(note.Length > 0f)
                    {
                        Trail trail = lane.AddChild<Trail>();
                        trail.SetNote(note);
                        Trails.Add(trail);
                    }
                    LivingNotes.Add(note);
                    notes[i] = note;
                }
            }
            else if(CurrentTime >= SongLength + 2f)
            {
                FinishedSong();
            }

            for(int i=0; i<LivingNotes.Count; i++)
            {
                Note note = LivingNotes[i];
                if(note.BakedTime <= CurrentTime - NoteTimings.Error)
                {
                    LivingNotes.Remove(note);
                }
            }

            foreach(Lane lane in Lanes)
            {
                foreach(Panel child in lane.Children)
                {
                    // Loop through all the arrows
                    if(child is Arrow arrow)
                    {
                        float noteTime = arrow.Note.BakedTime;
                        float percent = 100f * ((noteTime - CurrentTime) / ScreenTime);
                        Log.Info(Downscroll);
                        if(Downscroll)
                        {
                            arrow.Style.Top = Length.Percent(100f-percent);
                        }
                        else
                        {
                            arrow.Style.Top = Length.Percent(percent);
                        }

                        if(!arrow.Missed && CurrentTime > noteTime + NoteTimings.Error)
                        {
                            ResetCombo();
                            arrow.Missed = true;
                        }

                        if(percent <= -50f)
                        {
                            Arrows.Remove(arrow);
                            arrow.Delete();
                        }
                    }

                    // Loop through all the trails
                    if(child is Trail trail)
                    {
                        float positionPercent = 100f * ((trail.Note.BakedTime - CurrentTime) / ScreenTime);
                        float lengthPercent = 100f * (trail.Note.BakedLength / ScreenTime);
                        if(Downscroll)
                        {
                            float downscrollPos = 100f-positionPercent-lengthPercent+6f;
                            if(downscrollPos > 0f && positionPercent < -2.5f)
                            {
                                lengthPercent += positionPercent + 2.5f;
                                positionPercent = -2.5f;
                                downscrollPos = 100f-positionPercent-lengthPercent+6f;
                            }
                            if(downscrollPos <= 0f && lengthPercent >= 100f)
                            {
                                trail.Style.Top = Length.Percent(0f);
                                trail.Style.Height = Length.Percent(100f-positionPercent+6f);
                            }
                            else
                            {
                                trail.Style.Top = Length.Percent(downscrollPos);
                                trail.Style.Height = Length.Percent(lengthPercent);
                            }
                        }
                        else
                        {
                            if(positionPercent < 0f)
                            {
                                lengthPercent += positionPercent;
                                positionPercent = 0f;
                            }
                            trail.Style.Top = Length.Percent(positionPercent);
                            trail.Style.Height = Length.Percent(lengthPercent);
                        }
                        if(positionPercent + lengthPercent <= 0f) trail.Delete();
                    }
                }
            }

            ProgressBar.Style.Width = Length.Percent((CurrentTime / SongLength) * 100f);
        }

        LastTime = CurrentTime;
    }

    [GameEvent.Client.BuildInput]
    public void BuildInput()
    {
        if(!IsPlaying) return;

        bool[] pressed = {
            Input.Pressed("LeftArrow"),
            Input.Pressed("DownArrow"), 
            Input.Pressed("UpArrow"),
            Input.Pressed("RightArrow")
        };

        bool[] held = {
            Input.Down("LeftArrow"),
            Input.Down("DownArrow"),
            Input.Down("UpArrow"),
            Input.Down("RightArrow")
        };

        foreach(Lane lane in Lanes)
        {
            lane.Receptor.SetClass("pressing", held[lane.LaneIndex]);
        }

        // Hit Arrows
        List<Note> notes = GetNotesToHit();
        float lowestOffset = -1f;
        for(int i=0; i<notes.Count; i++)
        {
            Note note = notes[i];
            if(note.Arrow != null && note.Arrow.Missed) continue;
            bool hit = false;
            switch((NoteType)note.Type)
            {
                case NoteType.Hold:
                    hit = held[note.Lane];
                    break;
                default:
                    hit = pressed[note.Lane];
                    break;
            }
            if(hit)
            {
                if(lowestOffset == -1f || note.Offset < lowestOffset) lowestOffset = note.Offset;
                Score += note.Points;

                if((NoteType)note.Type == NoteType.Normal)
                {
                    Combo += 1;
                    if(Combo > MaxCombo) MaxCombo = Combo;
                }

                LivingNotes.Remove(note);
                if(note.Arrow != null)
                {
                    Receptor receptor = Lanes[note.Lane].Receptor;
                    receptor.Glow(note.Arrow);

                    Arrows.Remove(note.Arrow);
                    note.Arrow.Delete();
                }

                notes.Remove(note);
            }
        }

        // Remove any arrows that were skipped (if any)
        foreach(Note note in notes)
        {

            if(note.Offset < lowestOffset)
            {
                Log.Info(note.Offset + " < " + lowestOffset);
                LivingNotes.Remove(note);
                ResetCombo();
                if(note.Arrow != null) note.Arrow.Missed = true;
            }
        }
    }

    public List<Note> GetNextNotes()
    {
        List<Note> notes = new();
        foreach(Note note in Notes)
        {
            if(CurrentTime >= note.BakedTime - ScreenTime)
            {
                notes.Add(note);
            }
        }
        return notes;
    }

    public void FinishedSong()
    {
        IsPlaying = false;
        this.Navigate("/");
    }

    public List<Note> GetNotesToHit()
    {
        float[] noteTimes = {1000, 1000, 1000, 1000};
        List<Note> notes = new();
        for(int i=0; i<LivingNotes.Count; i++)
        {
            Note note = LivingNotes[i];
            float noteTime = note.BakedTime;
            float time = CurrentTime;
            float distance = MathF.Abs(time - noteTime);
            float timing = NoteTimings.Error;
            if((NoteType)note.Type == NoteType.Hold) timing = NoteTimings.Critical;
            if(distance < timing && noteTime < noteTimes[note.Lane])
            {
                noteTimes[note.Lane] = distance;
                if((NoteType)note.Type == NoteType.Hold)
                {
                    note.Points = CritValue;
                }
                else
                {
                    note.Points = (int)MathF.Floor((float)CritValue / (distance > NoteTimings.Critical ? 2f : 1f));
                }
                notes.Add(note);
            }
            LivingNotes[i] = note;
        }

        return notes;
    }

    void ResetCombo()
    {
        Combo = 0;
    }
}