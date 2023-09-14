using Avalonia;
using Avalonia.Media;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Backend;
using Dynamically.Screens;
using Dynamically.Menus;
using Dynamically.Design;

namespace Dynamically.Shapes;
public class Circle : EllipseBase, IDismantable, IShape, IStringifyable, IHasFormula<CircleFormula>
{

    public static new readonly List<Circle> all = new();


    public Joint center;

    public List<Action<double, double>> onResize = new();

    public double radius
    {
        get => distanceSum / 2;
        set
        {
            double prev = distanceSum / 2;
            distanceSum = value * 2;
            UpdateFormula();
            foreach (var l in onResize) l(value, prev);
        }
    }

    public CircleFormula Formula { get; set; }
    public List<Action> OnRemoved = new();

    public Circle(Joint center, double radius) : base(center, center, radius * 2)
    {
        this.radius = radius;
        this.center = center;
        Formula = new CircleFormula(radius, center.X, center.Y);

        all.Add(this);

        OnMoved.Add((x, y, px, py) =>
        {
            double pcx = this.center.X, pcy = this.center.Y;
            this.center.X += x - px;
            this.center.Y += y - py;
            this.center.DispatchOnMovedEvents(this.center.X, this.center.Y, pcx, pcy);
            this.SetPosition(0, 0);
        });
        OnMoved.Add(__circle_OnChange);
        OnDragStart.Add(__circle_Moving);
        OnDragged.Add(__circle_StopMoving);

        center.Roles.AddToRole(Role.CIRCLE_Center, this);
        center.reposition();
    }
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

    public void Set(Joint center, double radius)
    {
        center.Roles.RemoveFromRole(Role.CIRCLE_Center, this);
        center.Draggable = this.center.Draggable;
        this.center = center;
        focal1 = this.center;
        focal2 = this.center;
        center.Roles.AddToRole(Role.CIRCLE_Center, this);
        this.radius = radius;
        UpdateFormula();
    }

    public void Set(Joint joint1, Joint joint2, Joint joint3)
    {
        Point FindIntersection(Point p1, Point p2, Point p3, Point p4)
        {
            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            // Find the point of intersection.
            return new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);
        }

        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        double x1 = (joint2.X + joint1.X) / 2;
        double y1 = (joint2.Y + joint1.Y) / 2;
        double dy1 = joint2.X - joint1.X;
        double dx1 = -(joint2.Y - joint1.Y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        double x2 = (joint3.X + joint2.X) / 2;
        double y2 = (joint3.Y + joint2.Y) / 2;
        double dy2 = joint3.X - joint2.X;
        double dx2 = -(joint3.Y - joint2.Y);

        // See where the lines intersect.
        Point intersection = FindIntersection(new Point(x1, y1), new Point(x1 + dx1, y1 + dy1), new Point(x2, y2), new Point(x2 + dx2, y2 + dy2));

        var center = intersection;
        double dx = center.X - joint1.X;
        double dy = center.Y - joint1.Y;
        var radius = Math.Sqrt(dx * dx + dy * dy);

        
        Set(center, radius);
        UpdateFormula();
    }

    public void UpdateFormula()
    {
        if (Formula == null) return;
        Formula.centerX = center.X;
        Formula.centerY = center.Y;
        Formula.radius = radius;
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
    public void __circle_Moving() {
        Formula.Moving = true;
    }
    public void __circle_StopMoving(double z, double x, double c, double v) {

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

    public override double GetClosenessToCenter(Point point)
    {
        return point.DistanceTo(center);
    }
    public override void Render(DrawingContext context)
    {
        if (MainWindow.BigScreen.HoveredObject == this && (MainWindow.BigScreen.FocusedObject == this || MainWindow.BigScreen.FocusedObject is not IShape))
        {
            context.DrawEllipse(UIColors.ShapeHoverFill, null, center, radius, radius);
        }
        else
        {
            context.DrawEllipse(UIColors.ShapeFill, null, center, radius, radius);
        }
        base.Render(context);
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

