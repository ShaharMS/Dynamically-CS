using Dynamically.Formulas;
using Dynamically.Screens;
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
        get => _anchored || DeFactoAnchored;
        set
        {
            _anchored = value;
        }
    }

    /// <summary>
    /// Use only when an illegal move is made on a joint or segment (for example, A is on BC, but is dragged beyond it).
    /// when enabled, pretty much the entire board should have this enabled as well, which is why this field is a static property.
    /// </summary>
    public static bool DeFactoAnchored = false;

    public override bool Draggable { get => base.Draggable && !Anchored; set => base.Draggable = value; }

    public override double X { get => base.X; set { if (dispatchingOnMoved) throw new ConstraintException($"Cannot edit Vertex {this}'s X position in an OnMoved function."); else if (!Anchored) base.X = UntouchedX = value; else UntouchedX = value; } }
    public override double Y { get => base.Y; set { if (dispatchingOnMoved) throw new ConstraintException($"Cannot edit Vertex {this}'s Y position in an OnMoved function."); else if (!Anchored) base.Y = UntouchedY = value; else UntouchedY = value; } }

    public List<Func<double, double, (double X, double Y)>> PositioningByFormula = new();


    int safety = 0;
    double epsilon = 0.70710678118; //0.5 * sqrt(2), for a diagonal of 0.5px

    /// <summary>
    /// Basically the same as X and Y, but not affected by anchors.
    /// </summary>
    public double UntouchedX, UntouchedY;

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
        double x = UntouchedX, y = UntouchedY;
        px ??= UntouchedX;
        py ??= UntouchedY;

        var broken = false;

        if (!_anchored)
        {
            X = (double)x; Y = (double)y;
            double? initialX = null, initialY = null;
            do
            {

                if (initialX != null && initialY != null)
                {
                    var p = new RatioOnSegmentFormula(new SegmentFormula(initialX.Value, initialY.Value, UntouchedX, UntouchedY), 0.5).PointOnRatio;
                    UntouchedX = p.X;
                    UntouchedY = p.Y;
                    initialX = initialY = null;
                }
                if (safety > 20)
                {
                    Log.Write($"Vertex {this} cannot be positioned currently. Locking board.");
                    safety = 0;
                    DeFactoAnchored = true;
                    MainWindow.BigScreen.Immovables.Add(this);
                    broken = true;
                    break;
                }
                foreach (var listener in PositioningByFormula)
                {
                    var p = listener(UntouchedX, UntouchedY);
                    UntouchedX = p.X; UntouchedY = p.Y;
                    initialX ??= UntouchedX;
                    initialY ??= UntouchedY;
                }
                safety++;
            } while (initialX != null && initialY != null && (initialX.Value, initialY.Value).DistanceTo(UntouchedX, UntouchedY) > epsilon);

            safety = 0;

            if (!broken)
            {
                MainWindow.BigScreen.Immovables.Remove(this);
                if (!MainWindow.BigScreen.Immovables.Any()) DeFactoAnchored = false;
            }

            X = UntouchedX; Y = UntouchedY;

            dispatchingOnMoved = true;
            foreach (var listener in OnMoved)
            {
                listener(X, Y, (double)px, (double)py);
            }
            dispatchingOnMoved = false;
        }

        Reposition();
    }
}
