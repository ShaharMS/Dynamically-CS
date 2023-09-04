
using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using System;
using System.Collections.Generic;
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

    public Segment con12;
    public Segment con23;
    public Segment con34;
    public Segment con41;

    QuadrilateralType _type = QuadrilateralType.IRREGULAR;

    public QuadrilateralType type
    {
        get => _type;
        set => ChangeType(value);
    }

    public Quadrilateral(Joint j1, Joint j2, Joint j3, Joint j4)
    {
        joint1 = j1;
        joint2 = j2;
        joint3 = j3;
        joint4 = j4;

        con12 = joint1.Connect(joint2);
        con23 = joint2.Connect(joint3);
        con34 = joint3.Connect(joint4);
        con41 = joint4.Connect(joint1);
    }

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

    public override bool Overlaps(Point p)
    {
        bool isInside = false;

        var CrossProduct = (Point p1, Point p2, Point p3) => (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);

        // Calculate vectors from the test point to the vertices
        double d1 = CrossProduct(p, joint1, joint2);
        double d2 = CrossProduct(p, joint2, joint3);
        double d3 = CrossProduct(p, joint3, joint4);
        double d4 = CrossProduct(p, joint4, joint1);

        // Check if the signs of the cross products are all the same
        return (d1 > 0 && d2 > 0 && d3 > 0 && d4 > 0) || (d1 < 0 && d2 < 0 && d3 < 0 && d4 < 0);
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
        return segment == con12 || segment == con23 || segment == con34 || segment == con41;
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
        return "Triangle " + ToString();
    }

    private string typeToString(QuadrilateralType type) => type.ToString().ToLower()
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