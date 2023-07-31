using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Joint
{


    bool _anchored;
    public bool Anchored
    {
        get => _anchored;
        set
        {
            _anchored = value;
            Draggable &= value;
        }
    }

    public override double X { get => base.X; set { if (!Anchored) base.X = value; } }
    public override double Y { get => base.Y; set { if (!Anchored) base.Y = value; } }

    public List<Func<double, double, (double X, double Y)>> PositioningByFormula = new();


    int safety = 0;
    double epsilon = 0.1;
    public override void DispatchOnMovedEvents(double x, double y, double px, double py)
    {
        if (!Anchored)
        {
            X = x; Y = y;
            foreach (var listener in PositioningByFormula)
            {
                var p = listener(X, Y);
                X = p.X; Y = p.Y;
            }
            foreach (var listener in OnMoved)
            {
                listener(x, y, px, py);
            }

            if (X - x < epsilon && X - y < epsilon) safety = 0;
            else if (epsilon > 100)
            {
                string Keys()
                {
                    var s = "";
                    foreach (var role in Roles.underlying.Keys)
                    {
                        if (Roles.CountOf(role) == 0) continue;
                        s += role.ToString();
                        s += $" ({Roles.CountOf(role)}) ({Log.StringifyCollection(Roles.Access<dynamic>(role))})\n\r";
                    }
                    if (s == "") return s;
                    return s.Substring(0, s.Length - 2);
                }
                Log.Write($"Joint {this} is unpositionable. Info:\n{Keys()}");
                safety = 0;
            }
            else
            {
                safety++;
                DispatchOnMovedEvents(X, Y, x, y);
            }
        }
    }
}
