using Avalonia;
using Dynamically.Formulas;
using GeometryBackend;
using GraphicsBackend;
using StaticExtensions;
using System;

namespace Dynamically.Shapes;

public class Triangle : DraggableGraphic
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    TriangleType _type = TriangleType.SCALENE;

    public TriangleType type
    {
        get => _type;
        set => ChangeType(value);
    }

    public Triangle(Joint j1, Joint j2, Joint j3)
    {
        joint1 = j1;
        joint2 = j2;
        joint3 = j3;

        joint1.Connect(joint2, joint3);
        joint2.Connect(joint3);
    }

    public Circle GenerateCircumCircle()
    {
        return Tools.CircleFrom3Joints(joint1, joint2, joint3);
    }

    public Circle GenerateInscribedCircle()
    {
        Stats GetCircleStats()
        {
            // Calculate the lengths of the triangle sides
            double a = joint2.DistanceTo(joint3);
            double b = joint1.DistanceTo(joint3);
            double c = joint1.DistanceTo(joint2);

            // Calculate the semiperimeter of the triangle
            double s = (a + b + c) / 2;

            // Calculate the radius of the inscribed circle
            double radius = Math.Sqrt((s - a) * (s - b) * (s - c) / s);

            // Calculate the coordinates of the center of the inscribed circle
            double centerX = (a * joint1.x + b * joint2.x + c * joint3.x) / (a + b + c);
            double centerY = (a * joint1.y + b * joint2.y + c * joint3.y) / (a + b + c);

            return new Stats
            {
                x = centerX,
                y = centerY,
                r = radius
            };
        }
        var stats = GetCircleStats();

        Circle circle = new Circle(new Joint(stats.x, stats.y), stats.r);
        circle.center.draggable = false;
        circle.draggable = false;

        foreach (var j in new[] {joint1, joint2, joint3})
        {
            j.onMoved.Add((double _, double _, double _, double _) =>
            {
                var stats = GetCircleStats();
                circle.center.x = stats.x;
                circle.center.y = stats.y;
                circle.radius = stats.r;
                circle.updateFormula();
                circle.InvalidateVisual();
            });
        }

        return circle;
    }
    TriangleType ChangeType(TriangleType type)
    {
        return type;
    }
}

public enum TriangleType
{
    EQUILATERAL,
    ISOSCELES,
    RIGHT,
    SCALENE,
}

public struct Stats
{
    public double x, y, r;
}