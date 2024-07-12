using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Geometry.Basics;

namespace Dynamically.Geometry;

public partial class Triangle : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IContextMenuSupporter<TriangleContextMenuProvider>, ISelectable, IMovementFreezable, IHasFormula<TriangleIncircleFormula>
{
    public TriangleContextMenuProvider Provider { get; }
    public TriangleIncircleFormula Formula { get; set; }

    public void Dismantle()
    {
        Type = TriangleType.SCALENE; // Remove position modifiers
        if (Vertex1.GotRemoved) Vertex1.Disconnect(Vertex2, Vertex3);
        if (Vertex2.GotRemoved) Vertex2.Disconnect(Vertex1, Vertex3);
        if (Vertex3.GotRemoved) Vertex3.Disconnect(Vertex2, Vertex1);

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
        ParentBoard.RemoveChild(this);
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

    public void __triangleMoveThroughIncircleCenter()
    {
        if (Incircle != null)
        {
            if ((ParentBoard.Selection?.EncapsulatedElements.Contains(Incircle!) ?? false) && 
                (Circumcircle != null && 
                    (ParentBoard.Selection?.EncapsulatedElements.Contains(Circumcircle.Center) ?? false))) return;
            Incircle.Center.CurrentlyDragging = false;
        }
        ForceStartDrag(ParentBoard.Mouse, -ParentBoard.MouseX + X, -ParentBoard.MouseY + Y);
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


    public bool Contains(Vertex vertex)
    {
        return vertex == Vertex1 || vertex == Vertex2 || vertex == Vertex3;
    }

    public bool Contains(Segment segment)
    {
        return segment == Segment12 || segment == Segment13 || segment == Segment23;
    }

    public bool HasMounted(Vertex vertex)
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
    public bool IsMovable()
    {
        return Vertex1.IsMovable() && Vertex2.IsMovable() && Vertex3.IsMovable();
    }
}
