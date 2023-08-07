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

public class JointContextMenuProvider : ContextMenuProvider
{

    public Joint Subject;
    public JointContextMenuProvider(Joint joint, ContextMenu menu)
    {
        Subject = joint;
        Menu = menu;
        GenerateDefaults();
        GeneratePerShapeSuggestions();
        EvaluateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }

    public override void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            Defaults_Rename(),
            Defaults_Remove(),
            Defaults_Anchored(),
            Defaults_Connect(),
            Defaults_Disconnect()
        };
    }

    public override void GeneratePerShapeSuggestions()
    {
        //Log.Write("Eval");
        Suggestions = new List<Control>();
        if (Subject.Roles.Has(Role.CIRCLE_Center))
        {
            //Log.Write("Circ");
            if (Subject.Roles.CountOf(Role.CIRCLE_Center) == 1)
            {
                Suggestions.Add(ShapeDefaults_CrateRadius(Subject.Roles.Access<Circle>(Role.CIRCLE_Center, 0)));
            }
        }
    }

    public override void EvaluateRecommendations()
    {

    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            Debug_DisplayRoles()
        };
    }

    public override void Regenerate()
    {
        base.Regenerate();
        Subject.ContextMenu = new ContextMenu
        {
            Items = Items
        };
    }

    // -------------------------------------------------------
    // ------------------------Defaults-----------------------
    // -------------------------------------------------------
    MenuItem Defaults_Rename()
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
    MenuItem Defaults_Remove()
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
    MenuItem Defaults_Connect()
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
    MenuItem Defaults_Disconnect()
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
    MenuItem Defaults_Anchored()
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

    MenuItem ShapeDefaults_CrateRadius(Circle circle, string circleName = "")
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

    MenuItem Debug_DisplayRoles()
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
}
