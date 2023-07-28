using Avalonia;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend;
using System;
using System.Collections.Generic;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Screens;
using Avalonia.Media;
using Dynamically.Design;
using System.Linq;

namespace Dynamically.Shapes;

public partial class Triangle : DraggableGraphic, IDismantable, IShape
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    public Segment con12;
    public Segment con13;
    public Segment con23;

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

        foreach (var j in new[] { joint1, joint2, joint3 }) j.Roles.AddToRole(Role.TRIANGLE_Corner, this);

        con12 = joint1.Connect(joint2);
        con13 = joint1.Connect(joint3);
        con23 = joint2.Connect(joint3);

        foreach (var j in new[] { joint1, joint2, joint3 }) j.reposition();

        OnMoved.Add((x, y, px, py) => {
            if (joint1.Anchored || joint2.Anchored || joint3.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            joint1.X += x - px;
            joint2.X += x - px;
            joint3.X += x - px;
            joint1.Y += y - py;
            joint2.Y += y - py;
            joint3.Y += y - py;
            EQ_temp_incircle_center = new Point(EQ_temp_incircle_center.X + x - px, EQ_temp_incircle_center.Y + y - py);
            joint1.DispatchOnMovedEvents(joint1.X, joint1.Y, joint1.X, joint1.Y);
            joint2.DispatchOnMovedEvents(joint2.X, joint2.Y, joint2.X, joint2.Y);
            joint3.DispatchOnMovedEvents(joint3.X, joint3.Y, joint3.X, joint3.Y);
            con12.reposition();
            con13.reposition();
            con23.reposition();
            this.SetPosition(0, 0);
        });

        MainWindow.BigScreen.Children.Add(this);
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

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.Roles.RemoveFromRole(Role.TRIANGLE_Corner, this);
        }
    }

    public Circle GenerateCircumCircle()
    {
        return Tools.CircleFrom3Joints(joint1, joint2, joint3);
    }

    public Circle GenerateInCircle()
    {
        var stats = GetCircleStats();

        var circle = new Circle(new Joint(stats.x, stats.y), stats.r);
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
        incircle.UpdateFormula();
        incircle.InvalidateVisual();
        foreach (var listener in incircle.center.OnMoved) listener(incircle.center.X, incircle.center.Y, mouseX, mouseY);
    }
    TriangleType ChangeType(TriangleType type)
    {
        // Actual shape modification
        switch (type)
        {
            case TriangleType.EQUILATERAL:
                void MakeEquilateralRelativeToABC(Joint A, Joint B, Joint C)
                {
                    Log.Write(B);
                    // AB and BC are the most similar to each other, so B was chosen. Now, reset the angle
                    // We'll do this by averaging AB and BC, resetting their length, and BC will 
                    // automatically be the same length as AC, because of equilateral definition.

                    // To Fix the angle, we'll take the point which creates the angle closest to 0/180, and preserve it
                    var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
                    var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);
                    var dist = (A.DistanceTo(B) + B.DistanceTo(C)) / 2;
                    if (Math.Abs(radBA % Math.PI) < Math.Abs(radBC % Math.PI)) // BA should be preserved
                    {
                        var rad = radBA + Math.PI / 3;
                        if (radBA > radBC) rad -= Math.PI / 1.5;
                        Log.Write(rad * 180 / Math.PI, "radBA:", radBA * 180 / Math.PI, "radBC:", radBC * 180 / Math.PI);
                        C.X = B.X + dist * Math.Cos(rad);
                        C.Y = B.Y + dist * -Math.Sin(rad);

                        // Don't forget to set length of AB too!
                        rad -= Math.PI / 3;
                        A.X = B.X + dist * Math.Cos(rad);
                        A.Y = B.Y + dist * -Math.Sin(rad);
                    }
                    else
                    {
                        var rad = radBC + Math.PI / 3;
                        if (radBC > radBA) rad -= Math.PI / 1.5;
                        Log.Write(rad * 180 / Math.PI, "radBC:", radBC * 180 / Math.PI, "radBA:", radBA * 180 / Math.PI);
                        A.X = B.X + dist * Math.Cos(rad);
                        A.Y = B.Y + dist * -Math.Sin(rad);

                        // Don't forget to set length of BC too!
                        rad -= Math.PI / 3;
                        C.X = B.X + dist * Math.Cos(rad);
                        C.Y = B.Y + dist * -Math.Sin(rad);
                    }

                    EQ_temp_incircle_center = new Point(GetCircleStats().x, GetCircleStats().y);
                }
                var a_ABBC_SimilarityOfSides = Math.Abs(con12.Length - con23.Length);
                var a_ACCB_ClosenessTo60Deg = Math.Abs(con13.Length - con23.Length);
                var a_BAAC_ClosenessTo60Deg = Math.Abs(con13.Length - con12.Length);
                if (a_ABBC_SimilarityOfSides < a_ACCB_ClosenessTo60Deg && a_ABBC_SimilarityOfSides < a_BAAC_ClosenessTo60Deg) MakeEquilateralRelativeToABC(joint1, joint2, joint3);
                else if (a_ACCB_ClosenessTo60Deg < a_ABBC_SimilarityOfSides && a_ACCB_ClosenessTo60Deg < a_BAAC_ClosenessTo60Deg) MakeEquilateralRelativeToABC(joint1, joint3, joint2);
                else MakeEquilateralRelativeToABC(joint2, joint1, joint3);
                break;
            case TriangleType.ISOSCELES:
                void MakeIsocelesRelativeToABC(Joint A, Joint B, Joint C)
                {
                    // ∠ABC is the head angle, therefore its position should be preserved, and should be where the two equals start from.
                    // We'll do this by averaging AB and BC, resetting their length, and BC will 
                    // automatically be the same length as AC, because of equilateral definition.

                    // To correct the distances, we'll  make sure the moving joint when setting connection length is not A:

                    var distance = Math.Max(B.DistanceTo(A), B.DistanceTo(C));
                    var radBA = B.RadiansTo(A);
                    A.X = B.X + distance * Math.Cos(radBA);
                    A.Y = B.Y + distance * Math.Sin(radBA);
                    var radBC = B.RadiansTo(C);
                    C.X = B.X + distance * Math.Cos(radBC);
                    C.Y = B.Y + distance * Math.Sin(radBC);

                    ISO_origin = B;
                    // Now, After equating the two sides, we're pretty much dones - we've reached teh definition of an isoceles triangle
                }
                var con12_to_con13_Diff = Math.Abs(con12.Length - con13.Length);
                var con12_to_con23_Diff = Math.Abs(con12.Length - con23.Length);
                var con13_to_con23_Diff = Math.Abs(con13.Length - con23.Length);
                if (con12_to_con23_Diff < con13_to_con23_Diff && con12_to_con23_Diff < con12_to_con13_Diff) MakeIsocelesRelativeToABC(joint1, joint2, joint3);
                else if (con12_to_con13_Diff < con13_to_con23_Diff && con12_to_con13_Diff < con12_to_con23_Diff) MakeIsocelesRelativeToABC(joint2, joint1, joint3);
                else MakeIsocelesRelativeToABC(joint1, joint3, joint2);
                break;
            case TriangleType.RIGHT:
                void MakeRightRelativeToABC(Joint A, Joint B, Joint C)
                {
                    // ∠ABC is the most similar to 90deg, therefore it should be preserved.

                    // Fixing the angle is easy, its just editing either A or C
                    // But, for user comfort, we'll modify the point which creates the angle with y = 0 least similar to 0/180
                    var radBA = B.RadiansTo(A);
                    var radBC = B.RadiansTo(C);
                    if (Math.Abs(radBA % Math.PI) < Math.Abs(radBC % Math.PI)) // BA should be preserved
                    {
                        var dist = B.DistanceTo(C);
                        var XPosOffset = dist * Math.Cos(radBA + (radBC < radBA ? Math.PI / 2 : -Math.PI / 2));
                        var YPosOffset = dist * Math.Sin(radBA + (radBC < radBA ? Math.PI / 2 : -Math.PI / 2));
                        C.X = B.X + XPosOffset;
                        C.Y = B.Y + YPosOffset;
                    }
                    else
                    {
                        var dist = B.DistanceTo(A);
                        var XPosOffset = dist * Math.Cos(radBC + ( radBA < radBC ? Math.PI / 2 : -Math.PI / 2));
                        var YPosOffset = dist * Math.Sin(radBC + ( radBA < radBC ? Math.PI / 2 : -Math.PI / 2));
                        A.X = B.X + XPosOffset;
                        A.Y = B.Y + YPosOffset;
                    }

                    R_origin = B;
                    // And we're done :)
                }

                var a_ABC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3));
                Log.Write(Tools.GetDegreesBetween3Points(joint1, joint2, joint3));
                var a_ACB_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2));
                Log.Write(Tools.GetDegreesBetween3Points(joint1, joint3, joint2));
                var a_BAC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3));
                Log.Write(Tools.GetDegreesBetween3Points(joint2, joint1, joint3));
                if (a_ABC_ClosenessTo90Deg < a_ACB_ClosenessTo90Deg && a_ABC_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) MakeRightRelativeToABC(joint1, joint2, joint3);
                else if (a_ACB_ClosenessTo90Deg < a_ABC_ClosenessTo90Deg && a_ACB_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) MakeRightRelativeToABC(joint1, joint3, joint2);
                else MakeRightRelativeToABC(joint2, joint1, joint3);
                break;
            case TriangleType.SCALENE:
                break;
        }

        // Event Listeners
        switch (type)
        {
            case TriangleType.EQUILATERAL:
                // Remove ISOCELES
                joint1.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint3, joint2, joint1, px, py));
                // Remove RIGHT
                joint1.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint3, joint2, joint1, px, py));
                // Add EQUILATERAL
                joint1.OnMoved.Add((_, _, px, py) => Equilateral_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Add((_, _, px, py) => Equilateral_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Add((_, _, px, py) => Equilateral_OnJointMove(joint3, joint2, joint1, px, py));
                break;
            case TriangleType.ISOSCELES:
                // Remove EQUILATERAL:
                joint1.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint3, joint2, joint1, px, py));
                // Add ISOCELES
                joint1.OnMoved.Add((_, _, px, py) => Isoceles_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Add((_, _, px, py) => Isoceles_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Add((_, _, px, py) => Isoceles_OnJointMove(joint3, joint2, joint1, px, py));
                break;
            case TriangleType.RIGHT:
                // Remove ISOCELES
                joint1.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint3, joint2, joint1, px, py));
                // Remove EQUILATERAL:
                joint1.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint3, joint2, joint1, px, py));
                // Add RIGHT
                joint1.OnMoved.Add((_, _ , px, py) => Right_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Add((_, _ , px, py) => Right_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Add((_, _ , px, py) => Right_OnJointMove(joint3, joint2, joint1, px, py));
                break;
            case TriangleType.SCALENE:
                // Remove ISOCELES
                joint1.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Isoceles_OnJointMove(joint3, joint2, joint1, px, py));
                // Remove RIGHT
                joint1.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Right_OnJointMove(joint3, joint2, joint1, px, py));
                // Remove EQUILATERAL:
                joint1.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint1, joint2, joint3, px, py));
                joint2.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint2, joint1, joint3, px, py));
                joint3.OnMoved.Remove((_, _, px, py) => Equilateral_OnJointMove(joint3, joint2, joint1, px, py));
                break;
        }
        joint1.reposition(); joint2.reposition(); joint3.reposition();
        return _type = type;
    }


    public override string ToString()
    {
        return $"△{joint1.Id}{joint2.Id}{joint3.Id}";
    }

    public override double Area()
    {
        return Math.Abs(con12.Length * con12.Formula.DistanceTo(joint3) / 2);
    }

    public override double GetClosenessToCenter(Point point)
    {
        var stats = GetCircleStats();
        return point.DistanceTo(stats.x, stats.y);
    }

    public override bool Overlaps(Point p)
    {
        double areaABC = 0.5 * Math.Abs(joint1.ScreenX * (joint2.ScreenY - joint3.ScreenY) +
                                       joint2.ScreenX * (joint3.ScreenY - joint1.ScreenY) +
                                       joint3.ScreenX * (joint1.ScreenY - joint2.ScreenY));

        double areaPBC = 0.5 * Math.Abs(p.X * (joint2.ScreenY - joint3.ScreenY) +
                                      joint2.ScreenX * (joint3.ScreenY - p.Y) +
                                      joint3.ScreenX * (p.Y - joint2.ScreenY));

        double areaPCA = 0.5 * Math.Abs(joint1.ScreenX * (p.Y - joint3.ScreenY) +
                                      p.X * (joint3.ScreenY - joint1.ScreenY) +
                                      joint3.ScreenX * (joint1.ScreenY - p.Y));

        double areaPAB = 0.5 * Math.Abs(joint1.ScreenX * (joint2.ScreenY - p.Y) +
                                      joint2.ScreenX * (p.Y - joint1.ScreenY) +
                                      p.X * (joint1.ScreenY - joint2.ScreenY));

        // If the sum of the sub-triangle areas is equal to the triangle area, the point is inside the triangle
        return Math.Abs(areaPBC + areaPCA + areaPAB - areaABC) < 0.0001; // Adjust epsilon as needed for floating-point comparison
    }

    public override void Render(DrawingContext context)
    {
        var geom = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = joint1,
            IsClosed = true,
            IsFilled = true
        };

        figure?.Segments?.Add(new LineSegment { Point = joint2 });
        figure?.Segments?.Add(new LineSegment { Point = joint3 });

        geom.Figures.Add(figure);

        if (MainWindow.BigScreen.HoveredObject == this)
        {
            context.DrawGeometry(UIColors.ShapeHoverFill, null, geom);
        } else
        {
            context.DrawGeometry(UIColors.ShapeFill, null, geom);
        }
    }

/*
    public static bool operator ==(Triangle rhs, Triangle lhs)
    {
        var arr = new Joint[] { rhs.joint1, rhs.joint2, rhs.joint3 };
        return arr.Contains(lhs.joint1) && arr.Contains(lhs.joint2) && arr.Contains(lhs.joint3);
    }
    public static bool operator !=(Triangle rhs, Triangle lhs)
    {
        var arr = new Joint[] { rhs.joint1, rhs.joint2, rhs.joint3 };
        return !arr.Contains(lhs.joint1) || !arr.Contains(lhs.joint2) || !arr.Contains(lhs.joint3);
    }
*/
    public bool IsDefinedBy(Joint j1, Joint j2, Joint j3)
    {
        var arr = new Joint[] { j1, j2, j3 };
        return arr.Contains(joint1) && arr.Contains(joint2) && arr.Contains(joint3);
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