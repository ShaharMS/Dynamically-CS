﻿using Avalonia;
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
    public double yIntercept
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
    public double slope
    {
        get => _slope;
        set
        {
            _slope = value;
            foreach (var l in OnChange) l();
        }
    }

    /// <summary>
    /// Useful when dealing with vertical rays, which cannot be represented accurately using slope & yItntercept.
    /// Only set when defining the ray using a point.
    /// </summary>
    public Point? ReferencePoint { get; private set; }

    public RayFormula(double yIntercept, double slope) : base()
    {
        this.yIntercept = yIntercept;
        this.slope = slope;
    }

    public RayFormula(Point pointOnRay, double slope) : base()
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

    public RayFormula(Point p1, Point p2) : base()
    {
        slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (p1.X > 0)
        {
            yIntercept = p1.Y - (slope * p1.X);
        }
        else
        {
            yIntercept = p1.Y + (slope * p1.X);
        }
        ReferencePoint = p1;
    }

    public RayFormula(double x1, double y1, double x2, double y2) : base() 
    {
        slope = (y2 - y1) / (x2 - x1);
        if (x1 > 0)
        {
            yIntercept = y1 - (slope * x1);
        }
        else
        {
            yIntercept = y1 + (slope * x1);
        }
        ReferencePoint = new Point(x1, y1);
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
        ReferencePoint = point;
    }

    public void Set(Point p1, Point p2)
    {
        slope = (p2.Y - p1.Y) / (p2.X - p1.X);
        if (p1.X > 0)
        {
            yIntercept = p1.Y - (slope * p1.X);
        }
        else
        {
            yIntercept = p1.Y + (slope * p1.X);
        }
        ReferencePoint = p1;
    }

    public void Set(double yIntercept, double slope)
    {
        this.yIntercept = yIntercept;
        this.slope = slope;
        ReferencePoint = null;
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
        ReferencePoint = pointOnRay;
    }

    public Point? Intersect(RayFormula formula)
    {
        if (formula == null) return null;
        if (formula.slope == slope) return null;

        var X = (yIntercept - formula._yIntercept) / (formula.slope - slope);
        var Y = SolveForY(X);

        if (Y.Length == 0) return null;
        return new Point(X, Y[0]);
    }

    public Point? Intersect(SegmentFormula formula) => formula.Intersect(this);

    public Point[] Intersect(CircleFormula formula)
    {
        var m = slope;
        var c = yIntercept;
        var a = formula.centerX;
        var b = formula.centerY;
        var r = formula.radius;

        // Get ready for a roller coaster!
        var x1 =
            (2 * b * m + 2 * a + Math.Sqrt((-2 * b * m - 2 * a).Pow(2) - 4 * (m.Pow(2) + 1) * (-r.Pow(2) + a.Pow(2) + b.Pow(2) + 2 * m * c - c.Pow(2) - 2 * b * c)))
            / (2 * (m.Pow(2) + 1));

        var x2 = 
            (2 * b * m + 2 * a - Math.Sqrt((-2 * b * m - 2 * a).Pow(2) - 4 * (m.Pow(2) + 1) * (-r.Pow(2) + a.Pow(2) + b.Pow(2) + 2 * m * c - c.Pow(2) - 2 * b * c)))
            / (2 * (m.Pow(2) + 1));

        // Roller coaster over, phew

        var y1 = SolveForY(x1);
        var y2 = SolveForY(x2);

        if (y1.Length == 0 && y2.Length == 0) return Array.Empty<Point>();
        else if (y1.Length == 0) return new[] {new Point(x2, y2[0])};
        else if (y2.Length == 0) return new[] {new Point(x1, y1[0])};
        else return new[] {new Point(x1, y1[0]), new Point(x2, y2[0])};
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
        double nSlope = -1 / slope;
        var nRay = new RayFormula(new Point(x, y), nSlope);
        return Intersect(nRay);
    }

    public override double[] SolveForX(double y)
    {
        if (double.IsNaN((y - yIntercept) / slope)) return Array.Empty<double>();
        return new double[] {(y - yIntercept) / slope};
    }

    public override double[] SolveForY(double x)
    {
        if (double.IsNaN(slope * x + yIntercept)) return Array.Empty<double>();
        return new double[] {slope * x + yIntercept};
    }

    public override void Move(double x, double y)
    {
        var pYI = _yIntercept;
        if (x > 0)
        {
            _yIntercept = y - (slope * x);
        }
        else
        {
            _yIntercept = y + (slope * x);
        }
        foreach (var l in OnMoved) l(0, pYI, 0, _yIntercept);
        foreach (var l in OnChange) l();
    }
}
