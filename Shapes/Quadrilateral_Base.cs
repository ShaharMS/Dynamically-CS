
using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Quadrilateral : DraggableGraphic, IDismantable, IShape, IStringifyable
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;
    public Joint joint4;

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

        foreach (var j in new[] { joint1, joint2, joint3, joint4 }) j.reposition();
    }
#pragma warning restore CS8618

    public void Dismantle()
    {
        joint1.Disconnect(joint2, joint4);
        joint3.Disconnect(joint2, joint4);
    }


    QuadrilateralType ChangeType(QuadrilateralType type)
    {
        _type = type;
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
        //Provider.Regenerate();
    }

    public override bool Overlaps(Point p)
    {
        // Solution one - make pentagon, if its area is smaller than this return true
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
        figure?.Segments?.Add(new LineSegment { Point = joint4 });

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