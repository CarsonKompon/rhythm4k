using Rhythm4K.Util;
using Sandbox;

namespace Rhythm4K;

public sealed class BezierCurveComponent : Component
{
    [Property] GameObject StartPoint { get; set; }
    [Property] GameObject EndPoint { get; set; }

    protected override void DrawGizmos()
    {
        if ( StartPoint != null && EndPoint != null )
        {
            var bezier = BezierCurveGenerator.GenerateBezierCurve( StartPoint.WorldPosition, EndPoint.WorldPosition, StartPoint.WorldPosition + StartPoint.WorldRotation.Forward * 100f, EndPoint.WorldPosition + EndPoint.WorldRotation.Forward * 100f, 50 );
            for ( int i = 0; i < bezier.Count - 1; i++ )
            {
                var p1 = Transform.World.PointToLocal( bezier[i] );
                var p2 = Transform.World.PointToLocal( bezier[i + 1] );
                Gizmo.Draw.Line( p1, p2 );
            }

            Gizmo.Draw.SolidSphere( Transform.World.PointToLocal( StartPoint.WorldPosition ), 1f );
            Gizmo.Draw.SolidSphere( Transform.World.PointToLocal( EndPoint.WorldPosition ), 1f );
        }
    }
    protected override void OnUpdate()
    {

    }
}
