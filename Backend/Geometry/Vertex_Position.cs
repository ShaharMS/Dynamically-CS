using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Vertex
{
    bool _anchored;
    public bool Anchored
    {
        get => _anchored;
        set
        {
            _anchored = value;
        }
    }

    public override bool Draggable { get => base.Draggable && !Anchored; set => base.Draggable = value; }

    public override double X { get => base.X; set { if (dispatchingOnMoved) throw new ConstraintException($"Cannot edit Vertex {this}'s X position in an OnMoved function."); else if (!Anchored) base.X = value; } }
    public override double Y { get => base.Y; set { if (dispatchingOnMoved) throw new ConstraintException($"Cannot edit Vertex {this}'s Y position in an OnMoved function."); else if (!Anchored) base.Y = value; } }

    public List<Func<double, double, (double X, double Y)>> PositioningByFormula = new();


    int safety = 0;
    double epsilon = 0.70710678118; //0.5 * sqrt(2), for a diagonal of 0.5px

    bool dispatchingOnMoved = false;

    /// <summary>
    /// Dispatches OnMoved events & updates position according to existing formulas. 
    /// when a parameter provided is null, or isnt provided, its reset to Instance X & Y coords.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="px"></param>
    /// <param name="py"></param>
    public override void DispatchOnMovedEvents(double? px = null, double? py = null)
    {
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
                    X = p.X; Y = p.Y;
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

    public bool IsMovementLegal(double futureX, double futureY)
    {
        if (Anchored) return true;

        double px = X, py = Y;

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
                return false;
            }
            foreach (var listener in PositioningByFormula)
            {
                var p = listener(X, Y);
                X = p.X; Y = p.Y;
                initialX ??= X;
                initialY ??= Y;
            }
            safety++;
        } while (initialX != null && initialY != null && (initialX.Value, initialY.Value).DistanceTo(X, Y) > epsilon);

        // Initial test for this vertex has passed. now, we need to check if other vertices that rely
        // on this vertex also pass the test.

        var typeDict = new Dictionary<Type, int>
        {
            {typeof(CircleFormula), 0},
            {typeof(PointFormula), 1},
            {typeof(RatioOnSegmentFormula), 2},
            {typeof(SegmentFormula), 3},
            {typeof(RayFormula), 4},
        };

        foreach (Formula formula in FormulasEffected)
        {
            var followers = formula.Followers;
            // We silently change the formula for now, to fake the change
            switch (typeDict[formula.GetType()])
            {
                case 0: 
                    var circle = (CircleFormula)formula;
                    circle.QuietSet(circle.Radius, futureX, futureY); 
                    
                    foreach (Vertex f in followers)
                    {
                        if (f == this) throw new Exception($"Vertex {this} cannot follow a formula it directly affects");
                        if (!f.IsMovementLegal(f.X, f.Y)) return false; // The vertex itself isnt moving, the formula is forcing it to move
                    }

                    // Test passed, reset circle
                    circle.QuietSet(circle.Radius, X, Y); // X and Y are "old" in this context
                    break;
                case 1:
                    var point = (PointFormula)formula;
                    point.QuietSet(futureX, futureY);

                    foreach (Vertex f in point.Followers)
                    {
                        if (f == this) throw new Exception($"Vertex {this} cannot follow a formula it directly affects");
                        if (!f.IsMovementLegal(f.X, f.Y)) return false;
                    }
                    point.QuietSet(X, Y);
                    break;
                case 2:
                    // Special case - handled by an inside SegmentFormula, no need to quiet set here
                    break;

                case 3:
                    var segment = (SegmentFormula)formula;
                    if (segment.X1 == X) segment.QuietSet(futureX, futureY, segment.X2, segment.Y2);
                    else segment.QuietSet(segment.X1, segment.Y1, futureX, futureY);

                    foreach (Vertex f in segment.Followers)
                    {
                        if (f == this) throw new Exception($"Vertex {this} cannot follow a formula it directly affects");
                        if (!f.IsMovementLegal(f.X, f.Y)) return false;
                    }

                    if (segment.X1 == futureX) segment.QuietSet(X, Y, segment.X2, segment.Y2);
                    else segment.QuietSet(segment.X1, segment.Y1, X, Y);
                    break;
                case 4:
                    // Also special case, for now. TODO :)
                    break;
            }
        }


        return true;
    }
}
