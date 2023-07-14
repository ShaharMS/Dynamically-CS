using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Screens;

public class BigScreen : DraggableWithContextInfo
{
    private DraggableWithContextInfo _focused;
    public DraggableWithContextInfo FocusedObject
    {
        get => _focused;
        set
        {
            if (value == null) _focused = this;
            else _focused = value;
            if (FocusManager.Instance != null) FocusManager.Instance.Focus(_focused, NavigationMethod.Unspecified);
        }
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
        Draggable = false;
        MouseOverCursor = Cursor.Default;

        AddHandler(PointerPressedEvent, SetCurrentFocus, RoutingStrategies.Tunnel);
    }

    private void SetCurrentFocus(object? sender, PointerPressedEventArgs e)
    {
        FocusedObject = this;
        foreach (var child in Children)
        {
            if (child is DraggableWithContextInfo)
            {
                var draggable = child as DraggableWithContextInfo;
                
                if (draggable != null && draggable.IsHovered)
                {
                    FocusedObject = draggable; // Automatically handles IsFocused on all objects
                } 
            }
        }

        if (FocusedObject is BigScreen) Log.Write("No object is focused");
        else if (FocusedObject is Joint) Log.Write($"{((Joint)FocusedObject).Id} Is Focused");
        else if (FocusedObject is Connection) Log.Write($"{((Connection)FocusedObject).joint1.Id}{((Connection)FocusedObject).joint2.Id} Is Focused");
        else if (FocusedObject is EllipseBase) Log.Write($"Ellipse {((EllipseBase)FocusedObject).focal1.Id}{((EllipseBase)FocusedObject).focal2.Id} Is Focused");
    }

    public override void Render(DrawingContext context)
    {

    }
}
