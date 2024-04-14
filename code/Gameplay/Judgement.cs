using System;
using Sandbox;

public static class Judgement
{
    public static List<string> Names = new List<string>
        {
            "Perfect",
            "Great",
            "Good",
            "OK",
            "Meh",
            "Miss"
        };

    public static List<float> Scores = new List<float>
        {
            305f,
            300f,
            200f,
            100f,
            50f,
            0f
        };

    public static List<float> GetJudgementTimes( float difficulty )
    {
        var diff = 0.003f * MathX.Clamp( difficulty, 1f, 10 );
        return new List<float>
        {
            0.016f,
            0.064f - diff,
            0.097f - diff,
            0.127f - diff,
            0.151f - diff,
            0.188f - diff
        };
    }

    public static string GetRank( float accuracy )
    {
        if ( accuracy >= 100f )
        {
            return "SS";
        }
        if ( accuracy > 95f )
        {
            return "S";
        }
        if ( accuracy > 90f )
        {
            return "A";
        }
        if ( accuracy > 80f )
        {
            return "B";
        }
        if ( accuracy > 70f )
        {
            return "C";
        }
        return "D";
    }

    public static string GetRankInfo( float accuracy )
    {
        if ( accuracy >= 100f )
        {
            return "Perfect 100%";
        }
        if ( accuracy > 95f )
        {
            return "> 95%";
        }
        if ( accuracy > 90f )
        {
            return "> 90%";
        }
        if ( accuracy > 80f )
        {
            return "> 80%";
        }
        if ( accuracy > 70f )
        {
            return "> 70%";
        }
        return "<= 70%";
    }
}