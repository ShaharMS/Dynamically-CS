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
    /// If px & py == cx & cy, returns null.
    /// </summary>
    /// <param name="px"></param>
    /// <param name="py"></param>
    /// <returns></returns>
    public override Point? GetClosestOnFormula(double px, double py)
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
        var X = inputX; var Y = inputY;

        var rads = new Point(centerX, centerY).RadiansTo(X, Y);

        X = centerX + radius * Math.Cos(rads);
        Y = centerY + radius * Math.Sin(rads);

        return (X, Y);
    }
}
