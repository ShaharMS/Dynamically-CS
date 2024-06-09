using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Circle : IDismantable, IShape, IStringifyable, ISupportsAdjacency, IHasFormula<CircleFormula>, IContextMenuSupporter<CircleContextMenuProvider>, ISelectable, IMovementFreezable
{
    public CircleFormula Formula { get; set; }

    public CircleContextMenuProvider Provider { get; }

    public void Dismantle()
    {
        Center.Roles.RemoveFromRole(Role.CIRCLE_Center, this);
        foreach (var follower in Formula.Followers.ToArray())
        {
            follower.Roles.RemoveFromRole(Role.CIRCLE_On, this);
        }
        Formula.QueueRemoval = true;
        onResize.Clear();
        Circle.All.Remove(this);
        EllipseBase.All.Remove(this);

        foreach (var l in OnRemoved) l();
        OnRemoved.Clear();

        MainWindow.Instance.MainBoard.Children.Remove(Ring);
        MainWindow.Instance.MainBoard.Children.Remove(this);
    }
    public void __circle_OnChange(double z, double x, double c, double v)
    {
        UpdateFormula();
    }
    public void __circle_Remove(double z, double x)
    {
        _ = z; _ = x;
        Log.Write($"Removing circle {this}");
        Dismantle();
    }
    public void __circle_Moving()
    {
        if (CurrentlyDragging) Formula.Moving = true;
    }
    public void __circle_StopMoving(double z, double x, double c, double v)
    {

        _ = z; _ = x; _ = c; _ = v;
        Formula.Moving = false;
    }

    public void __circle_handleDiameter(Vertex dominant, Vertex moving)
    {
        var angle = dominant.RadiansTo(Center);
        var dist = dominant.DistanceTo(Center);

        double nx = dominant.X + dist * 2 * Math.Cos(angle),
               ny = dominant.Y + dist * 2 * Math.Sin(angle),
               ox = moving.X, oy = moving.Y;

        if (!moving.RoughlyEquals(new Point(nx, ny)))
        {
            moving.X = nx; moving.Y = ny;
            moving.DispatchOnMovedEvents(ox, oy);
        }
    }



    public override string ToString()
    {
        return $"●{Center.Id}";
    }
    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return "Circle " + ToString();
    }

    public override bool Overlaps(Point point)
    {
        return Center.DistanceTo(point) < Radius;
    }

    public override double Area()
    {
        return Radius * Radius * Math.PI;
    }

    public bool Contains(Vertex joint)
    {
        return joint == Center;
    }

    public bool Contains(Segment segment)
    {
        return false;
    }

    public bool HasMounted(Vertex joint)
    {
        return joint.Roles.Has(Role.CIRCLE_On, this);
    }

    public bool HasMounted(Segment segment)
    {
        return segment.Roles.Has(Role.CIRCLE_Tangent, this);
    }

    public bool EncapsulatedWithin(Rect rect)
    {
        foreach (var border in new[] {rect.Top, rect.Bottom}) if (Center.DistanceTo(new Point(Center.X, border)) < Radius) return false;
        foreach (var border in new[] {rect.Left, rect.Right}) if (Center.DistanceTo(new Point(border, Center.Y)) < Radius) return false;
        return rect.Contains(Center);
    }

    public bool IsMovable()
    {
        return Center.IsMovable();
    }
}
