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

namespace Dynamically.Shapes;

public partial class Quadrilateral : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IContextMenuSupporter<QuadrilateralContextMenuProvider>, ISelectable
{

    public QuadrilateralContextMenuProvider Provider { get; }

    public void Dismantle()
    {
        con1.Dismantle();
        con2.Dismantle();
        con3.Dismantle();
        con4.Dismantle();

        all.Remove(this);
        MainWindow.regenAll(0, 0, 0, 0);
        MainWindow.BigScreen.Children.Remove(this);
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
        foreach (var f in new[] { con1, con2, con3, con4 })
        {
            var i = f.Formula.Intersect(rayCast);
            if (i != null && i?.X >= p.X) intersections++;
        }
        foreach (var j in new[] { joint1, joint2, joint3, joint4 })
        {
            if (p.Equals(j))
            {
                intersections--;
                break;
            }
        }
        return intersections % 2 == 1;
    }

    public override double Area()
    {
        if (con1.SharesJointWith(con2))
        {
            return
                con1.Length * con2.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con1, con2))) / 2 +
                con3.Length * con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con3, con4))) / 2;
        }
        else if (con1.SharesJointWith(con3))
        {
            return
                con1.Length * con3.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con1, con3))) / 2 +
                con2.Length * con4.Length * Math.Abs(Math.Sin(Tools.GetRadiansBetweenConnections(con2, con4))) / 2;
        }

        return double.NaN;
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

    public bool EncapsulatedWithin(Rect rect)
    {
        return joint1.EncapsulatedWithin(rect) && joint2.EncapsulatedWithin(rect) && joint3.EncapsulatedWithin(rect) && joint4.EncapsulatedWithin(rect);
    }
}
