using Avalonia;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend;
using System;
using System.Collections.Generic;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Containers;
using Avalonia.Media;
using Dynamically.Design;
using System.Linq;
using Dynamically.Menus.ContextMenus;
using Avalonia.Controls;
using System.Globalization;

namespace Dynamically.Shapes;

public partial class Triangle : DraggableGraphic
{

    public static readonly List<Triangle> All = new();

    public Vertex Vertex1;
    public Vertex Vertex2;
    public Vertex Vertex3;

    public Segment Segment12;
    public Segment Segment13;
    public Segment Segment23;


    TriangleType _type = TriangleType.SCALENE;

    public TriangleType Type
    {
        get => _type;
        set => ChangeType(value);
    }


    public Circle? Circumcircle;
    public Circle? Incircle;

    public Triangle(Vertex j1, Vertex j2, Vertex j3) : base(j1.ParentBoard)
    {
        All.Add(this);

        Vertex1 = j1;
        Vertex2 = j2;
        Vertex3 = j3;

        Segment12 = Vertex1.Connect(Vertex2);
        Segment13 = Vertex1.Connect(Vertex3);
        Segment23 = Vertex2.Connect(Vertex3);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3 }) j.Roles.AddToRole(Role.TRIANGLE_Corner, this);
        foreach (var con in new[] { Segment12, Segment13, Segment23 }) con.Roles.AddToRole(Role.TRIANGLE_Side, this);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3 }) j.Reposition();


        ContextMenu = new ContextMenu();
        Provider = new TriangleContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        OnDragStart.Add(() => { if (!IsMovable()) CurrentlyDragging = false; });
        OnMoved.Add((x, y, px, py) =>
        {
            if (Vertex1.Anchored || Vertex2.Anchored || Vertex3.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            Vertex1.X += x - px;
            Vertex2.X += x - px;
            Vertex3.X += x - px;
            Vertex1.Y += y - py;
            Vertex2.Y += y - py;
            Vertex3.Y += y - py;
            EQ_temp_incircle_center = new Point(EQ_temp_incircle_center.X + x - px, EQ_temp_incircle_center.Y + y - py);
            Vertex1.DispatchOnMovedEvents();
            Vertex2.DispatchOnMovedEvents();
            Vertex3.DispatchOnMovedEvents();
            Segment12.Reposition();
            Segment13.Reposition();
            Segment23.Reposition();
            this.SetPosition(0, 0);
        });
        OnDragged.Add(MainWindow.RegenAll);

        MainWindow.Instance.MainBoard.Children.Add(this);

        MainWindow.RegenAll(0, 0, 0, 0);
        Provider.Regenerate();
    }
    public Circle GenerateCircumCircle()
    {
        Circumcircle = Tools.CircleFrom3Joints(Vertex1, Vertex2, Vertex3);
        Circumcircle.Center.Roles.AddToRole(Role.TRIANGLE_CircumCircleCenter, this);
        Provider.Regenerate();
        return Circumcircle;
    }

    public Circle GenerateInCircle()
    {
        var stats = GetCircleStats();

        var circle = new Circle(new Vertex(ParentBoard, stats.x, stats.y), stats.r);
        circle.Center.Draggable = false;
        circle.Draggable = false;
        Incircle = circle;

        circle.Center.Roles.AddToRole(Role.TRIANGLE_InCircleCenter, this);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3 })
        {
            j.OnMoved.Add(__RecalculateInCircle);
        }

        Provider.Regenerate();

        return circle;
    }

    Stats GetCircleStats()
    {
        // Calculate the lengths of the Triangle sides
        double a = Vertex2.DistanceTo(Vertex3);
        double b = Vertex1.DistanceTo(Vertex3);
        double c = Vertex1.DistanceTo(Vertex2);

        // Calculate the semiperimeter of the Triangle
        double s = (a + b + c) / 2;

        // Calculate the Radius of the inscribed circle
        double radius = Math.Sqrt((s - a) * (s - b) * (s - c) / s);

        // Calculate the coordinates of the Center of the inscribed circle
        double centerX = (a * Vertex1.X + b * Vertex2.X + c * Vertex3.X) / (a + b + c);
        double centerY = (a * Vertex1.Y + b * Vertex2.Y + c * Vertex3.Y) / (a + b + c);

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

        if (Type != TriangleType.EQUILATERAL && Math.Abs(60 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) < Settings.MakeEquilateralAngleOffset &&
            Math.Abs(60 - Tools.GetDegreesBetween3Points(Vertex2, Vertex1, Vertex3)) < Settings.MakeEquilateralAngleOffset &&
            Math.Abs(60 - Tools.GetDegreesBetween3Points(Vertex1, Vertex3, Vertex2)) < Settings.MakeEquilateralAngleOffset) l.Add((TriangleType.EQUILATERAL, "", 1 /* Chosen so it would often show at the top of the list */));
        else if (Type != TriangleType.EQUILATERAL)  // Don't suggest both isosceles & equilateral. doesn't make sense more often than not.
        {
            if (Type != TriangleType.ISOSCELES && Type != TriangleType.ISOSCELES_RIGHT && Segment12.Length.IsSimilarTo(Segment13.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{Segment12} = {Segment13}", Segment12.Length.GetSimilarityPercentage(Segment13.Length)));
            if (Type != TriangleType.ISOSCELES && Type != TriangleType.ISOSCELES_RIGHT && Segment12.Length.IsSimilarTo(Segment23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{Segment12} = {Segment23}", Segment12.Length.GetSimilarityPercentage(Segment23.Length)));
            if (Type != TriangleType.ISOSCELES && Type != TriangleType.ISOSCELES_RIGHT && Segment23.Length.IsSimilarTo(Segment13.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES, $"{Segment23} = {Segment13}", Segment23.Length.GetSimilarityPercentage(Segment13.Length)));
        }

        if (Type != TriangleType.RIGHT && Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{Vertex1}{Vertex2}{Vertex3} = 90°", 1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100));
        if (Type != TriangleType.RIGHT && Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex2, Vertex1, Vertex3)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{Vertex2}{Vertex1}{Vertex3} = 90°", 1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100));
        if (Type != TriangleType.RIGHT && Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex3, Vertex2)) < Settings.MakeRightAngleOffset) l.Add((TriangleType.RIGHT, $"∠{Vertex1}{Vertex3}{Vertex2} = 90°", 1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100));

        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) < Settings.MakeRightAngleOffset && Segment12.Length.IsSimilarTo(Segment23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{Vertex1}{Vertex2}{Vertex3} = 90°, {Segment12} = {Segment23}", ((1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100) + Segment12.Length.GetSimilarityPercentage(Segment23.Length)) / 2 + 0.02));
        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex2, Vertex1, Vertex3)) < Settings.MakeRightAngleOffset && Segment13.Length.IsSimilarTo(Segment23.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{Vertex2}{Vertex1}{Vertex3} = 90°, {Segment13} = {Segment23}", ((1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100) + Segment13.Length.GetSimilarityPercentage(Segment23.Length)) / 2 + 0.02));
        if (Type != TriangleType.ISOSCELES_RIGHT && Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex3, Vertex2)) < Settings.MakeRightAngleOffset && Segment13.Length.IsSimilarTo(Segment12.Length, Settings.MakeIsocelesSideRatioDiff)) l.Add((TriangleType.ISOSCELES_RIGHT, $"∠{Vertex1}{Vertex3}{Vertex2} = 90°, {Segment13} = {Segment12}", ((1 - Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3)) / 100) + Segment13.Length.GetSimilarityPercentage(Segment12.Length)) / 2 + 0.02));


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
            StartPoint = Vertex1,
            IsClosed = true,
            IsFilled = true
        };

        figure?.Segments?.Add(new LineSegment { Point = Vertex2 });
        figure?.Segments?.Add(new LineSegment { Point = Vertex3 });

        geom.Figures.Add(figure);

        if (MainWindow.Instance.MainBoard.HoveredObject == this && (MainWindow.Instance.MainBoard.FocusedObject == this || MainWindow.Instance.MainBoard.FocusedObject is not IShape))
        {
            context.DrawGeometry(UIColors.ShapeHoverFill, null, geom);
        }
        else
        {
            context.DrawGeometry(UIColors.ShapeFill, null, geom);
        }
    }

    public bool IsDefinedBy(Vertex j1, Vertex j2, Vertex j3)
    {
        var arr = new Vertex[] { j1, j2, j3 };
        return arr.Contains(Vertex1) && arr.Contains(Vertex2) && arr.Contains(Vertex3);
    }
    public static bool Exists(Vertex j1, Vertex j2, Vertex j3)
    {
        foreach (var triangle in All)
        {
            if (triangle.IsDefinedBy(j1, j2, j3)) return true;
        }
        return false;
    }
    public static bool Exists(char cid, char id1, char id2)
    {
        var c = Vertex.GetJointById(cid);
        if (c == null) return false;
        var j1 = Vertex.GetJointById(id1);
        if (j1 == null) return false;
        var j2 = Vertex.GetJointById(id2);
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