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
using Dynamically.Shapes;
using Avalonia;
using Dynamically.Screens;
using Dynamically.Backend.Helpers;

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenuProvider
{
    public List<Control> Items
    {
        get
        {
#pragma warning disable CA1806
            var list = Defaults.ToList();
            new TextSeparator("Suggestions", list);
            list = list.Concat(Suggestions.ToList()).ToList();
            new TextSeparator("Recommended", list);
            list = list.Concat(Recommendations.ToList()).ToList();
            if (MainWindow.Debug)
            {
                new TextSeparator("Debug", list);
                list = list.Concat(Debugging.ToList()).ToList();
            }
#pragma warning restore CA1806

            return list;
        }
    }

    public List<Control> Defaults = new();
    public List<Control> Suggestions = new();
    public List<Control> Recommendations = new();
    public List<Control> Debugging = new();

    public Joint Subject;
    public ContextMenu Menu;
    public JointContextMenuProvider(Joint joint, ContextMenu menu)
    {
        Subject = joint;
        Menu = menu;
        GenerateDefaults();
        GeneratePerShapeSuggestions();
        EvaluateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }

    public void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            defaults_Rename(),
            defaults_Remove(),
            defaults_Anchored(),
            defaults_Connect(),
            defaults_Disconnect()
        };
    }

    public void GeneratePerShapeSuggestions()
    {
        //Log.Write("Eval");
        Suggestions = new List<Control>();
        if (Subject.Roles.Has(Role.CIRCLE_Center))
        {
            //Log.Write("Circ");
            if (Subject.Roles.CountOf(Role.CIRCLE_Center) == 1)
            {
                Suggestions.Add(shapedefaults_CrateRadius(Subject.Roles.Access<Circle>(Role.CIRCLE_Center, 0)));
            }
        }
    }

    public void EvaluateRecommendations()
    {

    }

    public void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            debug_DisplayRoles()
        };
    }

    public void Regenerate()
    {
        GenerateDefaults();
        GeneratePerShapeSuggestions();
        EvaluateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();

        Subject.ContextMenu = new ContextMenu
        {
            Items = Items
        };
    }

#pragma warning disable IDE1006
    // -------------------------------------------------------
    // ------------------------Defaults-----------------------
    // -------------------------------------------------------
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
    MenuItem defaults_Connect()
    {
        var connect = new MenuItem
        {
            Header = "Connect to..."
        };

        connect.Click += (s, args) =>
        {
            var potential = new Joint(MainWindow.Mouse.GetPosition(null));
            MainWindow.BigScreen.HandleCreateConnection(Subject, potential);
        };

        return connect;
    }
    MenuItem defaults_Disconnect()
    {
        var options = new List<MenuItem>();

        foreach (var c in Subject.Connections)
        {
            var item = new MenuItem();
            if (c.joint1 == Subject) item.Header = c.joint2 + $" ({c})";
            else item.Header = c.joint2 + $" ({c})";

            item.Click += (sender, e) =>
            {
                c.joint1.Disconnect(c.joint2);
            };

            options.Add(item);
        }

        var dis = new MenuItem
        {
            Header = "Disconnect From...",
            IsEnabled = Subject.Connections.Count > 0,
            Items = options
        };

        return dis;
    }
    MenuItem defaults_Anchored()
    {
        var c = new MenuItem();
        if (Subject.Anchored) c.Header = "Unanchor";
        else c.Header = "Anchor";
        c.Click += (s, args) =>
        {
            Subject.Anchored = !Subject.Anchored;

            if (Subject.Anchored) c.Header = "Unanchor";
            else c.Header = "Anchor";
        };
        return c;
    }
    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    MenuItem shapedefaults_CrateRadius(Circle circle, string circleName = "")
    {
        var text = "Create Radius";
        if (circleName.Length > 0) text += " At " + circleName;

        var item = new MenuItem
        {
            Header = text,
        };
        item.Click += (sender, e) =>
        {
            var j = new Joint(BigScreen.Mouse.GetPosition(null));
            j.Roles.AddToRole(Role.CIRCLE_On, circle);
            MainWindow.BigScreen.HandleCreateConnection(Subject, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { circle })));
        };

        return item;
    }


    // -------------------------------------------------------
    // ----------------------Recommended----------------------
    // -------------------------------------------------------



    // -------------------------------------------------------
    // -------------------------Debug-------------------------
    // -------------------------------------------------------

    MenuItem debug_DisplayRoles()
    {
        string Keys()
        {
            var s = "";
            foreach (var role in Subject.Roles.underlying.Keys)
            {
                if (Subject.Roles.CountOf(role) == 0) continue;
                s += role.ToString();
                s += $" ({Subject.Roles.CountOf(role)}) ({Log.StringifyCollection(Subject.Roles.Access<dynamic>(role))})\n\r";
            }
            if (s == "") return s;
            return s.Substring(0, s.Length - 2);
        }
        var roles = new MenuItem
        {
            Header = "Display Roles",
            Items = new Control[] { new Label { Content = Keys() }}
        };

        return roles;
    }
#pragma warning restore IDE1006
}
