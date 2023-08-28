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
using Dynamically.Backend;
using System.Collections;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using System.Reactive.Subjects;
using Avalonia.Media;
using System.Threading;

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenuProvider : ContextMenuProvider
{

    public Joint Subject;
    public JointContextMenuProvider(Joint joint, ContextMenu menu)
    {
        Subject = joint;
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
            Defaults_Rename(),
            Defaults_Anchored(),
            Defaults_Connect(),
            Defaults_Disconnect(),
            Defaults_Dismount(),
            Defaults_Remove(),
        };
    }

    public override void GenerateSuggestions()
    {
        //Log.Write("Eval");
        Suggestions = new List<Control>
        {
            Suggestions_MarkAngle()
        };
        if (Subject.Roles.Has(Role.CIRCLE_Center))
        {
            //Log.Write("Circ");
            if (Subject.Roles.CountOf(Role.CIRCLE_Center) == 1)
            {
                Suggestions.Add(ShapeDefaults_CreateRadius(Subject.Roles.Access<Circle>(Role.CIRCLE_Center, 0)));
                Suggestions.Add(ShapeDefaults_CreateDiameter(Subject.Roles.Access<Circle>(Role.CIRCLE_Center, 0)));
            }
        }
    }

    public override void GenerateRecommendations()
    {
        Recommendations = new List<Control?>
        {
            Recom_MergeJoints(),
            Recom_MountJoints()
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
        bool canPass = false;
        field.KeyDown += (sender, e) =>
        {
            canPass = field.Text.Length > 0 && !IDGenerator.Has(field.Text.ToCharArray()[0]);
            if (canPass) field.Background = null;
            else field.Background = new SolidColorBrush(Colors.Red);
            if (canPass && e.Key == Key.Enter)
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
    MenuItem Defaults_Dismount()
    {
        var options = new List<MenuItem>();

        foreach (var r in new[] { Role.CIRCLE_On, Role.SEGMENT_On, Role.SEGMENT_Center, Role.RAY_On })
        {
            foreach (var obj in Subject.Roles.Access<dynamic>(r))
            {
                var item = new MenuItem
                {
                    Header = obj.ToString(true),
                };
                item.Click += (sender, e) =>
                {
                    Subject.Roles.RemoveFromRole(r, obj);
                    Subject.Provider.Regenerate();
                    Subject.UpdateBoardRelations();
                };
                options.Add(item);
            }
        }

        var dis = new MenuItem
        {
            Header = "Dismount From...",
            IsEnabled = Subject.Roles.Has(Role.CIRCLE_On, Role.RAY_On, Role.SEGMENT_On, Role.SEGMENT_Center),
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

    MenuItem Suggestions_MarkAngle()
    {
        var items = new List<string>();
            foreach(Joint j1 in Joint.all)
            {
                foreach (Joint j2 in Joint.all)
                {
                    if (Subject == j1 || Subject == j2 || j1 == j2) continue;
                    if (Angle.Exists(Subject, j1, j2)) continue;
                    items.Add($"{j1}{Subject}{j2}");
                }
            }
        var l1 = new Label();
        l1.Content = "Angle: ∠";

        var ac = new AutoCompleteBox
        {
            Items = items
        };

        var d1 = new DockPanel();
        d1.Children.Add(l1);
        d1.Children.Add(ac);

        

        var button = new Button
        {
            Content = "Mark Angle",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        button.Click += (s, e) =>
        {
            var arr = ac.Text.ToCharArray();
            if (arr.Length == 3)
            {
                var j1 = Joint.GetJointById(arr[0]);
                var c = Joint.GetJointById(arr[1]);
                var j2 = Joint.GetJointById(arr[2]);
                if (j1 != null && j2 != null && c != null && j1 != j2 && j1 != c && j2 != c)
                {
                    _ = new Angle(j1, c, j2);
                    // Hide hack
                    var prev = Subject.ContextMenu;
                    Subject.ContextMenu = null;
                    Subject.ContextMenu = prev;
                    return;
                }
            }
            button.Content = "Invalid Angle!";
            DockPanel.SetDock(button, Dock.Top);
            DispatcherTimer.Run(() =>
            {
                button.Content = "Mark Angle";
                return false;
            }, new TimeSpan(0, 0, 2));
        };

        return new MenuItem
        {
            Header = "Mark Angle",
            Items = new List<Control>
            {
                d1,
                button
            }
        };
    }

    MenuItem ShapeDefaults_CreateRadius(Circle circle, string circleName = "")
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

    MenuItem ShapeDefaults_CreateDiameter(Circle circle, string circleName = "")
    {
        var text = "Create Diameter";
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

            var j1 = new Joint(j.X - (j.X - circle.center.X) * 2, j.Y - (j.Y - circle.center.Y) * 2);
            j1.Roles.AddToRole(Role.CIRCLE_On, circle);
            j1.Connect(Subject);

            j.Connect(j1).Roles.AddToRole(Role.CIRCLE_Diameter, circle);
        };

        return item;
    }


    // -------------------------------------------------------
    // ----------------------Recommended----------------------
    // -------------------------------------------------------

    class C : IComparer<Joint>
    {
        Joint Subject;
        public C(Joint j) { Subject = j; }
        public int Compare(Joint? x, Joint? y)
        {
            if (y == null || x == null) return int.MaxValue;
            return (int)(Subject.DistanceTo(x) - Subject.DistanceTo(y));
        }
    }

    MenuItem? Recom_MergeJoints()
    {
        List<Joint> veryCloseTo = new();
        foreach (var j in Joint.all)
        {
            if (j != Subject && Subject.DistanceTo(j) <= Settings.JointMergeDistance && Tools.QualifiesForMerge(Subject, j)) veryCloseTo.Add(j);
        }
        veryCloseTo.Sort(new C(Subject));

        if (veryCloseTo.Count == 0) return null;
        if (veryCloseTo.Count == 1)
        {
            var m = new MenuItem
            {
                Header = $"Merge With {veryCloseTo[0]}",
                IsEnabled = !Subject.Anchored
            };
            m.Click += (sender, args) =>
            {
                if (veryCloseTo[0].Anchored)
                {
                    var id = Subject.Id;
                    veryCloseTo[0].Roles.TransferFrom(Subject.Roles);
                    Subject.RemoveFromBoard();
                    veryCloseTo[0].Id = id;
                    veryCloseTo[0].UpdateBoardRelations();
                    veryCloseTo[0].Provider.Regenerate();
                }
                else
                {
                    Subject.Roles.TransferFrom(veryCloseTo[0].Roles);
                    veryCloseTo[0].RemoveFromBoard();
                    Subject.UpdateBoardRelations();
                    Regenerate();
                }
            };
            return m;
        }

        var list = new List<MenuItem>();
        foreach (var cj in veryCloseTo)
        {
            var m = new MenuItem
            {
                Header = $"Merge With {cj}",
                IsEnabled = !Subject.Anchored
            };
            m.Click += (sender, args) =>
            {
                if (cj.Anchored)
                {
                    var id = Subject.Id;
                    cj.Roles.TransferFrom(Subject.Roles);
                    Subject.RemoveFromBoard();
                    cj.Id = id;
                    cj.UpdateBoardRelations();
                    cj.Provider.Regenerate();
                }
                else
                {
                    Subject.Roles.TransferFrom(cj.Roles);
                    cj.RemoveFromBoard();
                    Subject.UpdateBoardRelations();
                    Regenerate();
                }
            };
            list.Add(m);
        }

        var c = new MenuItem
        {
            Header = "Merge With...",
            Items = list
        };

        return c;
    }


    MenuItem? Recom_MountJoints()
    {
        List<dynamic> veryCloseTo = new();
        foreach (var container in new List<dynamic>().Concat(Circle.all).Concat(Segment.all)) // container is IHasFormula<Formula>
        {
            if (!container.Contains(Subject) && !container.HasMounted(Subject) && container.Formula.DistanceTo(Subject) < Settings.JointMountDistance && Tools.QualifiesForMount(Subject, container))
            {
                veryCloseTo.Add(container);
            }
        }
        if (veryCloseTo.Count == 0) return null;
        if (veryCloseTo.Count == 1)
        {
            var m = new MenuItem
            {
                Header = $"Mount On {veryCloseTo[0]}"
            };
            m.Click += (sender, args) =>
            {
                if (veryCloseTo[0] is Circle)
                {
                    Subject.Roles.AddToRole(Role.CIRCLE_On, veryCloseTo[0] as Circle);
                }
                else if (veryCloseTo[0] is Segment)
                {
                    Subject.Roles.AddToRole(Role.SEGMENT_On, veryCloseTo[0] as Segment);
                }
                else Log.Write($"{Subject} cannot mount on {veryCloseTo[0]}");
                Subject.UpdateBoardRelations();
                foreach (Segment c in Subject.Connections) c.Provider.Regenerate();
                Regenerate();
            };
            return m;
        }

        var list = new List<MenuItem>();
        foreach (var cs in veryCloseTo)
        {
            var m = new MenuItem
            {
                Header = $"Mount On {cs}"
            };
            m.Click += (sender, args) =>
            {
                if (cs is Circle)
                {
                    Subject.Roles.AddToRole(Role.CIRCLE_On, cs as Circle);
                }
                else if (cs is Segment)
                {
                    Subject.Roles.AddToRole(Role.SEGMENT_On, cs as Segment);
                }
                else Log.Write($"{Subject} cannot mount on {cs}");
                Subject.UpdateBoardRelations();
                foreach (Segment c in Subject.Connections) c.Provider.Regenerate();
                Regenerate();
            };
            list.Add(m);
        }

        var c = new MenuItem
        {
            Header = "Mount On...",
            Items = list
        };

        return c;
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
