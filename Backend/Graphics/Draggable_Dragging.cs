using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public partial class DraggableGraphic : Canvas
{
    public virtual bool Draggable { get; set; }
    public bool CurrentlyDragging;
    public List<Action<double, double, double, double>> OnMoved = new();
    public List<Action<double, double, double, double>> OnDragged = new();

    public Cursor MouseOverCursor = new (StandardCursorType.SizeAll);
    public Cursor MouseOverDisabledCursor = new (StandardCursorType.No);

    private Point _startPosition;
    private Point _startMousePosition;

    public void DispatchOnMovedEvents(double x, double y, double px, double py)
    {
        foreach (var listener in OnMoved)
        {
            listener(x, y, px, py);
        }
    }

    public void DispatchOnDraggedEvents(double x, double y, double px, double py)
    {
        foreach (var listener in OnDragged)
        {
            listener(x, y, px, py);
        }
    }

    public void ForceStartDrag(dynamic args)
    {
        CurrentlyDragging = true;
        _startPosition = new Point(args.GetPosition(null).X, args.GetPosition(null).Y - MainWindow.BigScreen.GetPosition().Y);
        _startMousePosition = args.GetPosition(null);
        args.Pointer.Capture(this);
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

            DispatchOnMovedEvents(X, Y, before.X, before.Y);
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