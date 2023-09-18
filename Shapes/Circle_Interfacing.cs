using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Circle : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IHasFormula<CircleFormula>, IContextMenuSupporter<CircleContextMenuProvider>
{
    public CircleFormula Formula { get; set; }

    public CircleContextMenuProvider Provider { get; }

    public void Dismantle()
    {
        foreach (var follower in Formula.Followers.ToArray())
        {
            follower.Roles.RemoveFromRole(Role.CIRCLE_On, this);
        }
        Formula.queueRemoval = true;
        onResize.Clear();
        Circle.all.Remove(this);
        EllipseBase.all.Remove(this);

        foreach (var l in OnRemoved) l();

        MainWindow.BigScreen.Children.Remove(ring);
        MainWindow.BigScreen.Children.Remove(this);
    }
    public void __circle_OnChange(double z, double x, double c, double v)
    {
        UpdateFormula();
    }
    public void __circle_Remove(double z, double x)
    {
        _ = z; _ = x;
        Dismantle();
    }
    public void __circle_Moving()
    {
        Formula.Moving = true;
    }
    public void __circle_StopMoving(double z, double x, double c, double v)
    {

        _ = z; _ = x; _ = c; _ = v;
        Formula.Moving = false;
    }





    public override string ToString()
    {
        return $"●{center.Id}";
    }
    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return "Circle " + ToString();
    }

    public override bool Overlaps(Point point)
    {
        return center.GetPosition().DistanceTo(point.X, point.Y) < radius;
    }

    public override double Area()
    {
        return radius * radius * Math.PI;
    }

    public bool Contains(Joint joint)
    {
        return joint == center;
    }

    public bool Contains(Segment segment)
    {
        return false;
    }

    public bool HasMounted(Joint joint)
    {
        return joint.Roles.Has(Role.CIRCLE_On, this);
    }

    public bool HasMounted(Segment segment)
    {
        return segment.Roles.Has(Role.CIRCLE_Tangent, this);
    }


}
