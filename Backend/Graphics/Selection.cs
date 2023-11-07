using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Screens;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Dynamically.Menus.ContextMenus;

namespace Dynamically.Backend.Graphics;

public class Selection : DraggableGraphic, IStringifyable
{
    double sx;
    double sy;

    double ex;
    double ey;

    public Rect Rect { get => new Rect(sx.Min(ex), sy.Min(ey), Math.Abs(sx - ex), Math.Abs(sy - ey)); }

    public HashSet<DraggableGraphic> EncapsulatedElements = new();

    public SelectionContextMenuProvider Provider { get; }
    public Selection(Point start)
    {
        sx = ex = start.X; sy = ey = start.Y;

        MainWindow.BigScreen.Children.Add(this);

        OnMoved.Add((x, y, px, py) => {
            double offsetX = x - px, offsetY = y - py;
            foreach (var item in EncapsulatedElements)
            {
                if (item is not Vertex) continue;
                item.X += offsetX;
                item.Y += offsetY;
            }
            foreach (var item in EncapsulatedElements) if (item is Vertex) item.DispatchOnMovedEvents();
        });
        OnDragStart.Add(() => {
            foreach (var item in EncapsulatedElements) if (item is Vertex) item.DispatchOnDragStartEvents();
        });
        OnDragged.Add((_, _, _ ,_) => {
            foreach (var item in EncapsulatedElements) if (item is Vertex) item.DispatchOnDraggedEvents();
        });

        ContextMenu = new ContextMenu();
        Provider = new SelectionContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        MainWindow.Instance.PointerMoved += EvalSelection;
        MainWindow.Instance.PointerReleased += FinishSelection;
    }

    private void FinishSelection(object? sender, PointerReleasedEventArgs e)
    {
        if (ex.RoughlyEquals(sx) && ey.RoughlyEquals(sy)) {
            Cancel();
            return;
        }

        foreach (dynamic item in Vertex.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
            item.Opacity = 1;

        MainWindow.BigScreen.FocusedObject = this;

        MainWindow.Instance.PointerMoved -= EvalSelection;
        MainWindow.Instance.PointerReleased -= FinishSelection;
    }

    private void EvalSelection(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(MainWindow.BigScreen);
        ex = pos.X; ey = pos.Y;
        var rect = Rect; // Use getter once.
        foreach (dynamic item in Vertex.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
        {
            if (item.EncapsulatedWithin(rect))
            {
                EncapsulatedElements.Add(item);
                item.Opacity = 1;
            }
            else
            {
                EncapsulatedElements.Remove(item);
                item.Opacity = 0.5;
            }
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(UIColors.SelectionFill, UIColors.SelectionOutline, Rect);
    }

    public override double Area()
    {
        return 0;
    }

    public void Cancel()
    {
        MainWindow.Instance.PointerMoved -= EvalSelection;
        MainWindow.Instance.PointerReleased -= FinishSelection;
        MainWindow.BigScreen.Children.Remove(this);
        MainWindow.BigScreen.Selection = null;
        EncapsulatedElements.Clear();
        foreach (var element in Vertex.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
        {
            element.Opacity = 1;
        }
    }

    public override string ToString()
    {
        return $"({sx.Min(ex)}, {sy.Min(ey)}) -> ({sx.Max(ex)}, {sy.Max(ey)})";
    }

    public string ToString(bool descriptive)
    {
        return "Selection";
    }
}
