using Microsoft.VisualBasic;
using Sandbox.UI;

namespace Rhythm4K;

public partial class Trail : Panel
{
    public Image TrailTrunk;
    public Image TrailCap;
    public Note Note;
    

    public Trail()
    {
        TrailTrunk = AddChild<Image>();
        TrailTrunk.AddClass("trunk");
        TrailTrunk.SetTexture("/sprites/arrows/alloy/hold.png");

        TrailCap = AddChild<Image>();
        TrailCap.AddClass("cap");

        if(Cookie.Get<bool>("rhythm4k.downscroll", false))
        {
            AddClass("downscroll");
            TrailCap.SetTexture("/sprites/arrows/alloy/hold-cap-downscroll.png");
        }
        else
        {
            TrailCap.SetTexture("/sprites/arrows/alloy/hold-cap.png");
        }
    }

    public void SetNote(Note note, bool setColor = true)
    {
        Note = note;
        SetClass("lane-" + note.Lane.ToString(), true);

        if(setColor)
        {
            if(note.Offset % 250f == 0f) Style.FilterHueRotate = -10f;              // 4th (Red)
            else if(note.Offset % 125f == 0f) Style.FilterHueRotate = -140f;        // 8th (Blue)
            else if(note.Offset % 83.33f <= 0.01f) Style.FilterHueRotate = -95f;   // 12th (Purple)
            else if(note.Offset % 62.5f == 0f) Style.FilterHueRotate = 45f;         // 16th (Yellow)
            else if(note.Offset % 41.66f <= 0.01f) Style.FilterHueRotate = -53f;    // 24th (Pink)
            else if(note.Offset % 31.25f == 0f) Style.FilterHueRotate = 18;          // 32nd (Orange)
            else if(note.Offset % 20.833f <= 0.01f) Style.FilterHueRotate = -170f;  // 48th (Cyan)
            else if(note.Offset % 15.625f == 0f) Style.FilterHueRotate = 120f;      // 64th (Green)
            else Style.FilterSaturate = 0f;
        }
    }
}