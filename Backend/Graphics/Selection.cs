using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using ISelectable = Dynamically.Backend.Interfaces.ISelectable;

namespace Dynamically.Backend.Graphics;

public class Selection : DraggableGraphic
{
    double sx;
    double sy;

    double ex;
    double ey;

    public Rect Rect { get => new Rect(sx.Min(ex), sy.Min(ey), Math.Abs(sx - ex), Math.Abs(sy - ey)); }

    HashSet<DraggableGraphic> EncapsulatedElements = new();
    public Selection(Point start)
    {
        sx = ex = start.X; sy = ey = start.Y;

        MainWindow.BigScreen.Children.Add(this);


        MainWindow.Instance.PointerMoved += EvalSelection;
        MainWindow.Instance.PointerReleased += FinishSelection;
    }

    private void FinishSelection(object? sender, PointerReleasedEventArgs e)
    {
        MainWindow.BigScreen.FocusedObject = this;

        MainWindow.Instance.PointerMoved -= EvalSelection;
        MainWindow.Instance.PointerReleased -= FinishSelection;
    }

    private void EvalSelection(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(MainWindow.BigScreen);
        ex = pos.X; ey = pos.Y;
        var rect = Rect; // Use getter once.
        foreach (dynamic item in Joint.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
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
        MainWindow.BigScreen.Children.Remove(this);
        EncapsulatedElements.Clear();
        foreach (var element in Joint.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
        {
            element.Opacity = 1;
        }
    }

    public override string ToString()
    {
        return $"({sx.Min(ex)}, {sy.Min(ey)}) -> ({sx.Max(ex)}, {sy.Max(ey)})";
    }
}
