using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class SegmentContextMenuProvider : ContextMenuProvider
{

    public Segment Subject;
    public SegmentContextMenuProvider(Segment segment, ContextMenu menu)
    {
        Subject = segment;
        Menu = menu;
        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }

    public override void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            Defaults_Disconnect(),
            Defaults_Label()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control> 
        {
            Suggestions_CreateOnSegment(),
            Suggestions_CreateMiddle()
        };
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

    MenuItem Defaults_Disconnect()
    {
        var m = new MenuItem
        {
            Header = $"Disconnect {Subject}"
        };

        m.Click += (s, e) => { Subject.joint1.Disconnect(Subject.joint2); };

        return m;
    }
    MenuItem Defaults_Label()
    {
        var len = new MenuItem
        {
            Header = "Length (Exact) " + (Subject.TextDisplayMode == SegmentTextDisplay.LENGTH_EXACT ? "✓" : "")
        };
        len.Click += (s, e) => { Subject.TextDisplayMode = SegmentTextDisplay.LENGTH_EXACT; };

        var lenR = new MenuItem
        {
            Header = "Length (Rounded) " + (Subject.TextDisplayMode == SegmentTextDisplay.LENGTH_ROUND ? "✓" : "")
        };
        lenR.Click += (s, e) => { Subject.TextDisplayMode = SegmentTextDisplay.LENGTH_ROUND; };

        var none = new MenuItem
        {
            Header = "Nothing " + (Subject.TextDisplayMode == SegmentTextDisplay.NONE ? "✓" : "")
        };
        none.Click += (s, e) => { Subject.Label.Content = "";  Subject.TextDisplayMode = SegmentTextDisplay.NONE; };

        var paramField = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Text = Subject.Label.Content.ToString(),
            Watermark = "Letter",
            MaxLength = 1
        };
        paramField.SelectAll();
        paramField.Focus();
        paramField.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Subject.Label.Content = paramField.Text?.ToCharArray()[0];
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
        hBar.Children.Add(new Label { Content = "Parameter:" });
        hBar.Children.Add(paramField);

        var param = new MenuItem
        {
            Header = "Parameter" + (Subject.TextDisplayMode == SegmentTextDisplay.PARAM ? "✓" : ""),
            Items = new Control[] { hBar, new Label { Content = "(Press `enter` to confirm)" } }
        };

        var customField = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Text = Subject.Label.Content.ToString(),
            Watermark = "Word"
        };
        customField.SelectAll();
        customField.Focus();
        customField.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                Subject.Label.Content = customField.Text?.ToCharArray()[0];
                //Hide hack
                var prev = Subject.ContextMenu;
                Subject.ContextMenu = null;
                Subject.ContextMenu = prev;
            }
        };

        var hBar2 = new DockPanel
        {
            LastChildFill = true
        };
        DockPanel.SetDock(hBar2, Dock.Left);
        hBar2.Children.Add(new Label { Content = "Content:" });
        hBar2.Children.Add(customField);

        var custom = new MenuItem
        {
            Header = "Custom" + (Subject.TextDisplayMode == SegmentTextDisplay.CUSTOM ? "✓" : ""),
            Items = new Control[] { hBar2, new Label { Content = "(Press `enter` to confirm)" } }
        };

        return new MenuItem
        {
            Header = "Label",
            Items = new[]
            {
                len,
                lenR,
                none,
                param,
                custom
            }
        };
    }

    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    MenuItem Suggestions_CreateOnSegment() {
        var c = new MenuItem {
            Header = "Create Interior Joint" 
        };
        c.Click += (o, e) => {
            var j = new Joint(BigScreen.Mouse.GetPosition(null));
            j.Roles.AddToRole(Role.SEGMENT_On, Subject);

            j.ForceStartDrag(MainWindow.Mouse);
        };

        return c;
    }

    MenuItem Suggestions_CreateMiddle() {
        var m = new MenuItem {
            Header = "Create Middle"
        };
        m.Click += (o, e) => {
            var j = new Joint(BigScreen.Mouse.GetPosition(null));
            j.Roles.AddToRole(Role.SEGMENT_Center, Subject);
        };

        return m;
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
            Items = new Control[] { new Label { Content = Keys() } }
        };

        return roles;
    }
}