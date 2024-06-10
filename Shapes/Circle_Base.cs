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
using Dynamically.Containers;
using Dynamically.Menus;
using Dynamically.Design;
using Dynamically.Menus.ContextMenus;
using Avalonia.Controls;

namespace Dynamically.Shapes;
public partial class Circle : EllipseBase
{

    public static new readonly List<Circle> All = new();


    public Vertex Center;

    public List<Action<double, double>> onResize = new();

    public List<Action> OnRemoved = new();

    public double Radius
    {
        get => DistanceSum / 2;
        set
        {
            double prev = DistanceSum / 2;
            DistanceSum = value * 2;
            UpdateFormula();
            foreach (var l in onResize) l(value, prev);
        }
    }

    public new double Opacity {
        get => base.Opacity;
        set {
            base.Opacity = value;
            Ring.Opacity = value;
        }
    }

    public Circle(Vertex center, double radius) : base(center, center, radius * 2)
    {
        All.Add(this);

        this.Radius = radius;
        this.Center = center;

        Formula = new CircleFormula(radius, center.X, center.Y);

        OnDragStart.Add(() => { if (!IsMovable()) CurrentlyDragging = false; });
        OnDragStart.Add(__circle_Moving);
        OnDragStart.Add(() => {
            double offsetX = ParentBoard.MouseX - Center.X;
            double offsetY = ParentBoard.MouseY - Center.Y;
            Center.ForceStartDrag(ParentBoard.Mouse, -offsetX, -offsetY);
        });
        OnMoved.Add(__circle_OnChange);
        OnDragged.Add(__circle_StopMoving);
        OnDragged.Add(MainWindow.RegenAll);


        ContextMenu = new ContextMenu();
        Provider = new CircleContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;
        Ring.ContextMenu = ContextMenu;

        center.Roles.AddToRole(Role.CIRCLE_Center, this);
        center.Reposition();
    }

    public void Set(Vertex center, double radius)
    {
        center.Roles.RemoveFromRole(Role.CIRCLE_Center, this);
        center.Draggable = this.Center.Draggable;
        this.Center = center;
        Focal1 = this.Center;
        Focal2 = this.Center;
        center.Roles.AddToRole(Role.CIRCLE_Center, this);
        this.Radius = radius;
        UpdateFormula();
    }

    static Point FindIntersection(Point p1, Point p2, Point p3, Point p4)
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

    public void Set(Vertex joint1, Vertex joint2, Vertex joint3)
    {

        // Get the perpendicular bisector of (X1, y1) and (x2, y2).
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
        Formula.CenterX = Center.X;
        Formula.CenterY = Center.Y;
        Formula.Radius = Radius;
    }

    public override double GetClosenessToCenter(Point point)
    {
        return point.DistanceTo(Center);
    }
    public override void Render(DrawingContext context)
    {
        if (MainWindow.Instance.MainBoard.HoveredObject == this && (MainWindow.Instance.MainBoard.FocusedObject == this || MainWindow.Instance.MainBoard.FocusedObject is not IShape))
        {
            context.DrawEllipse(UIColors.ShapeHoverFill, null, Center, Radius, Radius);
        }
        else
        {
            context.DrawEllipse(UIColors.ShapeFill, null, Center, Radius, Radius);
        }
        base.Render(context);
    }
}

