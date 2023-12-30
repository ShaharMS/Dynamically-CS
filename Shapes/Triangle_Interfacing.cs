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
        if (Vertex1.GotRemoved) Vertex1.Disconnect(Vertex2, Vertex3);
        if (Vertex2.GotRemoved) Vertex2.Disconnect(Vertex1, Vertex3);
        if (Vertex3.GotRemoved) Vertex3.Disconnect(Vertex2, Vertex1);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3 })
        {
            j.OnMoved.Remove(__RecalculateInCircle);
        }

        if (Incircle != null)
        {
            Incircle.Draggable = true;
            Incircle.Center.Draggable = true;
            Incircle.Center.Roles.RemoveFromRole(Role.TRIANGLE_InCircleCenter, this);
        }

        Circumcircle?.Center.Roles.RemoveFromRole(Role.TRIANGLE_CircumCircleCenter, this);

        foreach (var j in new[] { Vertex1, Vertex2, Vertex3 })
        {
            j.Roles.RemoveFromRole(Role.TRIANGLE_Corner, this);
        }

        Triangle.All.Remove(this);
        MainWindow.BigScreen.Children.Remove(this);
    }


    public void __RecalculateInCircle(double ux, double uy, double px, double py)
    {
        var stats = GetCircleStats();
        if (Incircle == null) return;
        Incircle.Center.X = stats.x; // No need to validate movement, this value "doesn't matter"
        Incircle.Center.Y = stats.y;
        Incircle.Radius = stats.r;
        Incircle.UpdateFormula();
        Incircle.InvalidateVisual();
        foreach (var listener in Incircle.Center.OnMoved) listener(Incircle.Center.X, Incircle.Center.Y, px, py);
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
        return $"△{Vertex1.Id}{Vertex2.Id}{Vertex3.Id}";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return $"{TypeToString(Type)} " + ToString();
    }

    private string TypeToString(TriangleType type) => type != TriangleType.SCALENE ? new CultureInfo("en-US", false).TextInfo.ToTitleCase(type.ToString().ToLower().Replace('_', ' ')) : "Triangle";

    public override double Area()
    {
        return Segment12.Length * Segment23.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetween3Points(Vertex1, Vertex2, Vertex3))) / 2;
    }


    public override bool Overlaps(Point p)
    {
        double areaABC = 0.5 * Math.Abs(Vertex1.X * (Vertex2.Y - Vertex3.Y) +
                                       Vertex2.X * (Vertex3.Y - Vertex1.Y) +
                                       Vertex3.X * (Vertex1.Y - Vertex2.Y));

        double areaPBC = 0.5 * Math.Abs(p.X * (Vertex2.Y - Vertex3.Y) +
                                      Vertex2.X * (Vertex3.Y - p.Y) +
                                      Vertex3.X * (p.Y - Vertex2.Y));

        double areaPCA = 0.5 * Math.Abs(Vertex1.X * (p.Y - Vertex3.Y) +
                                      p.X * (Vertex3.Y - Vertex1.Y) +
                                      Vertex3.X * (Vertex1.Y - p.Y));

        double areaPAB = 0.5 * Math.Abs(Vertex1.X * (Vertex2.Y - p.Y) +
                                      Vertex2.X * (p.Y - Vertex1.Y) +
                                      p.X * (Vertex1.Y - Vertex2.Y));

        // If the sum of the sub-Triangle areas is equal to the Triangle area, the point is inside the Triangle
        return Math.Abs(areaPBC + areaPCA + areaPAB - areaABC) < 0.0001; // Adjust epsilon as needed for floating-point comparison
    }


    public bool Contains(Vertex joint)
    {
        return joint == Vertex1 || joint == Vertex2 || joint == Vertex3;
    }

    public bool Contains(Segment segment)
    {
        return segment == Segment12 || segment == Segment13 || segment == Segment23;
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
        return Vertex1.EncapsulatedWithin(rect) && Vertex2.EncapsulatedWithin(rect) && Vertex3.EncapsulatedWithin(rect);
    }
}
