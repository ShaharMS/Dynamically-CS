using Avalonia;
using Dynamically.Shapes;
using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using Dynamically.Backend.Helpers;
using Dynamically.Backend;

namespace Dynamically.Formulas;

public class RayFormula : Formula
{
    double _yIntercept;
    public double YIntercept
    {
        get => _yIntercept;
        set
        {
            var prev = _yIntercept;
            _yIntercept = value;
            foreach (var l in OnChange) l();
        }
    }
    double _slope;
    public double Slope
    {
        get => _slope;
        set
        {
            _slope = value;
            foreach (var l in OnChange) l();
        }
    }

    public void QuietSet(double yIntercept, double slope)
    {
        _yIntercept = yIntercept;
        _slope = slope;
    }

    /// <summary>
    /// Useful when dealing with vertical rays, which cannot be represented accurately using Slope & yItntercept.
    /// Only set when defining the ray using a point.
    /// </summary>
    public Point? ReferencePoint { get; private set; }

    public RayFormula(double yIntercept, double slope) : base()
    {
        YIntercept = yIntercept;
        Slope = slope;
    }

    public RayFormula(Point pointOnRay, double slope) : base()
    { 
        if (pointOnRay.X > 0)
        {
            YIntercept = pointOnRay.Y - (slope * pointOnRay.X);
        }
        else
        {
            YIntercept = pointOnRay.Y + (slope * pointOnRay.X);
        }
        this.Slope = slope;
    }

    public RayFormula(Point p1, Point p2) : base()
    {
        Slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (p1.X > 0)
        {
            YIntercept = p1.Y - (Slope * p1.X);
        }
        else
        {
            YIntercept = p1.Y + (Slope * p1.X);
        }
        ReferencePoint = p1;
    }

    public RayFormula(double x1, double y1, double x2, double y2) : base() 
    {
        Slope = (y2 - y1) / (x2 - x1);
        if (x1 > 0)
        {
            YIntercept = y1 - (Slope * x1);
        }
        else
        {
            YIntercept = y1 + (Slope * x1);
        }
        ReferencePoint = new Point(x1, y1);
    }

    /// <summary>
    /// Keeps Slope
    /// </summary>
    /// <param name="point"></param>
    public void ChangePositionByPoint(Point point)
    {
        if (point.X > 0)
        {
            YIntercept = point.Y - (Slope * point.X);
        }
        else
        {
            YIntercept = point.Y + (Slope * point.X);
        }
        ReferencePoint = point;
    }

    public void Set(Point p1, Point p2)
    {
        Slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (p1.X > 0)
        {
            YIntercept = p1.Y - (Slope * p1.X);
        }
        else
        {
            YIntercept = p1.Y + (Slope * p1.X);
        }
        ReferencePoint = p1;
    }

    public void Set(double yIntercept, double slope)
    {
        this.YIntercept = yIntercept;
        this.Slope = slope;
        ReferencePoint = null;
    }

    public void Set(Point pointOnRay, double slope)
    {
        if (pointOnRay.X > 0)
        {
            YIntercept = pointOnRay.Y - (slope * pointOnRay.X);
        }
        else
        {
            YIntercept = pointOnRay.Y + (slope * pointOnRay.X);
        }
        this.Slope = slope;
        ReferencePoint = pointOnRay;
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.Slope.RoughlyEquals(Slope)) return null;

        var X = (YIntercept - formula._yIntercept) / (formula.Slope - Slope);
        var Y = SolveForY(X);

        if (Y.Length == 0) return null;
        return new Point(X, Y[0]);
    }

    public Point? Intersect(SegmentFormula formula) => formula.Intersect(this);

    public Point[] Intersect(CircleFormula formula)
    {
        var m = Slope;
        var c = YIntercept;
        var a = formula.CenterX;
        var b = formula.CenterY;
        var r = formula.Radius;

        var A = (m * m + 1);
        var B = (2 * (m * (c - b) - a));
        var C = (a * a + (c - b) * (c - b) - r * r);

        var x1 = (-B + Math.Sqrt(B * B - 4 * A * C)) / (2 * A);
        var x2 = (-B - Math.Sqrt(B * B - 4 * A * C)) / (2 * A);

        var y1 = SolveForY(x1);
        var y2 = SolveForY(x2);

        if (y1.Length == 0 && y2.Length == 0) return Array.Empty<Point>();
        else if (y1.Length == 0) return new[] {new Point(x2, y2[0])};
        else if (y2.Length == 0) return new[] {new Point(x1, y1[0])};
        else return new[] {new Point(x1, y1[0]), new Point(x2, y2[0])};
    }

    public bool Intersects(RayFormula formula) => Intersect(formula) != null;
    public bool Intersects(SegmentFormula formula) => Intersect(formula) != null;
    public bool Intersects(CircleFormula formula) => Intersect(formula).Length > 0;

    public Point[] GetPointsByDistanceFrom(Point start, double distance)
    {
        var dx = distance * Math.Cos(Tools.GetRadiansBetween3Points(start, new Point(0, 0), new Point(1, 0)));
        if (SolveForY(start.X + dx).Length == 0) return Array.Empty<Point>();
        if (SolveForY(start.X - dx).Length == 0) return Array.Empty<Point>();
        return new[] { new Point(start.X + dx, SolveForY(start.X + dx)[0]), new Point(start.X - dx, SolveForY(start.X - dx)[0]) };
    }
    public override Point? GetClosestOnFormula(double x, double y)
    {
        double nSlope = -1 / Slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        return Intersect(nRay);
    }

    public override double[] SolveForX(double y)
    {
        if (double.IsNaN((y - YIntercept) / Slope)) return Array.Empty<double>();
        return new double[] {(y - YIntercept) / Slope};
    }

    public override double[] SolveForY(double x)
    {
        if (double.IsNaN(Slope * x + YIntercept)) return Array.Empty<double>();
        return new double[] {Slope * x + YIntercept};
    }

    public override void Move(double x, double y)
    {
        var pYI = _yIntercept;
        if (x > 0)
        {
            _yIntercept = y - (Slope * x);
        }
        else
        {
            _yIntercept = y + (Slope * x);
        }
        foreach (var l in OnMoved) l(0, pYI, 0, _yIntercept);
        foreach (var l in OnChange) l();
    }
}
