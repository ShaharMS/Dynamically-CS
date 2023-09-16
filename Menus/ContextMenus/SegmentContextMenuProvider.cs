using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Screens;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class SegmentContextMenuProvider : ContextMenuProvider
{

    public Segment Subject {get => _sub; set => _sub = value; }
    public SegmentContextMenuProvider(Segment segment, ContextMenu menu)
    {
        Subject = segment;
        Menu = menu;
        Name = Subject.ToString(true);
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
            Defaults_Label(),
            Defaults_Anchored()
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

    public override void GenerateRecommendations()
    {
        Recommendations = new List<Control?>
        {
            Recom_MakeStraight(),
            Recom_MakeDiameter()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
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
        len.Click += (s, e) => { Subject.TextDisplayMode = SegmentTextDisplay.LENGTH_EXACT; Subject.InvalidateVisual(); };

        var lenR = new MenuItem
        {
            Header = "Length (Rounded) " + (Subject.TextDisplayMode == SegmentTextDisplay.LENGTH_ROUND ? "✓" : "")
        };
        lenR.Click += (s, e) => { Subject.TextDisplayMode = SegmentTextDisplay.LENGTH_ROUND; Subject.InvalidateVisual(); };

        var none = new MenuItem
        {
            Header = "Nothing " + (Subject.TextDisplayMode == SegmentTextDisplay.NONE ? "✓" : "")
        };
        none.Click += (s, e) => { Subject.Label.Content = "";  Subject.TextDisplayMode = SegmentTextDisplay.NONE; Subject.InvalidateVisual(); };

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

    MenuItem? Recom_MakeStraight()
    {
        List<Joint> candidatesj1 = new(), candidatesj2 = new();
        foreach (Joint j in Subject.joint1.Relations)
        {
            if (Tools.QualifiesForStraighten(Subject.joint2, j, Subject.joint1) && Math.Abs(Subject.joint1.DegreesTo(j) - Subject.joint2.DegreesTo(Subject.joint1)) < Settings.ConnectionStraighteningAngleOffset) candidatesj1.Add(j);
        }
        foreach (Joint j in Subject.joint2.Relations)
        {
            if (Tools.QualifiesForStraighten(Subject.joint1, j, Subject.joint2) && Math.Abs(Subject.joint2.DegreesTo(j) - Subject.joint1.DegreesTo(Subject.joint2)) < Settings.ConnectionStraighteningAngleOffset) candidatesj2.Add(j);
        }

        var items = new List<MenuItem>();

        foreach (var joint in candidatesj1)
        {
            var item = new MenuItem
            {
                Header = $"Straighten {(joint.Id > Subject.joint2.Id ? $"{Subject.joint2.Id}{joint.Id}" : $"{joint.Id}{Subject.joint2.Id}")}"
            };
            item.Click += (sender, e) =>
            {
                Joint j1 = Subject.joint1, j2 = Subject.joint2;
                j1.Disconnect(j2, joint);
                j1.Roles.AddToRole(Role.SEGMENT_On, j2.Connect(joint));
            };
            items.Add(item);
        }

        foreach (var joint in candidatesj2)
        {
            var item = new MenuItem
            {
                Header = $"Straighten {(joint.Id > Subject.joint1.Id ? $"{Subject.joint1.Id}{joint.Id}" : $"{joint.Id}{Subject.joint1.Id}")}"
            };
            item.Click += (sender, e) =>
            {
                Joint j1 = Subject.joint1, j2 = Subject.joint2;
                var followers = Subject.Formula.Followers.ToList().Concat(Subject.MiddleFormula.Followers);
                var prev = j2.GetConnectionTo(joint);
                if (prev != null)
                {
                    followers = followers.Concat(prev.Formula.Followers).Concat(prev.MiddleFormula.Followers);
                }
                j2.Disconnect(j1, joint);
                var con = j1.Connect(joint);
                j2.Roles.AddToRole(Role.SEGMENT_On, con);
                foreach(var f in followers.ToHashSet())
                {
                    f.Roles.AddToRole(Role.SEGMENT_On, con);
                }
            };
            items.Add(item);
        }

        if (items.Count == 0) return null;
        else if (items.Count == 1)
        {
            return items[0];
        } else
        {
            return new MenuItem
            {
                Header = "Straighten Segment...",
                Items = items
            };
        }
    }

    MenuItem? Recom_MakeDiameter()
    {
        if (!Subject.Roles.Has(Role.CIRCLE_Chord) || Subject.Roles.CountOf(Role.CIRCLE_Chord) > 1) return null;
        var circle = Subject.Roles.Access<Circle>(Role.CIRCLE_Chord)[0];
        if (circle == null || Subject.Length < circle.radius * Settings.MakeDiameterLengthRatio) return null;
        var item = new MenuItem
        {
            Header = $"Make Diameter ({circle})"
        };
        item.Click += (sender, e) =>
        {
            Subject.Roles.RemoveFromRole(Role.CIRCLE_Chord, circle);
            Subject.Roles.AddToRole(Role.CIRCLE_Diameter, circle);
            // Don't wait for user gesture, update position right after click
            // Place the diameter at a position that makes sense - same slope and a bit longer
            var slope = Subject.Formula.slope;
            var ray = new RayFormula(circle.center, slope);
            var j1pos = ray.GetClosestOnFormula(Subject.joint1);
            if (j1pos != null)
            {
                Subject.joint1.X = j1pos.Value.X;
                Subject.joint1.Y = j1pos.Value.Y;
            }
            var j2pos = ray.GetClosestOnFormula(Subject.joint2);
            if (j2pos != null)
            {
                Subject.joint2.X = j2pos.Value.X;
                Subject.joint2.Y = j2pos.Value.Y;
            }
            Subject.joint1.DispatchOnMovedEvents(Subject.joint1.X, Subject.joint1.Y, Subject.joint1.X, Subject.joint1.Y);
            Subject.joint2.DispatchOnMovedEvents(Subject.joint2.X, Subject.joint2.Y, Subject.joint2.X, Subject.joint2.Y);
            Regenerate();
        };
        return item;
    }

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