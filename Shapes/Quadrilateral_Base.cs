
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
    public static readonly List<Quadrilateral> all = new();

    public Joint joint1;
    public Joint joint2;
    public Joint joint3;
    public Joint joint4;

    Func<double> _degrees1;
    Func<double> _degrees2;
    Func<double> _degrees3;
    Func<double> _degrees4;
    public double degrees1 { get => _degrees1(); }
    public double degrees2 { get => _degrees2(); }
    public double degrees3 { get => _degrees3(); }
    public double degrees4 { get => _degrees4(); }

    public double radians1 { get => _degrees1().ToRadians(); }
    public double radians2 { get => _degrees2().ToRadians(); }
    public double radians3 { get => _degrees3().ToRadians(); }
    public double radians4 { get => _degrees4().ToRadians(); }

    public Joint[] angle1Joints;
    public Joint[] angle2Joints;
    public Joint[] angle3Joints;
    public Joint[] angle4Joints;


    public Segment con1;
    public Segment con2;
    public Segment con3;
    public Segment con4;

    public Tuple<Segment, Segment>[] opposites;

    public Tuple<Segment, Segment>[] adjacents;

    QuadrilateralType _type = QuadrilateralType.IRREGULAR;
    public QuadrilateralType Type
    {
        get => _type;
        set => ChangeType(value);
    }

    
    public Circle? circumcircle;
    public Circle? incircle;

#pragma warning disable CS8618
    public Quadrilateral(Joint j1, Joint j2, Joint j3, Joint j4)
    {
        joint1 = j1;
        joint2 = j2;
        joint3 = j3;
        joint4 = j4;

        
        var sides = Quadrilateral.GetValidQuadrilateralSides(j1, j2, j3, j4);
        if (sides.Count == 0) return; // Don't do anything
        
        foreach (var j in new[] { joint1, joint2, joint3, joint4 }) j.Roles.AddToRole(Role.QUAD_Corner, this);

        for (int i = 0; i < 4; i++) {
            var side = sides[i];
            var x = side.Item1.Connect(side.Item2);
            x.Roles.AddToRole(Role.QUAD_Side, this);
            if (i == 0) con1 = x;
            else if (i == 1) con2 = x;
            else if (i == 2) con3 = x;
            else if (i == 3) con4 = x;
        }

        Quadrilateral.AssignSegmentData(this);
        Quadrilateral.AssignAngles(this);

        foreach (var j in new[] { joint1, joint2, joint3, joint4 }) j.reposition();

        ContextMenu = new ContextMenu();
        Provider = new QuadrilateralContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        OnMoved.Add((x, y, px, py) =>
        {
            if (joint1.Anchored || joint2.Anchored || joint3.Anchored || joint4.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            joint1.X += x - px;
            joint2.X += x - px;
            joint3.X += x - px;
            joint4.X += x - px;
            joint1.Y += y - py;
            joint2.Y += y - py;
            joint3.Y += y - py;
            joint4.Y += y - py;
            joint1.DispatchOnMovedEvents(joint1.X, joint1.Y, joint1.X, joint1.Y);
            joint2.DispatchOnMovedEvents(joint2.X, joint2.Y, joint2.X, joint2.Y);
            joint3.DispatchOnMovedEvents(joint3.X, joint3.Y, joint3.X, joint3.Y);
            joint4.DispatchOnMovedEvents(joint4.X, joint4.Y, joint4.X, joint4.Y);
            con1?.reposition();
            con2?.reposition();
            con3?.reposition();
            con4?.reposition();
            this.SetPosition(0, 0);
        });
        OnDragged.Add(MainWindow.regenAll);

        all.Add(this);
        MainWindow.regenAll(0,0,0,0);
        MainWindow.BigScreen.Children.Add(this);
    }
#pragma warning restore CS8618
    

    public Point GetCentroid()
    {
        return new((joint1.X + joint2.X + joint3.X + joint4.X) / 4, (joint1.Y + joint2.Y + joint3.Y + joint4.Y) / 4);
    }

    public override void Render(DrawingContext context)
    {
        var geom = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = con1.joint1,
            IsClosed = true,
            IsFilled = true
        };

        figure?.Segments?.Add(new LineSegment { Point = con1.joint2 });
        figure?.Segments?.Add(new LineSegment { Point = con2.joint1 });
        figure?.Segments?.Add(new LineSegment { Point = con2.joint2 });
        figure?.Segments?.Add(new LineSegment { Point = con3.joint1 });
        figure?.Segments?.Add(new LineSegment { Point = con3.joint2 });
        figure?.Segments?.Add(new LineSegment { Point = con4.joint1 });
        figure?.Segments?.Add(new LineSegment { Point = con4.joint2 });

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

    public bool IsDefinedBy(Joint j1, Joint j2, Joint j3, Joint j4)
    {
        var arr = new Joint[] { j1, j2, j3, j4 };
        return arr.Contains(joint1) && arr.Contains(joint2) && arr.Contains(joint3) && arr.Contains(joint4);
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