
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

public partial class Quadrilateral : DraggableGraphic, IDismantable, IShape, IStringifyable
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

    QuadrilateralType _type = QuadrilateralType.IRREGULAR;
    public QuadrilateralType Type
    {
        get => _type;
        set => ChangeType(value);
    }

    
    public Circle? circumcircle;
    public Circle? incircle;

    public QuadrilateralContextMenuProvider Provider;

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

        all.Add(this);
        MainWindow.regenAll(0,0,0,0);
        MainWindow.BigScreen.Children.Add(this);
    }
#pragma warning restore CS8618

    public void Dismantle()
    {
        joint1.Disconnect(joint2, joint4);
        joint3.Disconnect(joint2, joint4);
    }


    QuadrilateralType ChangeType(QuadrilateralType type)
    {
        // Actual shape modification
        switch (type)
        {
            case QuadrilateralType.SQUARE:
                var angle1ClosenessTo90Deg = Math.Abs(90 - degrees1);
                var angle2ClosenessTo90Deg = Math.Abs(90 - degrees2);
                var angle3ClosenessTo90Deg = Math.Abs(90 - degrees3);
                var angle4ClosenessTo90Deg = Math.Abs(90 - degrees4);
                var closest = Math.Min(Math.Min(angle1ClosenessTo90Deg, angle2ClosenessTo90Deg), Math.Min(angle3ClosenessTo90Deg, angle4ClosenessTo90Deg));
                if (closest == angle1ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                else if (closest == angle2ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle2Joints[0], angle2Joints[1], angle2Joints[2]);
                else if (closest == angle3ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle3Joints[0], angle3Joints[1], angle3Joints[2]);
                else if (closest == angle4ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle4Joints[0], angle4Joints[1], angle4Joints[2]);
                break;
            case QuadrilateralType.RECTANGLE: 
                var _angle1ClosenessTo90Deg = Math.Abs(90 - degrees1);
                var _angle2ClosenessTo90Deg = Math.Abs(90 - degrees2);
                var _angle3ClosenessTo90Deg = Math.Abs(90 - degrees3);
                var _angle4ClosenessTo90Deg = Math.Abs(90 - degrees4);
                var _closest = Math.Min(Math.Min(_angle1ClosenessTo90Deg, _angle2ClosenessTo90Deg), Math.Min(_angle3ClosenessTo90Deg, _angle4ClosenessTo90Deg));
                if (_closest == _angle1ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                else if (_closest == _angle2ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle2Joints[0], angle2Joints[1], angle2Joints[2]);
                else if (_closest == _angle3ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle3Joints[0], angle3Joints[1], angle3Joints[2]);
                else if (_closest == _angle4ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle4Joints[0], angle4Joints[1], angle4Joints[2]);
                break;
            case QuadrilateralType.RHOMBUS:
                ForceType(QuadrilateralType.RHOMBUS, joint1, joint2, joint3);
                break;
            case QuadrilateralType.PARALLELOGRAM:
                if (!con1.SharesJointWith(con3)) {
                    var pair1AngleDiff = Math.Abs(con1.)
                } 
                else if (!con1.SharesJointWith(con2)) {

                }
                else { // 1 opposes 4

                }
                break;
            // case TriangleType.EQUILATERAL:
            //     var a_ABBC_SimilarityOfSides = Math.Abs(con12.Length - con23.Length);
            //     var a_ACCB_ClosenessTo60Deg = Math.Abs(con13.Length - con23.Length);
            //     var a_BAAC_ClosenessTo60Deg = Math.Abs(con13.Length - con12.Length);
            //     if (a_ABBC_SimilarityOfSides < a_ACCB_ClosenessTo60Deg && a_ABBC_SimilarityOfSides < a_BAAC_ClosenessTo60Deg) ForceType(TriangleType.EQUILATERAL, joint1, joint2, joint3);
            //     else if (a_ACCB_ClosenessTo60Deg < a_ABBC_SimilarityOfSides && a_ACCB_ClosenessTo60Deg < a_BAAC_ClosenessTo60Deg) ForceType(TriangleType.EQUILATERAL, joint1, joint3, joint2);
            //     else ForceType(TriangleType.EQUILATERAL, joint2, joint1, joint3);
            //     break;
            // case TriangleType.ISOSCELES:
            //     var con12_to_con13_Diff = Math.Abs(con12.Length - con13.Length);
            //     var con12_to_con23_Diff = Math.Abs(con12.Length - con23.Length);
            //     var con13_to_con23_Diff = Math.Abs(con13.Length - con23.Length);
            //     if (con12_to_con23_Diff < con13_to_con23_Diff && con12_to_con23_Diff < con12_to_con13_Diff) ForceType(TriangleType.ISOSCELES, joint1, joint2, joint3);
            //     else if (con12_to_con13_Diff < con13_to_con23_Diff && con12_to_con13_Diff < con12_to_con23_Diff) ForceType(TriangleType.ISOSCELES, joint2, joint1, joint3);
            //     else ForceType(TriangleType.ISOSCELES, joint1, joint3, joint2);
            //     break;
            // case TriangleType.RIGHT:
            //     var a_ABC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3));
            //     var a_ACB_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2));
            //     var a_BAC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3));
            //     if (a_ABC_ClosenessTo90Deg < a_ACB_ClosenessTo90Deg && a_ABC_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) ForceType(TriangleType.RIGHT, joint1, joint2, joint3);
            //     else if (a_ACB_ClosenessTo90Deg < a_ABC_ClosenessTo90Deg && a_ACB_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) ForceType(TriangleType.RIGHT, joint1, joint3, joint2);
            //     else ForceType(TriangleType.RIGHT, joint2, joint1, joint3);
            //     break;
            // case TriangleType.ISOSCELES_RIGHT:
            //     var a_ABC_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint2, joint3));
            //     var a_ACB_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint1, joint3, joint2));
            //     var a_BAC_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(joint2, joint1, joint3));
            //     if (a_ABC_ClosenessTo90Deg1 < a_ACB_ClosenessTo90Deg1 && a_ABC_ClosenessTo90Deg1 < a_BAC_ClosenessTo90Deg1)
            //     {
            //         ForceType(TriangleType.RIGHT, joint1, joint2, joint3);
            //         ForceType(TriangleType.ISOSCELES, joint1, joint2, joint3);
            //     }
            //     else if (a_ACB_ClosenessTo90Deg1 < a_ABC_ClosenessTo90Deg1 && a_ACB_ClosenessTo90Deg1 < a_BAC_ClosenessTo90Deg1)
            //     {
            //         ForceType(TriangleType.RIGHT, joint1, joint3, joint2);
            //         ForceType(TriangleType.ISOSCELES, joint1, joint3, joint2);
            //     }
            //     else
            //     {
            //         ForceType(TriangleType.RIGHT, joint2, joint1, joint3);
            //         ForceType(TriangleType.ISOSCELES, joint2, joint1, joint3);
            //     }
            //     break;
            // case TriangleType.SCALENE:
            //     break;
        }
        joint1.reposition(); joint2.reposition(); joint3.reposition(); joint4.reposition();
        _type = type;
        Provider.Regenerate();
        return type;
    }

    public void __Disment(Joint z, Joint x)
    {
        _ = z; _ = x;
        Dismantle();
    }
    public void __Disment(double z, double x)
    {
        _ = z; _ = x;
        Dismantle();
    }

    public void __Regen(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v;
        Provider.Regenerate();
    }

    public override bool Overlaps(Point p)
    {
        var rayCast = new RayFormula(p, 0);

        int intersections = 0;
        foreach (var f in new[] {con1, con2, con3, con4})
        {
            var i = f.Formula.Intersect(rayCast);
            if (i != null && i?.X >= p.X) intersections++;
        }
        foreach (var j in new[] {joint1, joint2, joint3, joint4}) {
            if (p.Equals(j)) {
                intersections--;
                break;
            } 
        }
        return intersections % 2 == 1;
    }

    public override double Area()
    {
        if (con1.SharesJointWith(con2)) {
            return 
                con1.Length * con2.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con1, con2))) / 2 +
                con3.Length * con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con3, con4))) / 2;
        } else if (con1.SharesJointWith(con3)) {
            return 
                con1.Length * con3.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con1, con3))) / 2 +
                con2.Length * con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con2, con4))) / 2;
        }

        return double.NaN;
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
    public bool Contains(Joint joint)
    {
        return joint == joint1 || joint == joint2 || joint == joint3 || joint == joint4;
    }

    public bool Contains(Segment segment)
    {
        return segment == con1 || segment == con2 || segment == con3 || segment == con4;
    }

    public bool HasMounted(Joint joint)
    {
        return false;
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public override string ToString()
    {
        return $"{joint1.Id}{joint2.Id}{joint3.Id}{joint4.Id}";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return $"{typeToString(Type)} " + ToString();
    }

    private string typeToString(QuadrilateralType type) => type != QuadrilateralType.IRREGULAR ? new CultureInfo("en-US", false).TextInfo.ToTitleCase(type.ToString().ToLower().Replace('_', ' ')) : "Quadrilateral";
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