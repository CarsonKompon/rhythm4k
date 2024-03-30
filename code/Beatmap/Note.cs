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
    public float BakedTime { get; set; } = -1f;
    public float BakedLength { get; set; } = -1f;
    public int Points { get; set; }

    public Note( float offset, int lane, NoteType type, float length = 0f, bool isBaked = false )
    {
        if ( isBaked )
        {
            BakedTime = offset;
            BakedLength = length;
        }
        else
        {
            Offset = offset;
            Length = length;
        }
        Lane = lane;
        Type = (int)type;
    }
}