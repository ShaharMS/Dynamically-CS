using Avalonia;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend;
using System;

namespace Dynamically.Shapes;

public class Triangle : DraggableGraphic
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    public Connection con12;
    public Connection con13;
    public Connection con23;

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

        con12 = joint1.Connect(joint2);
        con13 = joint1.Connect(joint3);
        con23 = joint2.Connect(joint3);
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
            double centerX = (a * joint1.X + b * joint2.X + c * joint3.X) / (a + b + c);
            double centerY = (a * joint1.Y + b * joint2.Y + c * joint3.Y) / (a + b + c);

            return new Stats
            {
                x = centerX,
                y = centerY,
                r = radius
            };
        }
        var stats = GetCircleStats();

        Circle circle = new Circle(new Joint(stats.x, stats.y), stats.r);
        circle.center.Draggable = false;
        circle.Draggable = false;

        foreach (var j in new[] {joint1, joint2, joint3})
        {
            j.OnMoved.Add((double _, double _, double _, double _) =>
            {
                var stats = GetCircleStats();
                circle.center.X = stats.x;
                circle.center.Y = stats.y;
                circle.radius = stats.r;
                circle.updateFormula();
                circle.InvalidateVisual();
            });
        }

        return circle;
    }
    TriangleType ChangeType(TriangleType type)
    {
        switch (type) {
            case TriangleType.EQUILATERAL:
                var a123 = Tools.GetDegreesBetween3Points(joint1, joint2, joint3);
                var a132 = Tools.GetDegreesBetween3Points(joint1, joint3, joint2);
                var a213 = Tools.GetDegreesBetween3Points(joint2, joint1, joint3);
                break;
            case TriangleType.ISOSCELES:
                break;
            case TriangleType.RIGHT:
                break;
            case TriangleType.SCALENE:
                break;
        }
        return _type = type;
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