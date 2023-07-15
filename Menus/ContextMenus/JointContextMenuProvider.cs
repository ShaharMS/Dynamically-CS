using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenuProvider
{
    public List<Control> Items
    {
        get => Defaults.Concat(new List<Control> { new TextSeparator("Suggestions") }).Concat(Suggestions).ToList();
    }

    public List<Control> Defaults = new();
    public List<Control> Suggestions = new();

    public Joint Subject;
    public JointContextMenuProvider(Joint joint)
    {
        Subject = joint;
        GenerateDefaults();
        EvaluateSuggestions();
    }

    public void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            defaults_Rename(),
            defaults_Remove()
        };
    }

    public void GeneratePerShapeDefaults()
    {
        Suggestions = new List<Control> 
        {
            defaults_Remove()
        };
    }

    public void EvaluateSuggestions()
    {

    }


    // --------------------------------------
    // Defaults
    // --------------------------------------
    MenuItem defaults_Rename()
    {
        var field = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Text = Subject.Id.ToString(),
            Watermark = "Letter",
            MaxLength = 1
        };
        field.SelectAll();
        field.Focus();
        field.TextInput += (sender, e) =>
        {
            if (e.Text == "\n" || e.Text == "\n\r")
            {
                Subject.Id = field.Text.ToCharArray()[0];
                //Hide hack
                var prev = Subject.ContextMenu;
                Subject.ContextMenu = null;
                Subject.ContextMenu = prev;
            }
            if (e.Text == null) return;
            field.Text = e.Text.ToUpper();
        };
        field.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Subject.Id = field.Text.ToCharArray()[0];
                //Hide hack
                var prev = Subject.ContextMenu;
                Subject.ContextMenu = null;
                Subject.ContextMenu = prev;
            }
        };

        var hBar = new DockPanel
        {
            LastChildFill = true
        };
        DockPanel.SetDock(hBar, Dock.Left);
        hBar.Children.Add(new Label { Content = "New Name:" });
        hBar.Children.Add(field);

        var rename = new MenuItem
        {
            Header = "Rename...",
            Items = new Control[] { hBar, new Label { Content = "(Press `enter` to confirm)" } }
        };

        return rename;
    }
    MenuItem defaults_Remove()
    {
        var remove = new MenuItem
        {
            Header = "Remove"
        };
        remove.Click += (sender, e) =>
        {
            Subject.RemoveFromBoard();
        };
        return remove;
    }
}

// public class JointContextMenu : ContextMenu
// {
//     public Joint Subject;

//     AvaloniaList<MenuItem> menuItems = new();

//     public JointContextMenu(Joint subject) : base() {
//         Subject = subject;
//         List<object> menuItems = new();

//         MenuItem menuItem1 = new()
//         {
//             Header = "Option 1"
//         };
//         menuItems.Add(menuItem1);

//         MenuItem submenuItem = new()
//         {
//             Header = "Submenu"
//         };

//         MenuItem subMenuItem1 = new MenuItem
//         {
//             Header = "Sub-option 1"
//         };
//         submenuItem.Items = new List<object> { subMenuItem1 };

//         MenuItem subMenuItem2 = new MenuItem
//         {
//             Header = "Sub-option 2"
//         };
//         submenuItem.Items = new List<object> { subMenuItem2 };

//         menuItems.Add(submenuItem);

//         this.Items = menuItems;

//     }

//     public void EvaluateSuggestions() {
//         // First, Basics:
//         menuItems.Clear();

//         //menuItems.Add(Generate_Rename());

//         MenuItem menuItem1 = new MenuItem
//         {
//             Header = "Option 1"
//         };
//         menuItems.Add(menuItem1);

//         MenuItem submenuItem = new MenuItem
//         {
//             Header = "Submenu"
//         };

//         MenuItem subMenuItem1 = new MenuItem
//         {
//             Header = "Sub-option 1"
//         };
//         submenuItem.Items = new List<object> { subMenuItem1 };

//         MenuItem subMenuItem2 = new MenuItem
//         {
//             Header = "Sub-option 2"
//         };
//         submenuItem.Items = new List<object> { subMenuItem2 };

//         menuItems.Add(submenuItem);

//         Items = menuItems;

//         InvalidateVisual();
//     }

//     MenuItem Generate_Rename() {
//         var field = new TextBox
//         {
//             NewLine = "",
//             AcceptsTab = false,
//             AcceptsReturn = false,
//             Text = Subject.Id.ToString(),
//             Watermark = "Letter"
//         };
//         field.SelectAll();
//         field.KeyDown += (sender, e) => {
//             if (e.Key == Key.Enter) {
//                 Subject.Id = field.Text.ToCharArray()[0];
//             } else if (field.Text.Length > 1) {
//                 field.Text = e.Key.ToString().ToUpper()[..1];
//             }
//         };

//         var hBar = new DockPanel 
//         {
//             LastChildFill = true
//         };
//         DockPanel.SetDock(hBar, Dock.Left);
//         hBar.Children.Add(new Label {Content = "New Name:"});
//         hBar.Children.Add(field);

//         var vBar = new DockPanel();
//         DockPanel.SetDock(vBar, Dock.Top);
//         vBar.Children.Add(hBar);
//         vBar.Children.Add(new Label {Content = "(Press `enter` to confirm)"});

//         var item = new MenuItem
//         {
//             Header = "Rename..."
//         };
//         item.Items = new AvaloniaList<Control> { item };

//         return item;
//     }
// }
