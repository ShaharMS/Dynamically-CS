﻿using Dynamically.Formulas;
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
        }
    }

    public override bool Draggable { get => base.Draggable && !Anchored; set => base.Draggable = value; }

    public override double X { get => base.X; set { if (!Anchored) base.X = value; } }
    public override double Y { get => base.Y; set { if (!Anchored) base.Y = value; } }

    public List<Func<double, double, (double X, double Y)>> PositioningByFormula = new();


    int safety = 0;
    double epsilon = 0.70710678118; //0.5 * sqrt(2), for a diagonal of 0.5px
    public override void DispatchOnMovedEvents(double x, double y, double px, double py)
    {
        if (!Anchored)
        {
            X = x; Y = y;
            double? initialX = null, initialY = null;
            do
            {
                
                if (initialX != null && initialY != null)
                {
                    var p = new RatioOnSegmentFormula(new SegmentFormula(initialX.Value, initialY.Value, X, Y), 0.5).pointOnRatio;
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
                    if (initialX == null) initialX = X;
                    if (initialY == null) initialY = Y;
                }
                safety++;
            } while ((initialX != null && initialY != null && (initialX.Value, initialY.Value).DistanceTo(X, Y) > epsilon));

            x = X; y = Y;
            safety = 0;
            foreach (var listener in OnMoved)
            {
                listener(X, Y, px, py);
            }

            if ((X, Y).DistanceTo(x, y) < epsilon) safety = 0;
            else if (safety > 20)
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
        reposition();
    }
}