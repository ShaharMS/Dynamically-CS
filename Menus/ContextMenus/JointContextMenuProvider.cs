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

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenuProvider : ContextMenuProvider
{

    public Joint Subject;
    public JointContextMenuProvider(Joint joint, ContextMenu menu)
    {
        Subject = joint;
        Menu = menu;
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
        Suggestions = new List<Control>();
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

            void AddRole(double z, double y, double x, double v)
            {
                j1.Roles.AddToRole(Role.CIRCLE_On, circle);
                j.OnDragged.Remove(AddRole);
            }

            j.OnDragged.Add(AddRole);

            j.Connect(j1);

            j.OnMoved.Add((cx, cy, _, _) =>
            {
                if (!j.CurrentlyDragging) return;
                j1.X = j.X - (j.X - circle.center.X) * 2;
                j1.Y = j.Y - (j.Y - circle.center.Y) * 2;
            });
            j1.OnMoved.Add((cx, cy, _, _) =>
            {
                if (!j1.CurrentlyDragging) return;
                j.X = j1.X - (j1.X - circle.center.X) * 2;
                j.Y = j1.Y - (j1.Y - circle.center.Y) * 2;
            });
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
            if (j != Subject && Subject.DistanceTo(j) <= Settings.JointMergeDistance) veryCloseTo.Add(j);
        }
        veryCloseTo.Sort(new C(Subject));

        if (veryCloseTo.Count == 0) return null;
        if (veryCloseTo.Count == 1)
        {
            var m = new MenuItem
            {
                Header = $"Merge With {veryCloseTo[0]}"
            };
            m.Click += (sender, args) =>
            {
                Subject.Roles.TransferFrom(veryCloseTo[0].Roles);
                veryCloseTo[0].RemoveFromBoard();
            };
            return m;
        }

        var list = new List<MenuItem>();
        foreach (var cj in veryCloseTo)
        {
            var m = new MenuItem
            {
                Header = $"Merge With {cj}"
            };
            m.Click += (sender, args) =>
            {
                Subject.Roles.TransferFrom(cj.Roles);
                cj.RemoveFromBoard();
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
            if (!container.Contains(Subject) && !container.HasMounted(Subject) && container.Formula.DistanceTo(Subject) < Settings.JointMountDistance) {
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
            Items = new Control[] { new Label { Content = Keys() }}
        };

        return roles;
    }
}
