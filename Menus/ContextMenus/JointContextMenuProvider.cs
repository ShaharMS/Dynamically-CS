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

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenuProvider
{
    public List<Control> Items
    {
        get
        {
            var list = Defaults.ToList();
            new TextSeparator("Suggestions", Menu, list);
            list.Concat(Suggestions.ToList());
            new TextSeparator("Recommended", Menu, list);
            list.Concat(Recommendations.ToList());

            return list;
        }
    }

    public List<Control> Defaults = new();
    public List<Control> Suggestions = new();
    public List<Control> Recommendations = new();

    public Joint Subject;
    public ContextMenu Menu;
    public JointContextMenuProvider(Joint joint, ContextMenu menu)
    {
        Subject = joint;
        Menu = menu;
        GenerateDefaults();
        GeneratePerShapeSuggestions();
        EvaluateRecommendations();
        
    }

    public void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            defaults_Rename(),
            defaults_Remove()
        };
    }

    public void GeneratePerShapeSuggestions()
    {
        Log.Write("Eval");
        Suggestions = new List<Control>();

        if (Subject.PartOf.ContainsKey(Role.CIRCLE_Center))
        {
            Log.Write("Circ");
            if (Subject.PartOf[Role.CIRCLE_Center].Count == 1)
            {
                Suggestions.Add(shapedefaults_CrateRadius(Subject.PartOf[Role.CIRCLE_Center].ElementAt(0) as Circle));
            }
        }
    }

    public void EvaluateRecommendations()
    {

    }


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
            var j = new Joint(MainWindow.BigScreen.MouseX, MainWindow.BigScreen.Y);
            j.GeometricPosition.Add(circle.Formula);
            j.ForceStartDrag();
        };

        return item;
    }
}