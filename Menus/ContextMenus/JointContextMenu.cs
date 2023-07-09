using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenu : ContextMenu
{
    public Joint Subject;

    public List<MenuItem> CurrentItems = new();

    public JointContextMenu(Joint subject) {
        Subject = subject;
        ContextMenuOpening += EvaluateSuggestions;
    }

    public void EvaluateSuggestions(object? sender, EventArgs e) {
        // First, Basics:
        CurrentItems.Clear();

        CurrentItems.Add(Generate_Rename());

        Items = CurrentItems;

        InvalidateVisual();
    }

    MenuItem Generate_Rename() {
        var field = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Text = Subject.id.ToString(),
            Watermark = "Letter"
        };
        field.SelectAll();
        field.KeyDown += (sender, e) => {
            if (e.Key == Key.Enter) {
                Subject.id = field.Text.ToCharArray()[0];
            } else if (field.Text.Length > 1) {
                field.Text = e.Key.ToString().ToUpper()[..1];
            }
        };

        var hBar = new DockPanel 
        {
            LastChildFill = true
        };
        DockPanel.SetDock(hBar, Dock.Left);
        hBar.Children.Add(new Label {Content = "New Name:"});
        hBar.Children.Add(field);

        var vBar = new DockPanel();
        DockPanel.SetDock(vBar, Dock.Top);
        vBar.Children.Add(hBar);
        vBar.Children.Add(new Label {Content = "(Press `enter` to confirm)"});

        var item = new MenuItem
        {
            Header = "Rename..."
        };
        var parts = new[] {vBar};
        item.Items = parts;

        return item;
    }
}
