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
        set {
            _x1 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _y1;
    public double y1 
    {
        get => _y1;
        set {
            _y1 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _x2;
    public double x2 
    {
        get => _x2;
        set {
            _x2 = value;
            Move(x1, y1, x2, y2);
        }
    }
    double _y2;
    public double y2 
    {
        get => _y2;
        set {
            _y2 = value;
            Move(x1, y1, x2, y2);
        }
    }

    double yIntercept
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

    public Point? Intersect(SegmentFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula.yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);
        if ((X > x1 || X < x2) || ((x2 > x1) && (X > x2 || X < x1))) return null;
        return new Point(X, Y[0]);
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula.yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);
        if ((X > x1 || X < x2) || ((x2 > x1) && (X > x2 || X < x1))) return null;
        return new Point(X, Y[0]);
    }

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
        return new[] { new Point(start.X + dx, SolveForY(start.X + dx)[0]), new Point(start.X - dx, SolveForY(start.X - dx)[0]) };
    }
    public override Point? GetClosestOnFormula(double x, double y)
    {
        Point potentialIntersect(RayFormula formula)
        {
            var X = (yIntercept - formula.yIntercept) / (formula.slope - slope);
            var Y = new RayFormula(new Point(x1, y1), new Point(x2, y2)).SolveForY(X);
            return new Point(X, Y[0]);
        }
        double nSlope = -1 / slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        var potential = potentialIntersect(nRay);
        if ((potential.X > x1 && potential.X < x2) || (potential.X > x2 && potential.X < x1)) return potential;
        if ((x1 > x2 && potential.X > x1) || (x1 < x2 && potential.X < x1)) return new Point(x1, y1);
        if ((x2 > x1 && potential.X > x2) || (x2 < x1 && potential.X < x2)) return new Point(x2, y2);
        return null;

    }

    public override double[] SolveForX(double y)
    {
        if ((y1 > y2 && (y > y1 || y < y2)) || (y1 < y2 && (y < y1 || y > y2))) return Array.Empty<double>();
        return new double[] { (y - yIntercept) / slope };
    }

    public override double[] SolveForY(double x)
    {
        if ((x1 > x2 && (x > x1 || x < x2)) || (x1 < x2 && (x < x1 || x > x2))) return Array.Empty<double>();
        return new double[] { slope * x + yIntercept };
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
