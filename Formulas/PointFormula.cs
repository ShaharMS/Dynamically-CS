﻿using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Geometry.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class PointFormula : Formula
{
    Vertex? _driver;
    double _x;    
    public double X
    {
        get => _x; 
        set => Move(_x, _y);
    }

    double _y;
    public double Y
    {
        get => _y;
        set => Move(_x, _y);
    }

    public void QuietSet(double x, double y)
    {
        _x = x;
        _y = y;
    }

    public PointFormula(double x, double y)
    {
        Move(x, y);
    }

    public PointFormula(Point p) : this(p.X, p.Y) { }

    public PointFormula(Vertex j) : this(j.X, j.Y)
    {
        _driver = j;
        j.OnMoved.Add(Move);
    }

    public void Set(Vertex j)
    {
        _driver?.OnMoved.Remove(Move);
        _driver = j;
        j.OnMoved.Add(Move);
        Move(j.X, j.Y);
    }

    public override Point? GetClosestOnFormula(double x, double y)
    {
        return new Point(X, Y);
    }

    public override void Move(double x, double y)
    {
        double px = _x, py = _y;
        _x = x; _y = y;

        foreach (var l in OnMoved) l(_x, _y, px, py);
        foreach (var l in OnChange) l();

        UpdateFollowers();
    }

    private void Move(double x, double y, double px, double py)
    {
        _ = px; _ = py;
        Move(x, y);
    }

    public override double[] SolveForX(double y)
    {
        if (y != Y) return Array.Empty<double>();
        return new[] { X };
    }

    public override double[] SolveForY(double x)
    {
        if (x != X) return Array.Empty<double>();
        return new[] { Y };
    }
}
