using Avalonia;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend;
using System;
using System.Collections.Generic;
using Dynamically.Backend.Helpers;

namespace Dynamically.Shapes;

public class Triangle : DraggableGraphic, IDismantable, IRoleMapAddable
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    public Connection con12;
    public Connection con13;
    public Connection con23;

    private Circle? incircle;

    TriangleType _type = TriangleType.SCALENE;

    public TriangleType Type
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

        Log.Write(j1.Connections.Count, j2.Connections.Count, j3.Connections.Count);

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.Roles.AddToRole(Role.TRIANGLE_Corner, this);
            j.reposition();
        }
    }

    public void Dismantle()
    {
        if (joint1.GotRemoved) joint1.Disconnect(joint2, joint3);
        if (joint2.GotRemoved) joint2.Disconnect(joint1, joint3);
        if (joint3.GotRemoved) joint3.Disconnect(joint2, joint1);

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.OnMoved.Remove(RecalcuateInCircle);
        }
        if (incircle != null)
        {
            incircle.Draggable = true;
            incircle.center.Draggable = true;
        }

    }

    public Circle GenerateCircumCircle()
    {
        return Tools.CircleFrom3Joints(joint1, joint2, joint3);
    }

    public Circle GenerateInCircle()
    {
        var stats = GetCircleStats();

        Circle circle = new Circle(new Joint(stats.x, stats.y), stats.r);
        circle.center.Draggable = false;
        circle.center.Roles.AddToRole(Role.CIRCLE_Center, circle);
        circle.Draggable = false;
        incircle = circle;

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.OnMoved.Add(RecalcuateInCircle);
        }

        return circle;
    }

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

    void RecalcuateInCircle(double ux, double uy, double mouseX, double mouseY)
    {
        var stats = GetCircleStats();
        if (incircle == null) return;
        incircle.center.X = stats.x;
        incircle.center.Y = stats.y;
        incircle.radius = stats.r;
        incircle.updateFormula();
        incircle.InvalidateVisual();
        foreach (var listener in incircle.center.OnMoved) listener(incircle.center.X, incircle.center.Y, mouseX, mouseY);
    }
    TriangleType ChangeType(TriangleType type)
    {
        switch (type)
        {
            case TriangleType.EQUILATERAL:
                void MakeEquilateralRelativeToABC(Joint A, Joint B, Joint C)
                {
                    // ∠ABC is the most similar to 60deg, therefore it should be preserved.
                    // We'll do this by averaging AB and BC, resetting their length, and BC will 
                    // automatically be the same length as AC, because of equilateral definition.

                    // To "Fix" the angle, we'll calculate four points, which create the angle of 60, or, PI/3:
                    // First, find the angle of AB with the X axis:
                    double radians = Math.Atan2(A.Y - B.Y, A.X - B.X);

                    // Then, find the candidates: (x: +-d*cos(radians + PI/3), y: +-d*sin(radians + PI/3))
                    // To find d, we'll average AB and BC:
                    double averageDistance = (A.DistanceTo(B) + B.DistanceTo(C)) / 2;
                    Point[] potentialPositions = new Point[4];
                    int currentIndex = 0;
                    foreach (int sinSign in new[] { 1, -1 })
                    {
                        foreach (int cosSign in new[] { 1, -1 })
                        {
                            potentialPositions[currentIndex] = new Point(
                                cosSign * averageDistance * Math.Cos(radians + Math.PI / 3),
                                sinSign * averageDistance * Math.Sin(radians + Math.PI / 3)
                            );
                            currentIndex++;
                        }
                    }
                    // The candidate who's distance to point C is the shortest, is the expected one:
                    double minDistance = double.PositiveInfinity;
                    Point chosen = new Point(-1, -1); // just a filler
                    foreach (Point candidate in potentialPositions)
                    {
                        var d = candidate.DistanceTo(C);
                        if (d < minDistance)
                        {
                            minDistance = d;
                            chosen = candidate;
                        }
                    }
                    // Now, set C:
                    C.X = chosen.X;
                    C.Y = chosen.Y;

                    // After translating C to the correct position, we need to reevaluate AB's length, since it needs to match (AB + BC) / 2
                    // Since B is the origin, we need to make sure the length manipulation happens to A, not B:
                    con12.joint1 = B;
                    con12.joint2 = A;
                    con12.Length = averageDistance;

                    // Now, because one angle is 60deg, and its incased in two sides of the same length, we must have an equilateral
                    // (isosceles with its head angle = 60deg)
                    // Were pretty much done :)
                }
                var a_ABC_ClosenessTo60Deg = Math.Abs(60 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3));
                var a_ACB_ClosenessTo60Deg = Math.Abs(60 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2));
                var a_BAC_ClosenessTo60Deg = Math.Abs(60 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3));
                if (a_ABC_ClosenessTo60Deg < a_ACB_ClosenessTo60Deg && a_ABC_ClosenessTo60Deg < a_BAC_ClosenessTo60Deg) MakeEquilateralRelativeToABC(joint1, joint2, joint3);
                else if (a_ACB_ClosenessTo60Deg < a_ABC_ClosenessTo60Deg && a_ACB_ClosenessTo60Deg < a_BAC_ClosenessTo60Deg) MakeEquilateralRelativeToABC(joint1, joint3, joint2);
                else MakeEquilateralRelativeToABC(joint2, joint1, joint3);
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