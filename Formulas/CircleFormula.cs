using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;
public class CircleFormula : Formula
{
    public bool Moving;
    double _radius;
    public double radius
    {
        get => _radius;
        set
        {
            _radius = value;
            foreach (var l in OnChange) l();
        }
    }
    double _centerX;
    public double centerX
    {
        get => _centerX;
        set
        {
            var prev = _centerX;
            _centerX = value;
            foreach (var l in OnChange) l();
            foreach (var l in OnMoved) l(value, _centerY, prev, _centerY);
        }
    }

    double _centerY;
    public double centerY
    {
        get => _centerY;
        set
        {
            var prev = _centerY;
            _centerY = value;
            foreach (var l in OnMoved) l(_centerX, value, _centerX, prev);
        }
    }
    public CircleFormula(double radius, double centerX, double centerY) : base()
    {
        this.radius = radius;
        this.centerX = centerX;
        this.centerY = centerY;
    }

    public CircleFormula(double radius, Point center) : this(radius, center.X, center.Y) { }

    public Point[] Intersect(RayFormula formula) => formula.Intersect(this);
    public Point[] Intersect(SegmentFormula formula) => formula.Intersect(this);

    public Point[] Intersect(CircleFormula formula)
    {
        var a = centerX;
        var b = centerY;
        var c = formula.centerX;
        var d = formula.centerY;
        var r1 = radius;
        var r2 = formula.radius;

        // we can extract the ray on which the 0-2 points of collision reside:
        var handleRay = new RayFormula((a * a + b * b + r2 * r2 - c * c - d * d - r1 * r1) / (2 * (b - d)), (c - a) / (b - d));

        // now just intersect it with this circle
        return handleRay.Intersect(this);
    }
    public bool Intersects(RayFormula formula) => Intersect(formula).Length > 0;
    public bool Intersects(SegmentFormula formula) => Intersect(formula).Length > 0;
    public bool Intersects(CircleFormula formula) => Intersect(formula).Length > 0;

    public override double[] SolveForX(double y)
    {
        if (y > centerY + radius || y < centerY - radius) return Array.Empty<double>();
        var x1 = centerX - Math.Sqrt(-(centerY - y).Pow(2) + radius.Pow(2));
        var x2 = centerX + Math.Sqrt(-(centerY - y).Pow(2) + radius.Pow(2));
        return new[] { x1, x2 };
    }

    public override double[] SolveForY(double x)
    {
        if (x > centerX + radius || x < centerX - radius) return Array.Empty<double>();
        var y1 = centerY - Math.Sqrt(-(centerX - x).Pow(2) + radius.Pow(2));
        var y2 = centerY + Math.Sqrt(-(centerX - x).Pow(2) + radius.Pow(2));
        return new[] { y1, y2 };
    }

    /// <summary>
    /// If px & py.RoughlyEquals(cx) & cy, returns null.
    /// </summary>
    /// <param name="px"></param>
    /// <param name="py"></param>
    /// <returns></returns>
    public override Point? GetClosestOnFormula(double px, double py)
    {
        //Avoid divide by 0 error:
        if (px.RoughlyEquals(centerX) && py.RoughlyEquals(centerY)) return null;
        double vX = px - centerX;
        double vY = py - centerY;
        double magV = Math.Sqrt(vX * vX + vY * vY);
        double aX = centerX + vX / magV * radius;
        double aY = centerY + vY / magV * radius;
        return new Point(aX, aY);
    }

    public override void Move(double x, double y)
    {
        double px = _centerX, py = _centerY;
        _centerX = x;
        _centerY = y;
        foreach (var l in OnMoved) l(_centerX, _centerY, px, py);
        foreach (var l in OnChange) l();
    }

    public override (double X, double Y) UpdateJointPosition(double inputX, double inputY)
    {
        if (!Moving) return base.UpdateJointPosition(inputX, inputY);

        return (inputX, inputY);
    }
}
