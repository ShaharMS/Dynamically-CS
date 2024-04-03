using Avalonia;
using Dynamically.Backend;
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

namespace Dynamically.Shapes;

public partial class Quadrilateral : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IContextMenuSupporter<QuadrilateralContextMenuProvider>, ISelectable, IMovementFreezable
{

    public QuadrilateralContextMenuProvider Provider { get; }

    public void Dismantle()
    {
        if (!Vertex1.GotRemoved && !Vertex2.GotRemoved && !Vertex3.GotRemoved && !Vertex4.GotRemoved)
        { // Dismantle was forceful, it is expected to completely disconnect the quadrilateral from its vertices
            Con1.Dismantle();
            Con2.Dismantle();
            Con3.Dismantle();
            Con4.Dismantle();
        }

        All.Remove(this);
        MainWindow.RegenAll(0, 0, 0, 0);
        MainWindow.Instance.MainBoard.Children.Remove(this);
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

    public override bool Overlaps(Point p)
    {
        var rayCast = new RayFormula(p, 0);

        int intersections = 0;
        foreach (var f in new[] { Con1, Con2, Con3, Con4 })
        {
            var i = f.Formula.Intersect(rayCast);
            if (i != null && i?.X >= p.X) intersections++;
        }
        foreach (var j in new[] { Vertex1, Vertex2, Vertex3, Vertex4 })
        {
            if (p.RoughlyEquals(j))
            {
                intersections--;
                break;
            }
        }
        return intersections % 2 == 1;
    }

    public override double Area()
    {
        if (Con1.SharesJointWith(Con2))
        {
            return
                Con1.Length * Con2.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(Con1, Con2))) / 2 +
                Con3.Length * Con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(Con3, Con4))) / 2;
        }
        else if (Con1.SharesJointWith(Con3))
        {
            return
                Con1.Length * Con3.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(Con1, Con3))) / 2 +
                Con2.Length * Con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(Con2, Con4))) / 2;
        }

        return double.NaN;
    }
    public bool Contains(Vertex joint)
    {
        return joint == Vertex1 || joint == Vertex2 || joint == Vertex3 || joint == Vertex4;
    }

    public bool Contains(Segment segment)
    {
        return segment == Con1 || segment == Con2 || segment == Con3 || segment == Con4;
    }

    public bool HasMounted(Vertex joint)
    {
        return false;
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public override string ToString()
    {
        return $"{Vertex1.Id}{Vertex2.Id}{Vertex3.Id}{Vertex4.Id}";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return $"{TypeToString(Type)} " + ToString();
    }

    private string TypeToString(QuadrilateralType type) => type != QuadrilateralType.IRREGULAR ? new CultureInfo("en-US", false).TextInfo.ToTitleCase(type.ToString().ToLower().Replace('_', ' ')) : "Quadrilateral";

    public bool EncapsulatedWithin(Rect rect)
    {
        return Vertex1.EncapsulatedWithin(rect) && Vertex2.EncapsulatedWithin(rect) && Vertex3.EncapsulatedWithin(rect) && Vertex4.EncapsulatedWithin(rect);
    }

    public bool IsMovable()
    {
        return Vertex1.IsMovable() && Vertex2.IsMovable() && Vertex3.IsMovable() && Vertex4.IsMovable();
    }
}
