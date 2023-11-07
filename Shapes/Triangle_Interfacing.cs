using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Triangle : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IContextMenuSupporter<TriangleContextMenuProvider>, ISelectable
{
    public TriangleContextMenuProvider Provider { get; }

    public void Dismantle()
    {
        Type = TriangleType.SCALENE; // Remove position modifiers
        if (joint1.GotRemoved) joint1.Disconnect(joint2, joint3);
        if (joint2.GotRemoved) joint2.Disconnect(joint1, joint3);
        if (joint3.GotRemoved) joint3.Disconnect(joint2, joint1);

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.OnMoved.Remove(__RecalculateInCircle);
        }

        if (incircle != null)
        {
            incircle.Draggable = true;
            incircle.center.Draggable = true;
            incircle.center.Roles.RemoveFromRole(Role.TRIANGLE_InCircleCenter, this);
        }

        if (circumcircle != null) circumcircle.center.Roles.RemoveFromRole(Role.TRIANGLE_CircumCircleCenter, this);

        foreach (var j in new[] { joint1, joint2, joint3 })
        {
            j.Roles.RemoveFromRole(Role.TRIANGLE_Corner, this);
        }

        Triangle.all.Remove(this);
        MainWindow.BigScreen.Children.Remove(this);
    }


    public void __RecalculateInCircle(double ux, double uy, double px, double py)
    {
        var stats = GetCircleStats();
        if (incircle == null) return;
        incircle.center.X = stats.x;
        incircle.center.Y = stats.y;
        incircle.radius = stats.r;
        incircle.UpdateFormula();
        incircle.InvalidateVisual();
        foreach (var listener in incircle.center.OnMoved) listener(incircle.center.X, incircle.center.Y, px, py);
    }

    public void __Disment(Vertex z, Vertex x)
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


    public override string ToString()
    {
        return $"△{joint1.Id}{joint2.Id}{joint3.Id}";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return $"{typeToString(Type)} " + ToString();
    }

    private string typeToString(TriangleType type) => type != TriangleType.SCALENE ? new CultureInfo("en-US", false).TextInfo.ToTitleCase(type.ToString().ToLower().Replace('_', ' ')) : "Triangle";

    public override double Area()
    {
        return con12.Length * con23.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetween3Points(joint1, joint2, joint3))) / 2;
    }


    public override bool Overlaps(Point p)
    {
        double areaABC = 0.5 * Math.Abs(joint1.X * (joint2.Y - joint3.Y) +
                                       joint2.X * (joint3.Y - joint1.Y) +
                                       joint3.X * (joint1.Y - joint2.Y));

        double areaPBC = 0.5 * Math.Abs(p.X * (joint2.Y - joint3.Y) +
                                      joint2.X * (joint3.Y - p.Y) +
                                      joint3.X * (p.Y - joint2.Y));

        double areaPCA = 0.5 * Math.Abs(joint1.X * (p.Y - joint3.Y) +
                                      p.X * (joint3.Y - joint1.Y) +
                                      joint3.X * (joint1.Y - p.Y));

        double areaPAB = 0.5 * Math.Abs(joint1.X * (joint2.Y - p.Y) +
                                      joint2.X * (p.Y - joint1.Y) +
                                      p.X * (joint1.Y - joint2.Y));

        // If the sum of the sub-Triangle areas is equal to the Triangle area, the point is inside the Triangle
        return Math.Abs(areaPBC + areaPCA + areaPAB - areaABC) < 0.0001; // Adjust epsilon as needed for floating-point comparison
    }


    public bool Contains(Vertex joint)
    {
        return joint == joint1 || joint == joint2 || joint == joint3;
    }

    public bool Contains(Segment segment)
    {
        return segment == con12 || segment == con13 || segment == con23;
    }

    public bool HasMounted(Vertex joint)
    {
        return false;
    }

    public bool HasMounted(Segment segment)
    {
        return segment.Roles.Has((Role.TRIANGLE_AngleBisector, Role.TRIANGLE_Perpendicular), this);
    }

    public bool EncapsulatedWithin(Rect rect)
    {
        return joint1.EncapsulatedWithin(rect) && joint2.EncapsulatedWithin(rect) && joint3.EncapsulatedWithin(rect);
    }
}
