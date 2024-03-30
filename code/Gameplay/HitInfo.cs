using Sandbox;

namespace Rhythm4K;

public class HitInfo
{
    public float Time { get; set; }
    public float Offset { get; set; }

    public HitInfo( float time, float offset )
    {
        Time = time;
        Offset = offset;
    }
}