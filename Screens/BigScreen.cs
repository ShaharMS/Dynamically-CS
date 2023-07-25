using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Screens;

public class BigScreen : DraggableGraphic
{

    public static double MouseX
    {
        get => Mouse?.GetPosition(null).X ?? -1;
    }
    public static double MouseY
    {
        get => Mouse?.GetPosition(null).Y ?? -1;
    }

    public static PointerEventArgs Mouse
    {
        get => MainWindow.Mouse;
    }

    private DraggableGraphic _focused;
    public DraggableGraphic FocusedObject
    {
        get => _focused;
        set
        {
            if (value == null) _focused = this;
            else _focused = value;
            FocusManager.Instance?.Focus(_focused, NavigationMethod.Unspecified);
        }
    }

    private DraggableGraphic _hovered;
    public DraggableGraphic HoveredObject
    {
        get => _hovered;
        private set => _hovered = value;
    }

    /// <summary>
    /// Default:
    /// <code>Brushes.LightGray</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? BackgroundColor
    {
        get => MainWindow.BigScreen.Background ?? Brushes.Wheat;
        set => MainWindow.BigScreen.Background = value ?? Brushes.Wheat;
    }

    public BigScreen() : base()
    {
        _focused = this;
        _hovered = this;
        Draggable = false;
        MouseOverCursor = Cursor.Default;

        AddHandler(PointerPressedEvent, SetCurrentFocus, RoutingStrategies.Tunnel);
        MainWindow.Instance.AddHandler(PointerMovedEvent, SetCurrentHover, RoutingStrategies.Tunnel);
    }

    public void Refresh()
    {
        foreach (var child in Children)
        {
            if (child is DraggableGraphic draggable)
            {
                double area = Math.Clamp(draggable.Area(), 0, short.MaxValue);
                if (double.IsNaN(area))
                {
                    Log.Write(child);
                    area = int.MaxValue;
                }
                draggable.ZIndex = -Convert.ToInt32(area);
                draggable.InvalidateVisual();
            }
        }
    }

    public void HandleCreateConnection(Joint from, Joint potential, Dictionary<Role, List<object>>? requiresRoles = null)
    {
        var filtered = new List<Joint>();
        if (requiresRoles != null)
        {
            foreach (var j in Joint.all)
            {
                if (j.Roles.Has(requiresRoles)) filtered.Add(j);
            }
        }
        else filtered = Joint.all;


        potential.ForceStartDrag(MainWindow.Mouse);
        var current = potential;

        var alreadyDisconnected = new List<Joint>();
        foreach (var j in filtered) if (!j.IsConnectedTo(from)) alreadyDisconnected.Add(j);

        from.Connect(current);

        void EvalConnection(object? sender, PointerEventArgs args)
        {
            var pos = args.GetPosition(null);
            bool attached = false;

            foreach (var j in filtered)
            {
                if (j != potential && j.Overlaps(pos))
                {
                    potential.Hidden = true;
                    attached = true;
                    if (alreadyDisconnected.Contains(current)) from.Disconnect(current);
                    current = j;
                    if (alreadyDisconnected.Contains(current)) from.Connect(current);
                    break;
                }
            }

            if (!attached && current != potential)
            {
                potential.Hidden = false;
                if (alreadyDisconnected.Contains(current)) from.Disconnect(current);
                current = potential;
                if (alreadyDisconnected.Contains(current)) from.Connect(current);
            }

        }

        void Finish(object? sender, PointerReleasedEventArgs args)
        {
            if (potential.Hidden) potential.RemoveFromBoard();

            MainWindow.Instance.PointerMoved -= EvalConnection;
            MainWindow.Instance.PointerReleased -= Finish;
        }

        MainWindow.Instance.PointerMoved += EvalConnection;
        MainWindow.Instance.PointerReleased += Finish;
    }

    private void SetCurrentFocus(object? sender, PointerPressedEventArgs e)
    {
        FocusedObject = this;
        foreach (var child in Children)
        {
            if (child is DraggableGraphic draggable && draggable.IsHovered)
            {
                FocusedObject = draggable; // Automatically handles IsFocused on all objects
            }
        }

        if (FocusedObject is BigScreen) Log.Write("No object is focused");
        else if (FocusedObject is Joint joint) Log.Write($"{joint.Id} Is Focused");
        else if (FocusedObject is Connection connection) Log.Write($"{connection.joint1.Id}{connection.joint2.Id} Is Focused");
        else if (FocusedObject is EllipseBase ellipse) Log.Write($"Ellipse {ellipse.focal1.Id}{ellipse.focal2.Id} Is Focused");
    }

    private void SetCurrentHover(object? sender, PointerEventArgs e) // Called from MainWindow
    {
        HoveredObject = this;
        foreach (var child in Children)
        {
            if (child is DraggableGraphic draggable)
            {
                double area = draggable.Area();
                if (area < 0) { Log.Write(child, area); }
                if (double.IsNaN(area))
                {
                    Log.Write(child);
                    area = int.MaxValue;
                }
                draggable.ZIndex = -Convert.ToInt32(area);
                if (draggable.Overlaps(new Point(MouseX, MouseY)) && HoveredObject.Area() > area) HoveredObject = draggable;
                draggable.InvalidateVisual();
            }
        }

        if (HoveredObject is BigScreen) Log.Write("No object is hovered");
        else if (HoveredObject is Joint joint) Log.Write($"{joint.Id} Is Hovered");
        else if (HoveredObject is Connection connection) Log.Write($"{connection.joint1.Id}{connection.joint2.Id} Is Hovered");
        else if (HoveredObject is IShape shape) Log.Write($"{shape.GetType().Name} {shape} Is Hovered");
        else if (HoveredObject is EllipseBase ellipse) Log.Write($"Ellipse {ellipse.focal1.Id}{ellipse.focal2.Id} Is Hovered");
    }

    public override double Area()
    {
        return double.PositiveInfinity;
    }

    public override void Render(DrawingContext context)
    {

    }
}
