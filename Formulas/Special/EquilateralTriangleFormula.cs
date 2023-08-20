using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;

namespace Dynamically.Formulas.Special;

public class EquilateralTriangleFormula : ShapeTypeFormula
{
    public Triangle Triangle;

    /// <summary>
    /// One is at x degs, one at x + 60, and another at x + 120
    /// </summary>
    public Joint[] Order;

    public EquilateralTriangleFormula(Triangle subject)
    {
        Triangle = subject;
        Current = Triangle.joint1;
    }
    public override (double X, double Y) GetPositionForCurrent(double x, double y) {

        if (!Active) return (x, y);
        if (!Triangle.Contains(Current)) return (x, y);
        if (Current.CurrentlyDragging) return (x, y);

        var other1 = Current != Triangle.joint1 ? Triangle.joint1 : Current != Triangle.joint2 ? Triangle.joint2 : Triangle.joint3;
        var other2 = Current != Triangle.joint1 && other1 != Triangle.joint1 ? Triangle.joint1 : Current != Triangle.joint2 && other1 != Triangle.joint2 ? Triangle.joint2 : Triangle.joint3;
        if (Current.Anchored || other1.Anchored || other2.Anchored) return (x, y);

        var center = Triangle.GetIncircleCenter();
        double len, angle;
        for (int i = 0; i < 3; i++)
        {
            if (Order[i].CurrentlyDragging)
            {
                len = center.DistanceTo(Order[i]);
                angle = center.RadiansTo(Order[i]);
                if (Array.IndexOf(Order, Current) == (i + 1) % 3) angle += 2 * Math.PI / 3;
                else angle += 4 * Math.PI / 3;
                return (center.X + len * Math.Cos(angle), center.Y + len * Math.Sin(angle));
            }
        }
        // for now
        // Nothing is moving with inention, assume first one is moving
        len = center.DistanceTo(Order[0]);
        angle = center.RadiansTo(Order[0]);
        if (Array.IndexOf(Order, Current) == 1) angle += 2 * Math.PI / 3;
        else angle += 4 * Math.PI / 3;
        return (center.X + len * Math.Cos(angle), center.Y + len * Math.Sin(angle));
    }
}
