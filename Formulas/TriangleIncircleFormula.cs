using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class TriangleIncircleFormula : Formula
{
    public Triangle Triangle { get; }

    public Circle? Circle { get; set; }

    public Point Center => GetIncircleCenter();

    public double CenterX => Center.X;

    public double CenterY => Center.Y;

    public double Radius => GetCircleStats().r;
    public TriangleIncircleFormula(Triangle triangle, Circle? Incircle)
    {
        Triangle = triangle;
        Triangle.Vertex1.OnMoved.Add((_, _, _, _) => { foreach (var l in OnChange) l(); });
        Triangle.Vertex2.OnMoved.Add((_, _, _, _) => { foreach (var l in OnChange) l(); });
        Triangle.Vertex3.OnMoved.Add((_, _, _, _) => { foreach (var l in OnChange) l(); });
        Circle = Incircle;
    }

    public override double[] SolveForX(double y)
    {
        return new double[] { Center.X };
    }

    public override double[] SolveForY(double x)
    {
        return new double[] { Center.Y };
    }

    public override Point? GetClosestOnFormula(double x, double y)
    {
        return Center;
    }

    public override void Move(double x, double y)
    {
        var currentCenter = Center;
        Triangle.Vertex1.X += x - currentCenter.X;
        Triangle.Vertex1.Y += y - currentCenter.Y;
        Triangle.Vertex2.X += x - currentCenter.X;
        Triangle.Vertex2.Y += y - currentCenter.Y;
        Triangle.Vertex3.X += x - currentCenter.X;
        Triangle.Vertex3.Y += y - currentCenter.Y;
    }

    public override (double X, double Y) UpdateVertexPosition(double inputX, double inputY)
    {
        if (Circle != null) Circle.Radius = Radius;
        return (Center.X, Center.Y);
    }

    Stats GetCircleStats()
    {
        // Calculate the lengths of the Triangle sides
        double a = Triangle.Vertex2.DistanceTo(Triangle.Vertex3);
        double b = Triangle.Vertex1.DistanceTo(Triangle.Vertex3);
        double c = Triangle.Vertex1.DistanceTo(Triangle.Vertex2);

        // Calculate the semiperimeter of the Triangle
        double s = (a + b + c) / 2;

        // Calculate the Radius of the inscribed circle
        double radius = Math.Sqrt((s - a) * (s - b) * (s - c) / s);

        // Calculate the coordinates of the Center of the inscribed circle
        double centerX = (a * Triangle.Vertex1.X + b * Triangle.Vertex2.X + c * Triangle.Vertex3.X) / (a + b + c);
        double centerY = (a * Triangle.Vertex1.Y + b * Triangle.Vertex2.Y + c * Triangle.Vertex3.Y) / (a + b + c);
        return new Stats
        {
            x = centerX,
            y = centerY,
            r = radius
        };
    }

    public Point GetIncircleCenter()
    {
        var s = GetCircleStats();
        return new Point(s.x, s.y);
    }
}
