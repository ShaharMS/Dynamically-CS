
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

    public Segment con12;
    public Segment con23;
    public Segment con34;
    public Segment con41;

    QuadrilateralType _type = QuadrilateralType.IRREGULAR;

    public QuadrilateralType Type
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

        //con12 = joint1.Connect(joint2);
        con23 = joint2.Connect(joint3);
        con34 = joint3.Connect(joint4);
        con41 = joint4.Connect(joint1);

        foreach (var j in new[] { joint1, joint2, joint3, joint4 }) j.Roles.AddToRole(Role.QUAD_Corner, this);
        foreach (var con in new[] { con12, con23, con34, con41 }) con?.Roles.AddToRole(Role.QUAD_Side, this);

        foreach (var j in new[] { joint1, joint2, joint3, joint4 }) j.reposition();
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
        bool checkWithTriangle(Joint joint1, Joint joint2, Joint joint3)
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

            // If the sum of the sub-Triangle areas is equal to the Triangle area, the point is inside the Triangle
            return Math.Abs(areaPBC + areaPCA + areaPAB - areaABC) < 0.0001; // Adjust epsilon as needed for floating-point comparison
        }

        return checkWithTriangle(joint1, joint2, joint3) || checkWithTriangle(joint1, joint4, joint3) || checkWithTriangle(joint2, joint1, joint4) || checkWithTriangle(joint2, joint3, joint4); 
        // Two extra check because we don't know which triangles overlap
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