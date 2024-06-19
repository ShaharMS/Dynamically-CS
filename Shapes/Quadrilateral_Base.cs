
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Quadrilateral : DraggableGraphic
{
    public static readonly List<Quadrilateral> All = new();

    public Vertex Vertex1;
    public Vertex Vertex2;
    public Vertex Vertex3;
    public Vertex Vertex4;

    Func<double> _degrees1;
    Func<double> _degrees2;
    Func<double> _degrees3;
    Func<double> _degrees4;
    public double Degrees1 { get => _degrees1(); }
    public double Degrees2 { get => _degrees2(); }
    public double Degrees3 { get => _degrees3(); }
    public double Degrees4 { get => _degrees4(); }

    public double Radians1 { get => _degrees1().ToRadians(); }
    public double Radians2 { get => _degrees2().ToRadians(); }
    public double Radians3 { get => _degrees3().ToRadians(); }
    public double Radians4 { get => _degrees4().ToRadians(); }

    public Vertex[] Angle1Joints;
    public Vertex[] Angle2Joints;
    public Vertex[] Angle3Joints;
    public Vertex[] Angle4Joints;


    public Segment Con1;
    public Segment Con2;
    public Segment Con3;
    public Segment Con4;

    public Tuple<Segment, Segment>[] Opposites;

    public Tuple<Segment, Segment>[] Adjacents;

    QuadrilateralType _type = QuadrilateralType.IRREGULAR;
    public QuadrilateralType Type
    {
        get => _type;
        set => ChangeType(value);
    }

    
    public Circle? Circumcircle;
    public Circle? Incircle;

#pragma warning disable CS8618
    public Quadrilateral(Vertex j1, Vertex j2, Vertex j3, Vertex j4) : base(j1.ParentBoard)
    {

        Vertex1 = j1;
        Vertex2 = j2;
        Vertex3 = j3;
        Vertex4 = j4;

        var sides = Quadrilateral.GetValidQuadrilateralSides(j1, j2, j3, j4);
        Log.WriteVar(sides);
        if (sides.Count == 0) return; // Don't do anything

        All.Add(this);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3, Vertex4 }) j.Roles.AddToRole(Role.QUAD_Corner, this);

        for (int i = 0; i < 4; i++) {
            var side = sides[i];
            var x = side.Item1.Connect(side.Item2);
            x.Roles.AddToRole(Role.QUAD_Side, this);
            if (i == 0) Con1 = x;
            else if (i == 1) Con2 = x;
            else if (i == 2) Con3 = x;
            else if (i == 3) Con4 = x;
        }

        Quadrilateral.AssignSegmentData(this);
        Quadrilateral.AssignAngles(this);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3, Vertex4 }) j.Reposition();

        ContextMenu = new ContextMenu();
        Provider = new QuadrilateralContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        OnDragStart.Add(() => { if (!IsMovable()) CurrentlyDragging = false; });
        OnMoved.Add((x, y, px, py) =>
        {
            if (Vertex1.Anchored || Vertex2.Anchored || Vertex3.Anchored || Vertex4.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            Vertex1.X += x - px;
            Vertex2.X += x - px;
            Vertex3.X += x - px;
            Vertex4.X += x - px;
            Vertex1.Y += y - py;
            Vertex2.Y += y - py;
            Vertex3.Y += y - py;
            Vertex4.Y += y - py;
            Vertex1.DispatchOnMovedEvents();
            Vertex2.DispatchOnMovedEvents();
            Vertex3.DispatchOnMovedEvents();
            Vertex4.DispatchOnMovedEvents();
            Con1?.Reposition();
            Con2?.Reposition();
            Con3?.Reposition();
            Con4?.Reposition();
            this.SetPosition(0, 0);
        });
        OnDragged.Add(MainWindow.RegenAll);

        MainWindow.RegenAll(0,0,0,0);
        MainWindow.Instance.MainBoard.Children.Add(this);
    }
#pragma warning restore CS8618
    

    public Point GetCentroid()
    {
        return new((Vertex1.X + Vertex2.X + Vertex3.X + Vertex4.X) / 4, (Vertex1.Y + Vertex2.Y + Vertex3.Y + Vertex4.Y) / 4);
    }

    public bool HasAsSide(Vertex v1, Vertex v2)
    {
        return Con1.IsMadeOf(v1, v2) || Con2.IsMadeOf(v1, v2) || Con3.IsMadeOf(v1, v2) || Con4.IsMadeOf(v1, v2);
    }

    public override void Render(DrawingContext context)
    {
        var geom = new PathGeometry();
        var figure1 = new PathFigure
        {
            StartPoint = Opposites[0].Item1.Vertex1,
            IsClosed = true,
            IsFilled = true
        };
        figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item1.Vertex1 });
        figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item1.Vertex2 });
        if (HasAsSide(Opposites[0].Item1.Vertex2, Opposites[0].Item2.Vertex1))
        {
            figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item2.Vertex1 });
            figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item2.Vertex2 });
        } else
        {
            figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item2.Vertex2 });
            figure1.Segments?.Add(new LineSegment { Point = Opposites[0].Item2.Vertex1 });
        }
            

        geom.Figures.Add(figure1);

        if (MainWindow.Instance.MainBoard.HoveredObject == this && (MainWindow.Instance.MainBoard.FocusedObject == this || MainWindow.Instance.MainBoard.FocusedObject is not IShape))
        {
            context.DrawGeometry(UIColors.ShapeHoverFill, null, geom);
        }
        else
        {
            context.DrawGeometry(UIColors.ShapeFill, null, geom);
        }
    }

    public bool IsDefinedBy(Vertex j1, Vertex j2, Vertex j3, Vertex j4)
    {
        var arr = new Vertex[] { j1, j2, j3, j4 };
        return arr.Contains(Vertex1) && arr.Contains(Vertex2) && arr.Contains(Vertex3) && arr.Contains(Vertex4);
    }
    }

public enum QuadrilateralType
{
    SQUARE,
    RECTANGLE,
    PARALLELOGRAM,
    RHOMBUS,
    TRAPEZOID,
    ISOSCELES_TRAPEZOID,
    RIGHT_TRAPEZOID,
    KITE,
    IRREGULAR
}