﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Containers;
using Dynamically.Backend.Helpers;

namespace Dynamically.Backend.Graphics;

public partial class DraggableGraphic : Canvas
{
    public virtual bool Draggable { get; set; }

    bool _dragging = false;
    public bool CurrentlyDragging 
    {
        get => _dragging;
        set
        {
            _dragging = value;
            if (_dragging) Log.Write($"Started Dragging: {this}"); else Log.Write($"Stopped Dragging: {this}");
        }
    }
    public List<Action<double, double, double, double>> OnMoved = new();
    public List<Action<double, double, double, double>> OnDragged = new();

    public List<Action> OnDragStart = new();

    public Cursor MouseOverCursor = new (StandardCursorType.SizeAll);
    public Cursor MouseOverDisabledCursor = new (StandardCursorType.Arrow);

    private Point _startPosition;
    private Point _startMousePosition;

    public Board ParentBoard { get; protected set; }

    /// <summary>
    /// when a parameter provided is null, or isnt provided, its reset to Instance X & Y coords.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="px"></param>
    /// <param name="py"></param>
    public virtual void DispatchOnMovedEvents(double? px = null, double? py = null)
    {
        var c = OnMoved.ToArray();
        foreach (var listener in c)
        {
            listener(X, Y, px ?? X, py ?? Y);
        }
    }

    public virtual void DispatchOnDragStartEvents() {
        var c = OnDragStart.ToArray();

        foreach (var listener in c)
        {
            listener();
        }
    }

    /// <summary>
    /// when a parameter provided is null, or isnt provided, its reset to Instance X & Y coords.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="px"></param>
    /// <param name="py"></param>
    public virtual void DispatchOnDraggedEvents(double? x = null, double? y = null, double? px = null, double? py = null)
    {
        var c = OnDragged.ToArray();
        foreach (var listener in c)
        {
            listener(x ?? X, y ?? Y, px ?? X, py ?? Y);
        }
    }

    public void ForceStartDrag(dynamic args, double offsetX = 0, double offsetY = 0)
    {
        CurrentlyDragging = true;
        _startPosition = new Point(args.GetPosition(null).X + offsetX - ParentBoard.GetPosition().X, args.GetPosition(null).Y + offsetY - ParentBoard.GetPosition().Y);
        _startMousePosition = args.GetPosition(null);
        args.Pointer.Capture(this);

        DispatchOnDragStartEvents();
    }

    protected override void OnPointerEnter(PointerEventArgs e)
    {
        base.OnPointerEnter(e);
        if (Draggable) Cursor = MouseOverCursor;
        else Cursor = MouseOverDisabledCursor;
    }

    protected override void OnPointerLeave(PointerEventArgs e)
    {
        base.OnPointerEnter(e);
        Cursor = Cursor.Default;

    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed) 
        { 
            CurrentlyDragging = false;
            e.Pointer?.Capture(null);

            DispatchOnDraggedEvents(X, Y, _startPosition.X, _startPosition.Y);
        }
        if (Draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            CurrentlyDragging = true;
            _startPosition = new Point(X, Y);
            _startMousePosition = e.GetPosition(null);
            e.Pointer?.Capture(this);

            DispatchOnDragStartEvents();

        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (CurrentlyDragging && e.Pointer.Captured == this)
        {
            CurrentlyDragging = false;
            e.Pointer?.Capture(null);

            DispatchOnDraggedEvents(X, Y, _startPosition.X, _startPosition.Y);

        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (CurrentlyDragging && e.Pointer.Captured == this)
        {
            var currentPosition = e.GetPosition(null);
            var offset = currentPosition - _startMousePosition;
            var before = new Point(X, Y);
            X = _startPosition.X + offset.X;
            Y = _startPosition.Y + offset.Y;

            DispatchOnMovedEvents(before.X, before.Y);
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Define a brush and pen for drawing
        var brush = new SolidColorBrush(Colors.Blue);
        var pen = new Pen(brush, 2);

        // Create a rectangle to draw based on X and Y properties
        var rect = new Rect(new Point(0, 0), new Size(100, 100));

        // Draw the rectangle
        context.DrawRectangle(brush, pen, rect);
    }
}