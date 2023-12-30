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
    public double X1
    {
        get => _x1;
        set
        {
            _x1 = value;
            Move(X1, Y1, X2, Y2);
        }
    }
    double _y1;
    public double Y1
    {
        get => _y1;
        set
        {
            _y1 = value;
            Move(X1, Y1, X2, Y2);
        }
    }
    double _x2;
    public double X2
    {
        get => _x2;
        set
        {
            _x2 = value;
            Move(X1, Y1, X2, Y2);
        }
    }
    double _y2;
    public double Y2
    {
        get => _y2;
        set
        {
            _y2 = value;
            Move(X1, Y1, X2, Y2);
        }
    }

    public void QuietSet(double x1, double y1, double x2, double y2)
    {
        _x1 = x1;
        _y1 = y1;
        _x2 = x2;
        _y2 = y2;
    }

    double PotentialYIntercept
    {
        get
        {
            if (X1 > 0)
            {
                return Y1 - (Slope * X1);
            }
            else
            {
                return Y1 + (Slope * X1);
            }
        }
    }
    public double Slope
    {
        get => (Y2 - Y1) / (X2 - X1);
    }

    public double Length
    {
        get => (X1, Y1).DistanceTo(X2, Y2);
    }

    public SegmentFormula(Point p1, Point p2) : base()
    {
        X1 = p1.X;
        Y1 = p1.Y;
        X2 = p2.X;
        Y2 = p2.Y;
    }

    public SegmentFormula(double x1, double y1, double x2, double y2) : base()
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public SegmentFormula(Segment segment)
    {
        X1 = segment.Vertex1.X;
        Y1 = segment.Vertex1.Y;
        X2 = segment.Vertex2.X;
        Y2 = segment.Vertex2.Y;

        segment.Vertex1.OnMoved.Add((_, _, _, _) =>
        {
            _x1 = segment.Vertex1.X;
            Y1 = segment.Vertex1.Y;
        });

        segment.Vertex2.OnMoved.Add((_, _, _, _) =>
        {
            _x2 = segment.Vertex2.X;
            Y2 = segment.Vertex2.Y;
        });
    }

    public RayFormula CastToRay() => new(X1, Y1, X2, Y2);

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
        if (formula.Slope.RoughlyEquals(Slope)) return null;

        if (!double.IsFinite(Slope))
        {
            var x = (X1 + X2) / 2;
            var y = formula.SolveForY(x);
            if (y.Length == 0) return null;
            var value = y[0];
            if (Math.Clamp(value, Y1.Min(Y2), Y2.Max(Y1)) != value) return null;
            return new Point(x, y[0]);
        }

        var X = (PotentialYIntercept - formula.YIntercept) / (formula.Slope - Slope);

        if (!double.IsFinite(formula.Slope))
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
        var m = Slope;
        var c = PotentialYIntercept;
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
            var X = (PotentialYIntercept - formula.YIntercept) / (formula.Slope - Slope);
            var Y = new RayFormula(new Point(X1, Y1), new Point(X2, Y2)).SolveForY(X);
            return new Point(X, Y.Length != 0 ? Y[0] : double.NaN);
        }
        double nSlope = -1 / Slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        var potential = potentialIntersect(nRay);
        if (double.IsNaN(potential.Y)) return null;
        if (potential.DistanceTo(X1, Y1) < Length && potential.DistanceTo(X2, Y2) < Length) return potential;
        if (potential.DistanceTo(X1, Y1) < potential.DistanceTo(X2, Y2)) return new Point(X1, Y1);
        if (potential.DistanceTo(X2, Y2) < potential.DistanceTo(X1, Y1)) return new Point(X2, Y2);
        return null;

    }

    public override double[] SolveForX(double y)
    {
        if ((Y1 > Y2 && (y > Y1 || y < Y2)) || (Y1 < Y2 && (y < Y1 || y > Y2)) || double.IsNaN((y - PotentialYIntercept) / Slope)) return Array.Empty<double>();
        return new double[] { (y - PotentialYIntercept) / Slope };
    }

    public override double[] SolveForY(double x)
    {
        if ((X1 > X2 && (x > X1 || x < X2)) || (X1 < X2 && (x < X1 || x > X2)) || double.IsNaN(Slope * x + PotentialYIntercept)) return Array.Empty<double>();
        return new double[] { Slope * x + PotentialYIntercept };
    }

    public override void Move(double x, double y)
    {
        var rectX = Math.Min(X1, X2);
        var rectY = Math.Min(Y1, Y2);
        double offsetX = x - rectX, offsetY = y - rectY;

        _x1 += offsetX;
        _x2 += offsetX;
        _y1 += offsetY;
        _y2 += offsetX;


        var nRectX = Math.Min(X1, X2);
        var nRectY = Math.Min(Y1, Y2);

        foreach (var l in OnMoved) l(nRectX, nRectY, rectX, rectY);
        foreach (var l in OnChange) l();
    }
    public void Move(double x1, double y1, double x2, double y2)
    {
        var rectX = Math.Min(this.X1, this.X2);
        var rectY = Math.Min(this.Y1, this.Y2);

        _x1 = x1; _y1 = y1;
        _x2 = x2; _y2 = y2;

        var nRectX = Math.Min(this.X1, this.X2);
        var nRectY = Math.Min(this.Y1, this.Y2);

        foreach (var l in OnMoved) l(nRectX, nRectY, rectX, rectY);
        foreach (var l in OnChange) l();
    }
}
