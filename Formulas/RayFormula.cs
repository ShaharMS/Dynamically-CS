using Avalonia;
using Dynamically.Backend;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Dynamically.Formulas;

public class RayFormula : ChangeListener, Formula
{
    double _yIntercept;
    public double yIntercept
    {
        get => _yIntercept;
        set
        {
            var prev = _yIntercept;
            _yIntercept = value;
            foreach (var l in OnMove) l(0, value, 0, prev);
        }
    }
    double _slope;
    public double slope
    {
        get => _slope;
        set
        {
            _slope = value;
            foreach (var l in OnChange) l();
        }
    }

    public RayFormula(double yIntercept, double slope)
    {
        this.yIntercept = yIntercept;
        this.slope = slope;
    }

    public RayFormula(Point pointOnRay, double slope) 
    { 
        if (pointOnRay.X > 0)
        {
            yIntercept = pointOnRay.Y - (slope * pointOnRay.X);
        }
        else
        {
            yIntercept = pointOnRay.Y + (slope * pointOnRay.X);
        }
        this.slope = slope;
    }

    public RayFormula(Point p1, Point p2)
    {
        slope = (p2.Y - p1.Y) / (p2.X - p2.X);
        if (p1.X > 0)
        {
            yIntercept = p1.Y - (slope * p1.X);
        }
        else
        {
            yIntercept = p1.Y + (slope * p1.X);
        }
    }

    /// <summary>
    /// Keeps slope
    /// </summary>
    /// <param name="point"></param>
    public void ChangePositionByPoint(Point point)
    {
        if (point.X > 0)
        {
            yIntercept = point.Y - (slope * point.X);
        }
        else
        {
            yIntercept = point.Y + (slope * point.X);
        }
    }

    public void Set(Point p1, Point p2)
    {
        slope = (p2.Y - p1.Y) / (p2.X - p2.X);
        if (p1.X > 0)
        {
            yIntercept = p1.Y - (slope * p1.X);
        }
        else
        {
            yIntercept = p1.Y + (slope * p1.X);
        }
    }

    public void Set(double yIntercept, double slope)
    {
        this.yIntercept = yIntercept;
        this.slope = slope;
    }

    public void Set(Point pointOnRay, double slope)
    {
        if (pointOnRay.X > 0)
        {
            yIntercept = pointOnRay.Y - (slope * pointOnRay.X);
        }
        else
        {
            yIntercept = pointOnRay.Y + (slope * pointOnRay.X);
        }
        this.slope = slope;
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula._yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);

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
    public Point? GetClosestOnFormula(double x, double y)
    {
        double nSlope = -1 / slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        return Intersect(nRay);
    }

    public Point? GetClosestOnFormula(Point point)
    {
        return GetClosestOnFormula(point.X, point.Y);
    }

    public double[] SolveForX(double y)
    {
        return new double[] {(y - yIntercept) / slope};
    }

    public double[] SolveForY(double x)
    {
        return new double[] {slope * x + yIntercept};
    }
}
