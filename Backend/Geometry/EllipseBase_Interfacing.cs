using Avalonia;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class EllipseBase : ICanFollowFormula
{
    public override double X { get => CenterX; set { if (dispatchingOnMoved) Log.Warn($"Cannot edit Vertex {this}'s X position in an OnMoved function."); else if (!Anchored) CenterX = value; } }
    public override double Y { get => CenterY; set { if (dispatchingOnMoved) Log.Warn($"Cannot edit Vertex {this}'s Y position in an OnMoved function."); else if (!Anchored) CenterY = value; } }

    List<Func<double, double, (double X, double Y)>> _positioningByFormula = new();
    public List<Func<double, double, (double X, double Y)>> PositioningByFormula => _positioningByFormula;

    public RoleMap Roles { get; set; }

    int safety = 0;
    double epsilon = 0.70710678118; //0.5 * sqrt(2), for a diagonal of 0.5px
    bool dispatchingOnMoved = false;


    public override void DispatchOnMovedEvents(double? px = null, double? py = null)
    {
        Log.Write("Dispatching on moved events for ellipse " + this);
        double x = X, y = Y;
        px ??= X;
        py ??= Y;

        if (!Anchored)
        {
            X = (double)x; Y = (double)y;
            double? initialX = null, initialY = null;
            do
            {

                if (initialX != null && initialY != null)
                {
                    var p = new RatioOnSegmentFormula(new SegmentFormula(initialX.Value, initialY.Value, X, Y), 0.5).PointOnRatio;
                    X = p.X;
                    Y = p.Y;
                    initialX = initialY = null;
                }
                if (safety > 20)
                {
                    safety = 0;
                    break;
                }
                foreach (var listener in PositioningByFormula)
                {
                    var p = listener(X, Y);
                    // We will use parametric representation here
                    var angle = Center.RadiansTo(p.X, p.Y);
                    var fociAngle = Focal1.RadiansTo(Focal2) < Focal2.RadiansTo(Focal1) ? Focal1.RadiansTo(Focal2) : Focal2.RadiansTo(Focal1);
                    angle -= fociAngle;
                    var expectedRadius = A * Math.Cos(angle) + B * Math.Sin(angle);
                    var foundRadius = Center.DistanceTo(p.X, p.Y);

                    // Move X & Y so foundRadius is equal to expectedRadius
                    var diff = foundRadius - expectedRadius;
                    X += diff * Math.Cos(angle);
                    Y += diff * Math.Sin(angle);
                    initialX ??= X;
                    initialY ??= Y;
                }
                safety++;
            } while (initialX != null && initialY != null && (initialX.Value, initialY.Value).DistanceTo(X, Y) > epsilon);
            safety = 0;
            dispatchingOnMoved = true;
            foreach (var listener in OnMoved)
            {
                listener(X, Y, (double)px, (double)py);
            }
            dispatchingOnMoved = false;
        }
        Reposition();
    }

    public (double X, double Y) GetCenterPosition(double x, double y)
    {
        _ = x; _ = y;
        return (CenterX, CenterY);
    }
}
