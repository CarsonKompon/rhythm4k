using Sandbox;
using Sandbox.UI;

public partial class Receptor : Panel
{
    public int LaneIndex = 0;
    public Image Sprite;
    public Image GlowSprite;
    public RealTimeSince Timer = 1f;

    public Receptor()
    {
        Sprite = AddChild<Image>();
        Sprite.SetClass("receptor-arrow", true);
        GlowSprite = AddChild<Image>();
        GlowSprite.SetClass("receptor-glow", true);
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        if(Timer > 0 && GlowSprite.HasClass("show"))
        {
            GlowSprite.RemoveClass("show");
        }
    }

    public void SetLane(int i)
    {
        LaneIndex = i;
        string @class = "lane-" + i.ToString();
        AddClass(@class);
        Sprite.AddClass(@class);
        GlowSprite.AddClass(@class);
    }

    public void Glow(Panel panel)
    {
        GlowSprite.Style.FilterHueRotate = panel.Style.FilterHueRotate;
        GlowSprite.Style.FilterSaturate = panel.Style.FilterSaturate;
        GlowSprite.AddClass("show");
        Timer = -0.1f;
    }
}