using Sandbox;

namespace Rhythm4K;

public enum NoteType
{
    Normal = 0,
    Mine = 1,
    Hold = 999,
}

public struct Note
{
    public float Offset { get; set; }
    public float Length { get; set; }
    public int Type { get; set; }
    public int Lane { get; set; }
    public float BakedTime { get; set; }
    public float BakedLength { get; set; }
    public int Points { get; set; }

    public Note( float offset, int lane, NoteType type, float length = 0f )
    {
        Offset = offset;
        Lane = lane;
        Type = (int)type;
        Length = length;
    }
}