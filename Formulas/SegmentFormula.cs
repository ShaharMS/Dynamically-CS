using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class SegmentFormula : Formula
{
    double _x1;
    public double x1
    {
        get => _x1;
        set
        {
            _x1 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _y1;
    public double y1
    {
        get => _y1;
        set
        {
            _y1 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _x2;
    public double x2
    {
        get => _x2;
        set
        {
            _x2 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _y2;
    public double y2
    {
        get => _y2;
        set
        {
            _y2 = value;
            Move(x1, y1, x2, y2);
        }
    }

    double potentialYIntercept
    {
        get
        {
            if (x1 > 0)
            {
                return y1 - (slope * x1);
            }
            else
            {
                return y1 + (slope * x1);
            }
        }
    }
    public double slope
    {
        get => (y2 - y1) / (x2 - x1);
    }

    public double Length
    {
        get => (x1, y1).DistanceTo(x2, y2);
    }

    public SegmentFormula(Point p1, Point p2) : base()
    {
        x1 = p1.X;
        y1 = p1.Y;
        x2 = p2.X;
        y2 = p2.Y;
    }

    public SegmentFormula(double x1, double y1, double x2, double y2) : base()
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    public SegmentFormula(Segment segment)
    {
        x1 = segment.joint1.X;
        y1 = segment.joint1.Y;
        x2 = segment.joint2.X;
        y2 = segment.joint2.Y;

        segment.joint1.OnMoved.Add((_, _, _, _) =>
        {
            _x1 = segment.joint1.X;
            y1 = segment.joint1.Y;
        });

        segment.joint2.OnMoved.Add((_, _, _, _) =>
        {
            _x2 = segment.joint2.X;
            y2 = segment.joint2.Y;
        });
    }

    public RayFormula CastToRay() => new RayFormula(x1, y1, x2, y2);

    public Point? Intersect(SegmentFormula formula)
    {
        var intersect = CastToRay().Intersect(formula.CastToRay());
        if (intersect == null) return null;
        var i = intersect.Value;
        return SolveForY(i.X).Length + formula.SolveForY(i.X).Length == 2 ? intersect : null;
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope.RoughlyEquals(slope)) return null;

        if (!double.IsFinite(slope))
        {
            var x = (x1 + x2) / 2;
            var y = formula.SolveForY(x);
            if (y.Length == 0) return null;
            var value = y[0];
            if (Math.Clamp(value, y1.Min(y2), y2.Max(y1)) != value) return null;
            return new Point(x, y[0]);
        }

        var X = (potentialYIntercept - formula.yIntercept) / (formula.slope - slope);

        if (!double.IsFinite(formula.slope))
        {
            if (formula.ReferencePoint == null) return null;
            X = formula.ReferencePoint.Value.X;
        }
        double[] Y = SolveForY(X);
        if (Y.Length == 0) return null;
        return new Point(X, Y[0]);
    }

    public Point[] Intersect(CircleFormula formula)
    {
        var m = slope;
        var c = potentialYIntercept;
        var a = formula.centerX;
        var b = formula.centerY;
        var r = formula.radius;

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
    public override double DistanceTo(Point point)
    {
        // Get the closest point on the ray to the given point
        Point? closestPoint = GetClosestOnFormula(point);
        if (!closestPoint.HasValue) return double.NaN;
        return point.DistanceTo(closestPoint.Value);
    }

    public Point[] GetPointsByDistanceFrom(Point start, double distance)
    {
        var dx = distance * Math.Cos(Tools.GetRadiansBetween3Points(start, new Point(0, 0), new Point(1, 0)));
        if (SolveForY(start.X + dx).Length == 0) return Array.Empty<Point>();
        if (SolveForY(start.X - dx).Length == 0) return Array.Empty<Point>();
        return new[] { new Point(start.X + dx, SolveForY(start.X + dx)[0]), new Point(start.X - dx, SolveForY(start.X - dx)[0]) };
    }
    public override Point? GetClosestOnFormula(double x, double y)
    {
        Point potentialIntersect(RayFormula formula)
        {
            var X = (potentialYIntercept - formula.yIntercept) / (formula.slope - slope);
            var Y = new RayFormula(new Point(x1, y1), new Point(x2, y2)).SolveForY(X);
            return new Point(X, Y.Length != 0 ? Y[0] : double.NaN);
        }
        double nSlope = -1 / slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        var potential = potentialIntersect(nRay);
        if (double.IsNaN(potential.Y)) return null;
        if (potential.DistanceTo(x1, y1) < Length && potential.DistanceTo(x2, y2) < Length) return potential;
        if (potential.DistanceTo(x1, y1) < potential.DistanceTo(x2, y2)) return new Point(x1, y1);
        if (potential.DistanceTo(x2, y2) < potential.DistanceTo(x1, y1)) return new Point(x2, y2);
        return null;

    }

    public override double[] SolveForX(double y)
    {
        if ((y1 > y2 && (y > y1 || y < y2)) || (y1 < y2 && (y < y1 || y > y2)) || double.IsNaN((y - potentialYIntercept) / slope)) return Array.Empty<double>();
        return new double[] { (y - potentialYIntercept) / slope };
    }

    public override double[] SolveForY(double x)
    {
        if ((x1 > x2 && (x > x1 || x < x2)) || (x1 < x2 && (x < x1 || x > x2)) || double.IsNaN(slope * x + potentialYIntercept)) return Array.Empty<double>();
        return new double[] { slope * x + potentialYIntercept };
    }

    public override void Move(double x, double y)
    {
        var rectX = Math.Min(x1, x2);
        var rectY = Math.Min(y1, y2);
        double offsetX = x - rectX, offsetY = y - rectY;

        _x1 += offsetX;
        _x2 += offsetX;
        _y1 += offsetY;
        _y2 += offsetX;


        var nRectX = Math.Min(x1, x2);
        var nRectY = Math.Min(y1, y2);

        foreach (var l in OnMoved) l(nRectX, nRectY, rectX, rectY);
        foreach (var l in OnChange) l();
    }
    public void Move(double x1, double y1, double x2, double y2)
    {
        var rectX = Math.Min(this.x1, this.x2);
        var rectY = Math.Min(this.y1, this.y2);

        _x1 = x1; _y1 = y1;
        _x2 = x2; _y2 = y2;

        var nRectX = Math.Min(this.x1, this.x2);
        var nRectY = Math.Min(this.y1, this.y2);

        foreach (var l in OnMoved) l(nRectX, nRectY, rectX, rectY);
        foreach (var l in OnChange) l();
    }
}
