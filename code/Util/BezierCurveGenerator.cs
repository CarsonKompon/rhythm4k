using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhythm4K.Util;

public static class BezierCurveGenerator
{
    public static List<Vector3> GenerateBezierCurve( Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint1, Vector3 controlPoint2, int segments )
    {
        List<Vector3> points = new List<Vector3>();

        for ( int i = 0; i <= segments; i++ )
        {
            float t = i / (float)segments;
            Vector3 point = Vector3.CubicBezier( startPoint, endPoint, controlPoint1, controlPoint2, t );
            points.Add( point );
        }

        return points;
    }
}

