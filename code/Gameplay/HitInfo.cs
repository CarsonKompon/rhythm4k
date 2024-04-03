using Sandbox;

namespace Rhythm4K;

public class HitInfo
{
    public int Lane { get; set; }
    public float Time { get; set; }
    public float Offset { get; set; }

    public HitInfo( int lane, float time, float offset )
    {
        Lane = lane;
        Time = time;
        Offset = offset;
    }
}