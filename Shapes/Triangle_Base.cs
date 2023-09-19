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
using Dynamically.Menus.ContextMenus;
using Avalonia.Controls;
using System.Globalization;

namespace Dynamically.Shapes;

public partial class Triangle : DraggableGraphic
{

    public static readonly List<Triangle> all = new();

    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    public Segment con12;
    public Segment con13;
    public Segment con23;


    TriangleType _type = TriangleType.SCALENE;

    public TriangleType Type
    {
        get => _type;
        set => ChangeType(value);
    }


    public Circle? circumcircle;
    public Circle? incircle;
//public EquilateralTriangleFormula EquilateralFormula;
    public Triangle(Joint j1, Joint j2, Joint j3)
    {
        if (Exists(j1, j2, j3)) return;

        joint1 = j1;
        joint2 = j2;
        joint3 = j3;

        all.Add(this);


        con12 = joint1.Connect(joint2);
        con13 = joint1.Connect(joint3);
        con23 = joint2.Connect(joint3);

        foreach (var j in new[] { joint1, joint2, joint3 }) j.Roles.AddToRole(Role.TRIANGLE_Corner, this);
        foreach (var con in new[] { con12, con13, con23 }) con.Roles.AddToRole(Role.TRIANGLE_Side, this);

        foreach (var j in new[] { joint1, joint2, joint3 }) j.reposition();


        ContextMenu = new ContextMenu();
        Provider = new TriangleContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        OnMoved.Add((x, y, px, py) =>
        {
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
        OnDragged.Add(MainWindow.regenAll);

        MainWindow.BigScreen.Children.Add(this);

        MainWindow.regenAll(0, 0, 0, 0);
        Provider.Regenerate();
    }
    public Circle GenerateCircumCircle()
    {
        circumcircle = Tools.CircleFrom3Joints(joint1, joint2, joint3);
        circumcircle.center.Roles.AddToRole(Role.TRIANGLE_CircumCircleCenter, this);
        Provider.Regenerate();
        return circumcircle;
    }

    public Circle GenerateInCircle()
    {
        var stats = GetCircleStats();

        var circle = new Circle(new Joint(stats.x, stats.y), stats.r);
        circle.center.Draggable = false;
        circle.Draggable = false;
        incircle = circle;

        circle.center.Roles.AddToRole(Role.TRIANGLE_InCircleCenter, this);

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.OnMoved.Add(__RecalculateInCircle);
        }

        Provider.Regenerate();

        return circle;
    }

    Stats GetCircleStats()
    {
        // Calculate the lengths of the Triangle sides
        double a = joint2.DistanceTo(joint3);
        double b = joint1.DistanceTo(joint3);
        double c = joint1.DistanceTo(joint2);

        // Calculate the semiperimeter of the Triangle
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

    public Point GetIncircleCenter()
    {
        var s = GetCircleStats();
        return new Point(s.x, s.y);
    }

    public List<(TriangleType type, string details, double confidence)> SuggestTypes()
    {
        var l = new List<(TriangleType type, string details, double confidence)>();

        if (Type != TriangleType.EQUILATERAL && Math.Abs(60 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3)) < Settings.MakeEquilateralAngleOffset &&
            Math.Abs(60 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3)) < Settings.MakeEquilateralAngleOffset &&
            Math.Abs(60 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2)) < Settings.MakeEquilateralAngleOffset) l.Add((TriangleType.EQUILATERAL, "", 1 /* Chosen so it would often show at the top of the list */));
        else if (Type != TriangleType.EQUILATERAL)  // Don't suggest both isosceles & equilateral. does'nt make sense more often than not.
        {
            if (Type != TriangleType.ISOSCELES && con12.Length.IsSimilarTo(con13.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{con12} = {con13}", con12.Length.GetSimilarityPercentage(con13.Length)));
            if (Type != TriangleType.ISOSCELES && con12.Length.IsSimilarTo(con23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{con12} = {con23}", con12.Length.GetSimilarityPercentage(con23.Length)));
            if (Type != TriangleType.ISOSCELES && con23.Length.IsSimilarTo(con13.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{con23} = {con13}", con23.Length.GetSimilarityPercentage(con13.Length)));
        }

        if (Type != TriangleType.RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{joint1}{joint2}{joint3} = 90°", Tools.GetDegreesBetween3Points(joint1, joint2, joint3) / 90));
        if (Type != TriangleType.RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{joint2}{joint1}{joint3} = 90°", Tools.GetDegreesBetween3Points(joint2, joint1, joint3) / 90));
        if (Type != TriangleType.RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{joint1}{joint3}{joint2} = 90°", Tools.GetDegreesBetween3Points(joint1, joint3, joint2) / 90));

        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3)) < Settings.MakeRightAngleOffset && con12.Length.IsSimilarTo(con23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{joint1}{joint2}{joint3} = 90°, {con12} = {con23}", (Tools.GetDegreesBetween3Points(joint1, joint2, joint3) / 90 + con12.Length.GetSimilarityPercentage(con23.Length)) / 2));
        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3)) < Settings.MakeRightAngleOffset && con13.Length.IsSimilarTo(con23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{joint2}{joint1}{joint3} = 90°, {con13} = {con23}", (Tools.GetDegreesBetween3Points(joint2, joint1, joint3) / 90 + con13.Length.GetSimilarityPercentage(con23.Length)) / 2));
        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2)) < Settings.MakeRightAngleOffset && con13.Length.IsSimilarTo(con12.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{joint1}{joint3}{joint2} = 90°, {con13} = {con12}", (Tools.GetDegreesBetween3Points(joint1, joint3, joint2) / 90 + con13.Length.GetSimilarityPercentage(con12.Length)) / 2));


        return l;
    }
    public override double GetClosenessToCenter(Point point)
    {
        var stats = GetCircleStats();
        return point.DistanceTo(stats.x, stats.y);
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

        if (MainWindow.BigScreen.HoveredObject == this && (MainWindow.BigScreen.FocusedObject == this || MainWindow.BigScreen.FocusedObject is not IShape))
        {
            context.DrawGeometry(UIColors.ShapeHoverFill, null, geom);
        }
        else
        {
            context.DrawGeometry(UIColors.ShapeFill, null, geom);
        }
    }

    public bool IsDefinedBy(Joint j1, Joint j2, Joint j3)
    {
        var arr = new Joint[] { j1, j2, j3 };
        return arr.Contains(joint1) && arr.Contains(joint2) && arr.Contains(joint3);
    }
    public static bool Exists(Joint j1, Joint j2, Joint j3)
    {
        foreach (var triangle in all)
        {
            if (triangle.IsDefinedBy(j1, j2, j3)) return true;
        }
        return false;
    }
    public static bool Exists(char cid, char id1, char id2)
    {
        var c = Joint.GetJointById(cid);
        if (c == null) return false;
        var j1 = Joint.GetJointById(id1);
        if (j1 == null) return false;
        var j2 = Joint.GetJointById(id2);
        if (j2 == null) return false;
        return Exists(c, j1, j2);
    }
}

public enum TriangleType
{
    EQUILATERAL,
    ISOSCELES,
    RIGHT,
    ISOSCELES_RIGHT,
    SCALENE,
}

public struct Stats
{
    public double x, y, r;
}