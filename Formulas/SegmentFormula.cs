using Avalonia;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class SegmentFormula : FormulaBase, Formula
{
    public Point p1 { get; set; }
    public Point p2 { get; set; }

    double yIntercept
    {
        get
        {
            if (p1.X > 0)
            {
                return p1.Y - (slope * p1.X);
            }
            else
            {
                return p1.Y + (slope * p1.X);
            }
        }
    }
    public double slope
    {
        get => (p2.Y - p1.Y) / (p2.X - p1.X);
    }

    public SegmentFormula(Point p1, Point p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Point? Intersect(SegmentFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula.yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);
        if ((X > p1.X || X < p2.X) || ((p2.X > p1.X) && (X > p2.X || X < p1.X))) return null;
        return new Point(X, Y[0]);
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula.yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);
        if ((X > p1.X || X < p2.X) || ((p2.X > p1.X) && (X > p2.X || X < p1.X))) return null;
        return new Point(X, Y[0]);
    }

    public double DistanceTo(Point point)
    {
        // Get the closest point on the ray to the given point
        Point? closestPoint = GetClosestOnFormula(point);
        if (!closestPoint.HasValue) return -1;
        // Calculate the distance between the closest point and the given point
        double dx = closestPoint.Value.X - point.X;
        double dy = closestPoint.Value.Y - point.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        return distance;
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
            var Y = SolveForY(X);
            return new Point(X, Y[0]);
        }
        double nSlope = -1 / slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        var potential = potentialIntersect(nRay);
        if ((potential.X > p1.X && potential.X < p2.X) || (potential.X > p2.X && potential.X < p1.X)) return potential;
        if ((p1.X > p2.X && potential.X > p1.X) || (p1.X < p2.X && potential.X < p1.X)) return new Point(p1.X, p1.Y);
        if ((p2.X > p1.X && potential.X > p2.X) || (p2.X < p1.X && potential.X < p2.X)) return new Point(p2.X, p2.Y);
        return null;

    }

    public override Point? GetClosestOnFormula(Point point)
    {
        return GetClosestOnFormula(point.X, point.Y);
    }

    public override double[] SolveForX(double y)
    {
        if ((p1.Y > p2.Y && (y > p1.Y || y < p2.Y)) || (p1.Y < p2.Y && (y < p1.Y || y > p2.Y))) return Array.Empty<double>();
        return new double[] { (y - yIntercept) / slope };
    }

    public override double[] SolveForY(double x)
    {
        if ((p1.X > p2.X && (x > p1.X || x < p2.X)) || (p1.X < p2.X && (x < p1.X || x > p2.X))) return Array.Empty<double>();
        return new double[] { slope * x + yIntercept };
    }
}
