using Avalonia;
using StaticExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;
public class CircleFormula : ChangeListener, Formula
{
    double _radius;
    public double radius
    {
        get => _radius;
        set
        {
            _radius = value;
            foreach (var l in onChange) l();
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
            foreach (var l in onMove) l(value, _centerY, prev, _centerY);
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
            foreach (var l in onMove) l(_centerX, value, _centerX, prev);
        }
    }

    public CircleFormula(double radius, double centerX, double centerY)
    {
        this.radius = radius;
        this.centerX = centerX;
        this.centerY = centerY;
    }

    public double[] SolveForX(double y)
    {
        if (y > centerY + radius || y < centerY - radius) return new double[0];
        var x1 = centerX - Math.Sqrt(-(centerY - y).Pow(2) + radius.Pow(2));
        var x2 = centerX + Math.Sqrt(-(centerY - y).Pow(2) + radius.Pow(2));
        return new[] { x1, x2 };
    }

    public double[] SolveForY(double x)
    {
        if (x > centerX + radius || x < centerX - radius) return new double[0];
        var y1 = centerY - Math.Sqrt(-(centerX - x).Pow(2) + radius.Pow(2));
        var y2 = centerY + Math.Sqrt(-(centerX - x).Pow(2) + radius.Pow(2));
        return new[] { y1, y2 };
    }

    /// <summary>
    /// If px & py == cx & cy, returns null.
    /// </summary>
    /// <param name="px"></param>
    /// <param name="py"></param>
    /// <returns></returns>
    public Point? GetClosestOnFormula(double px, double py)
    {
        //Avoid divide by 0 error:
        if (px == centerX && py == centerY) return null;
        double vX = px - centerX;
        double vY = py - centerY;
        double magV = Math.Sqrt(vX * vX + vY * vY);
        double aX = centerX + vX / magV * radius;
        double aY = centerY + vY / magV * radius;
        return new Point(aX, aY);
    }

    public Point? GetClosestOnFormula(Point point)
    {
        return GetClosestOnFormula(point.X, point.Y);
    }
}
