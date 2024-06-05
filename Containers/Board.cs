using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Remote.Protocol.Input;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamically.Containers;

public class Board : DraggableGraphic
{

    public static double MouseX
    {
        get => Mouse?.GetPosition(MainWindow.Instance.MainBoard).X ?? -1;
    }
    public static double MouseY
    {
        get => Mouse?.GetPosition(MainWindow.Instance.MainBoard).Y ?? -1;
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
            _focused.Focus();
        }
    }

    private DraggableGraphic _hovered;
    public DraggableGraphic HoveredObject
    {
        get => _hovered;
        private set => _hovered = value;
    }

    private Selection? _selection;
    public Selection? Selection
    {
        get => _selection;
        set  => _selection = value;
    }

    public Window Window { get; private set; }

    public Board(Window window) : base(null!)
    {
        _focused = this;
        _hovered = this;
        Draggable = false;
        MouseOverCursor = Cursor.Default;

        Window = window;

        Window.AddHandler(PointerPressedEvent, TryStartSelection, RoutingStrategies.Bubble);
        Window.AddHandler(PointerPressedEvent, SetCurrentFocus, RoutingStrategies.Bubble);
        Window.AddHandler(PointerMovedEvent, SetCurrentHover, RoutingStrategies.Bubble);
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
                    Log.Write(child, "Has area of NaN. This should not happen");
                    area = int.MaxValue;
                }
                draggable.ZIndex = -Convert.ToInt32(area);
                draggable.InvalidateVisual();
            }
        }
    }

    public void HandleCreateConnection(Vertex from, Vertex potential, Dictionary<Role, List<object>>? requiresRoles = null)
    {
        var filtered = new List<Vertex>();
        if (requiresRoles != null)
        {
            foreach (var j in Vertex.All)
            {
                if (j.Roles.Has(requiresRoles)) filtered.Add(j);
            }
        }
        else filtered = Vertex.All;
        filtered.Remove(potential);


        potential.ForceStartDrag(MainWindow.Mouse);
        var current = potential;

        var alreadyDisconnected = new List<Vertex>();
        foreach (var j in filtered) if (!j.IsConnectedTo(from)) alreadyDisconnected.Add(j);
        from.Connect(current);

        void EvalConnection(object? sender, PointerEventArgs args)
        {
            var pos = args.GetPosition(null);
            bool attached = false;

            foreach (var j in filtered)
            {
                if (j.Overlaps(pos))
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
            }

            if (current != potential && from.IsConnectedTo(potential)) from.Disconnect(potential);
            else if (current == potential && !from.IsConnectedTo(potential)) from.Connect(potential);

        }

        void Finish(object? sender, PointerReleasedEventArgs args)
        {
            if (potential.Hidden)
            {
                Log.Write(current, potential, potential.Hidden);
                potential.RemoveFromBoard();
#pragma warning disable CS8604 // Possible null reference argument.
                current.CreateBoardRelationsWith(from, from.GetConnectionTo(current));
#pragma warning restore CS8604 // Possible null reference argument.
            } else
            {
                if (!Vertex.All.Contains(potential)) Vertex.All.Add(potential); // Todo - shouldnt be necessary, need to resolve later
#pragma warning disable CS8604 // Possible null reference argument.
                potential.CreateBoardRelationsWith(from, from.GetConnectionTo(potential));
#pragma warning restore CS8604 // Possible null reference argument.
            }

            MainWindow.Instance.PointerMoved -= EvalConnection;
            MainWindow.Instance.PointerReleased -= Finish;

            MainWindow.RegenAll(0,0,0,0);
        }

        MainWindow.Instance.PointerMoved += EvalConnection;
        MainWindow.Instance.PointerReleased += Finish;
    }

    private void SetCurrentFocus(object? sender, PointerPressedEventArgs e)
    {
        if (MainWindow.Instance.WindowTabs.CurrentBoard != this) return;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (e.Source is not DraggableGraphic)
        {
            FocusedObject = this;
            return;
        }
        FocusedObject = this;
        foreach (var child in Children)
        {
            if (child is DraggableGraphic draggable && draggable.IsHovered)
            {
                FocusedObject = draggable; // Automatically handles IsFocused on All objects
            }
        }

        if (FocusedObject is Board) Log.Write("No object is focused");
        else if (FocusedObject is Vertex joint) Log.Write($"{joint.Id} Is Focused");
        else if (FocusedObject is Segment connection) Log.Write($"{connection} Is Focused");
        else if (FocusedObject is Angle angle) Log.Write($"{angle} Is Focused");
        else if (FocusedObject is Circle circle) Log.Write($"Circle {circle} Is Focused");
        else if (FocusedObject is Triangle triangle) Log.Write($"Triangle {triangle} Is Focused");
        else if (FocusedObject is Quadrilateral quadrilateral) Log.Write($"Quadrilateral {quadrilateral} Is Focused");
        else if (FocusedObject is EllipseBase ellipse) Log.Write($"Ellipse {ellipse.Focal1.Id}{ellipse.Focal2.Id} Is Focused");
        else if (FocusedObject is Selection selection) Log.Write($"Selection {selection} Is Focused");

        if (FocusedObject is not Backend.Graphics.Selection) Selection?.Cancel();

    }

    private void TryStartSelection(object? sender, PointerPressedEventArgs e)
    {
        if (MainWindow.Instance.WindowTabs.CurrentBoard != this) return;
        Log.WriteVar(e.Source, FocusedObject, HoveredObject);

        if (e.Source is Border && Selection != null)
        {
            Selection.Cancel();
            FocusedObject = this;
        }
        if (e.Source is not Border || FocusedObject is not DraggableGraphic || e.Source is ResizableBorder || e.Source is LightDismissOverlayLayer) return;

        if (FocusedObject is Selection selection)
        {
            if (selection.IsHovered) return;
            else
            {
                FocusedObject = this;
                selection.Cancel();
            }
        }


        Selection?.Cancel();
        var pos = e.GetPosition(this);
        EventHandler<PointerEventArgs> a = null!;
        a = (_, e) =>
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.PointerMoved -= a;
                Selection?.Cancel();
                return;
            }
            Selection = new Selection(pos, this);
            MainWindow.Instance.PointerMoved -= a;
        };
        MainWindow.Instance.PointerMoved += a;
    }

    private void SetCurrentHover(object? sender, PointerEventArgs e) // Called from MainWindow
    {
        if (MainWindow.Instance.WindowTabs.CurrentBoard != this) return;

        HoveredObject = this;
        foreach (var child in Children)
        {
            if (child is DraggableGraphic draggable)
            {
                double area = draggable.Area();
                if (area < 0) { Log.Write(child, area, "Has area below 0. Fix area algorithm/implement if unimplemented"); }
                if (double.IsNaN(area))
                {
                    Log.Write(child, "Has area of NaN. This should not happen");
                    area = int.MaxValue;
                }
                draggable.ZIndex = -Convert.ToInt32(area);
                if (draggable.Overlaps(new Point(MouseX, MouseY)) && HoveredObject.Area() > area) HoveredObject = draggable;
                draggable.InvalidateVisual();
            } else
            {
                child.ZIndex = 1000000000;
            }
        }
        /*
        if (HoveredObject is MainBoard) Log.Write("No object is hovered");
        else if (HoveredObject is Vertex joint) Log.Write($"{joint.Id} Is Hovered");
        else if (HoveredObject is Segment connection) Log.Write($"{connection.Vertex1.Id}{connection.Vertex2.Id} Is Hovered");
        else if (HoveredObject is IShape shape) Log.Write($"{shape.GetType().Name} {shape} Is Hovered");
        else if (HoveredObject is EllipseBase Ellipse) Log.Write($"Ellipse {Ellipse.Focal1.Id}{Ellipse.Focal2.Id} Is Hovered");
        */
    }

    public override double Area()
    {
        return double.PositiveInfinity;
    }

    public override void Render(DrawingContext context)
    {

    }
}
