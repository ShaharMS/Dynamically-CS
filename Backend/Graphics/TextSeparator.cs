using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Dynamically.Backend.Graphics;
public class TextSeparator : DockPanel
{
    public string Text { get; set; }

    private Label label;

    private ContextMenu Menu;
    public TextSeparator(string text, ContextMenu menu, MenuItem posDeterminer)
    {
        Text = text;
        Menu = menu;
        label = new Label
        {
            FontWeight = FontWeight.Thin,
            FontSize = 8,
            FontFamily = new FontFamily("Consolas"),
            Foreground = new SolidColorBrush(Colors.Black),
            Content = "― " + Text + ": ",
            BorderBrush = new SolidColorBrush(Colors.Black),
            BorderThickness = new Thickness(2)
        };
        Height = double.NaN;
        Width = double.NaN;
        IsEnabled = false;
        SetDock(this, Dock.Left);

        Background = new SolidColorBrush(Colors.Red);

        EventHandler UpdatePos = (object? s, EventArgs e) =>
        {
            var p = posDeterminer.PointToScreen(new Point(-100, -300));
            Log.Write(p.X, p.Y);
            Canvas.SetLeft(label, p.X);
            Canvas.SetTop(label, p.Y);
        };

        posDeterminer.LayoutUpdated += UpdatePos;

        Menu.ContextMenuOpening += (s, e) =>
        {
            MainWindow.BigScreen.Children.Add(label);
        };
        Menu.ContextMenuClosing += (s, e) =>
        {
            MainWindow.BigScreen.Children.Remove(label);
        };
        
    }

    bool once = true;
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        if (!once)
        {
            return base.ArrangeOverride(arrangeSize);
        }
        var b = base.ArrangeOverride(arrangeSize);
        if (Menu == null)
        {
            Log.Write("menuItem is null");
        }
        else if (Menu.Bounds.Width == 0)
        {
            Log.Write("menuItem Width not yet calculated");
            Menu.PropertyChanged += ElementPropertyChanged;

            // Event handler for PropertyChanged event
            void ElementPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
            {
                if (!once) return;
                if (e.Property == Control.BoundsProperty)
                {
                    // Bounds property has changed
                    var newBounds = (Rect)e.NewValue;
                    label.Width = newBounds.Width;
                    double lineWidth = 4;

                    double spaceLeft = newBounds.Width - label.Bounds.Width;
                    int fittingLines = (int)(spaceLeft / lineWidth);
                    Log.Write(newBounds.Width, label.Bounds.Width);
                    Log.Write($"{fittingLines} {spaceLeft} {lineWidth}");
                    if (fittingLines > 0) label.Content += new string('―', fittingLines);

                    once = false;
                }
            }
        }
        else
        {
            label.Width = Menu.Bounds.Width;
            double lineWidth = 4;
            double spaceLeft = Menu.Bounds.Width - label.Bounds.Width;
            int fittingLines = (int)(spaceLeft / lineWidth);
            Log.Write(Menu.Bounds.Width, label.Bounds.Width);
            Log.Write($"{fittingLines} {spaceLeft} {lineWidth}");
            if (fittingLines > 0) label.Content += new string('―', fittingLines);

            once = false;
        }

        return b;
    }
}
