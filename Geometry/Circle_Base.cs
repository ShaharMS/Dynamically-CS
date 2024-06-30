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
using Dynamically.Geometry.Basics;

namespace Dynamically.Geometry;
public partial class Circle : EllipseBase
{

    public static new readonly List<Circle> All = new();

    public new Vertex Center;

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

        OnDragStart.Add(() => {
            double offsetX = ParentBoard.MouseX - Center.X;
            double offsetY = ParentBoard.MouseY - Center.Y;
            Center.ForceStartDrag(ParentBoard.Mouse, -offsetX, -offsetY);
        });
        OnDragStart.Add(() => { CurrentlyDragging = false; });
        OnDragged.Add(AppWindow.RegenAll);
        Ring.OnMoved.Add((_, _, _, _) => { UpdateFormula(); });


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

    public void UpdateFormula()
    {
        if (Formula == null) return;
        Formula.CenterX = Center.X;
        Formula.CenterY = Center.Y;
        Formula.Radius = Radius;
    }

    public override void Render(DrawingContext context)
    {
        if (ParentBoard.HoveredObject == this && (ParentBoard.FocusedObject == this || ParentBoard.FocusedObject is not IShape))
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

