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

    public Circle GenerateCircleAroundTriangle()
    {
        return Tools.CircleFrom3Joints(joint1, joint2, joint3);
    }

    public Circle GenerateCircleInsideTriangle()
    {
        Joint j1, j2;
        Point p1, p2;

        /*  I'll use the angle bisector theorem to find the point on the side opposing the angle
        
         (c - connection/side length, c3 - current opposer)
         (r - point of seperation in the opposing side)
         c1/r = c2/c3-r
         c2*r = c1*c3 - c1*r
         r = (c1*c3)/(c1 + c2)

         After finding the point at which the bisector collides with its opposing side,
         I'll create two rays from 2 bisectors, and find their collision.
         For radius, ill dinf the distance from the center to a side on the triangle.
        */
        var pairs = new Joint[][] { new Joint[] { joint3, joint1 }, new Joint[] { joint2, joint3 }};
        double ratio;
        double l1 = pairs[0][0].distanceTo(pairs[0][1]);
        double l2 = pairs[1][0].distanceTo(pairs[1][1]);
        double l3 = pairs[2][0].distanceTo(pairs[2][1]);

        
        var r1 = (l3*l1)/(l2+l3); // Opposer - l1
        var r2 = (l1*l2)/(l1+l3); // Opposer - l2
        var r3 = (l2*l3)/(l2+l1); // Opposer - l3

        var x1 = (r1 * pairs[0][0].x + (l1 - r1) * pairs[0][1].x) / l1;
        var y1 = (r1 * pairs[0][0].y + (l1 - r1) * pairs[0][1].y) / l1;
        p1 = new Point(x1, y1);

        var x2 = (r2 * pairs[1][0].x + (l2 - r2) * pairs[1][1].x) / l2;
        var y2 = (r2 * pairs[1][0].y + (l2 - r2) * pairs[1][1].y) / l2;
        p2 = new Point(x2, y2);

        var circle = Tools.CircleFrom3Joints(j1, j2, j3);
        
        var ray1 = new RayFormula(p1, p2);
        var ray2 = new RayFormula(p2, p3);
        var ray3 = new RayFormula(p3, p1);
        
        joint1.geometricPosition.Add(ray1);
        joint2.geometricPosition.Add(ray2);
        joint3.geometricPosition.Add(ray3);
        
        return circle;
        //return null;
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