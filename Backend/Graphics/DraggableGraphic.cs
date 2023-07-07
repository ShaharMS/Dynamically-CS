﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public class DraggableGraphic : Canvas
{
    public bool Draggable = true;
    public bool CurrentlyDragging;
    private Point _startPosition;
    private Point _startMousePosition;
    public List<Action<double, double, double, double>> OnMoved = new List<Action<double, double, double, double>>();
    public List<Action<double, double, double, double>> OnDragged = new List<Action<double, double, double, double>>();

    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<DraggableGraphic, double>(nameof(X));

    public double X
    {
        get => GetValue(XProperty);
        set
        {
            SetLeft(this, value);
            //Margin = new Thickness(value, Margin.Top, 0, 0);
            SetValue(XProperty, value);
        }
    }

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<DraggableGraphic, double>(nameof(Y));

    public double Y
    {
        get => GetValue(YProperty);
        set
        {
            SetTop(this, value);
            //Margin = new Thickness(Margin.Left, value, 0, 0);
            SetValue(YProperty, value);
        }
    }

    public new bool IsFocused
    {
        get => MainWindow.BigScreen.FocusedObject == this;
        set => MainWindow.BigScreen.FocusedObject = this;
    }

    public virtual void DecideFocus(Point mousePos)
    {
        var pos = this.GetPosition();
        if (mousePos.X > pos.X && mousePos.X < pos.X + Width && mousePos.Y > pos.Y && mousePos.Y < pos.Y + Height) MainWindow.BigScreen.FocusedObject = this;
    }

    protected override void OnPointerEnter(PointerEventArgs e)
    {
        base.OnPointerEnter(e);
        if (Draggable) Cursor = new Cursor(StandardCursorType.SizeAll);
        else Cursor = new Cursor(StandardCursorType.No);
    }

    protected override void OnPointerLeave(PointerEventArgs e)
    {
        base.OnPointerEnter(e);
        Cursor = new Cursor(StandardCursorType.Arrow);

    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        DecideFocus(e.GetPosition(null));

        if (Draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            CurrentlyDragging = true;
            _startPosition = new Point(X, Y);
            _startMousePosition = e.GetPosition(null);
            e.Pointer.Capture(this);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (CurrentlyDragging && e.Pointer.Captured == this)
        {
            CurrentlyDragging = false;
            e.Pointer?.Capture(null);

            var currentPosition = e.GetPosition(null);
            var endX = X + (currentPosition.X - _startMousePosition.X);
            var endY = Y + (currentPosition.Y - _startMousePosition.Y);

            foreach (var listener in OnDragged)
            {
                listener(_startPosition.X, _startPosition.Y, endX, endY);
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (CurrentlyDragging && e.Pointer.Captured == this)
        {
            var currentPosition = e.GetPosition(null);
            var offset = currentPosition - _startMousePosition;
            X = _startPosition.X + offset.X;
            Y = _startPosition.Y + offset.Y;

            foreach (var listener in OnMoved)
            {
                listener(X, Y, currentPosition.X, currentPosition.Y);
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Define a brush and pen for drawing
        var brush = new SolidColorBrush(Colors.Blue);
        var pen = new Pen(brush, 2);

        // Create a rectangle to draw based on X and Y properties
        var rect = new Rect(new Point(X, Y), new Size(100, 100));

        // Draw the rectangle
        context.DrawRectangle(brush, pen, rect);
    }
}