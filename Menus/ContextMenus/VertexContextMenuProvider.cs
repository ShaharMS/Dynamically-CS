﻿using System.Security.Cryptography;
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
using Dynamically.Geometry;
using Avalonia;
using Dynamically.Containers;
using Dynamically.Backend.Helpers;
using System.Collections;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using System.Reactive.Subjects;
using Avalonia.Media;
using System.Threading;
using Avalonia.Interactivity;
using System.Data;
using Dynamically.Geometry.Basics;
using Dynamically.Backend.Roles;


namespace Dynamically.Menus.ContextMenus;

public class VertexContextMenuProvider : ContextMenuProvider
{

    public Vertex Subject { get => _sub; set => _sub = value; }
    public VertexContextMenuProvider(Vertex vertex, ContextMenu menu)
    {
        Subject = vertex;
        Menu = menu;
        Name = Subject.ToString(true);

        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (Settings.Debug) AddDebugInfo();
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
            Recom_MergeVertices(),
            Recom_MountVertex()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            Debug_DisplayRoles(),
            Debug_BoardPosition(),
            Debug_ScreenPosition()
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
            var potential = new Vertex(Subject.ParentBoard, Subject.ParentBoard.MousePosition);
            Subject.ParentBoard.HandleCreateSegment(Subject, potential);
        };

        return connect;
    }
    MenuItem Defaults_Disconnect()
    {
        var options = new List<MenuItem>();

        foreach (var vertex in Subject.Relations)
        {
            var item = new MenuItem();
            var segment = vertex.GetSegmentTo(Subject)!;
            if (segment.Vertex1 == Subject) item.Header = segment.Vertex2 + $" ({segment})";
            else item.Header = segment.Vertex2 + $" ({segment})";

            item.Click += (sender, e) =>
            {
                segment.Vertex1.Disconnect(segment.Vertex2);
            };

            options.Add(item);
        }

        var dis = new MenuItem
        {
            Header = "Disconnect From...",
            IsEnabled = Subject.Relations.Count > 0,
            Items = options
        };

        return dis;
    }
    MenuItem Defaults_Dismount()
    {
        var options = new List<MenuItem>();

        foreach (var r in new[] { Role.CIRCLE_On, Role.SEGMENT_On, Role.SEGMENT_Center })
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
        var items = new List<(string, double)>();
        foreach (Vertex j1 in Vertex.All)
        {
            foreach (Vertex j2 in Vertex.All)
            {
                if (Subject == j1 || Subject == j2 || j1 == j2) continue;
                if (Angle.Exists(Subject, j1, j2)) continue;
                items.Add(($"{j1}{Subject}{j2}", (Subject.HasSegmentWith(j1) ? 0.5 : 0) + (Subject.HasSegmentWith(j2) ? 0.5 : 0)));
            }
        }
        items.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        var l1 = new Label
        {
            Content = "Angle: ∠"
        };
        var ac = new AutoCompleteBox
        {
            Items = items.Select(x => x.Item1),

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
                var j1 = Vertex.GetVertexById(arr[0]);
                var c = Vertex.GetVertexById(arr[1]);
                var j2 = Vertex.GetVertexById(arr[2]);
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

        var specialSuggestions = items.Where(x => x.Item2 == 1).Select(x => x.Item1).DistinctBy(x =>
        {
            var rel = x.SkipLast(3);
            var res = x[1];
            var oth = new List<char> { x[0], x[2] };
            oth.Sort((a, b) => b.CompareTo(a));
            return res + oth[0] + oth[1];
        }).Select(x =>
        {
            var m = new MenuItem
            {
                Header = "★ Angle: ∠" + x
            };
            m.Click += (sender, args) =>
            {
                ac.Text = x;
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            };
            return m;
        }).ToList();
        

        return new MenuItem
        {
            Header = "Mark Angle",
            Items = specialSuggestions.Concat(new List<Control>
            {
                d1,
                button
            })
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
            var j = new Vertex(Subject.ParentBoard, Subject.ParentBoard.MousePosition);
            j.Roles.AddToRole(Role.CIRCLE_On, circle);
            Subject.ParentBoard.HandleCreateSegment(Subject, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { circle })));
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
            var j = new Vertex(Subject.ParentBoard, Subject.ParentBoard.MousePosition);
            j.Roles.AddToRole(Role.CIRCLE_On, circle);
            Subject.ParentBoard.HandleCreateSegment(Subject, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { circle })));

            var j1 = new Vertex(Subject.ParentBoard, j.X - (j.X - circle.Center.X) * 2, j.Y - (j.Y - circle.Center.Y) * 2);
            j1.Roles.AddToRole(Role.CIRCLE_On, circle);
            j1.Connect(Subject);

            j.Connect(j1).Roles.AddToRole(Role.CIRCLE_Diameter, circle);
        };

        return item;
    }


    // -------------------------------------------------------
    // ----------------------Recommended----------------------
    // -------------------------------------------------------

    class C : IComparer<Vertex>
    {
        readonly Vertex Subject;
        public C(Vertex j) { Subject = j; }
        public int Compare(Vertex? x, Vertex? y)
        {
            if (y == null || x == null) return int.MaxValue;
            return (int)(Subject.DistanceTo(x) - Subject.DistanceTo(y));
        }
    }

    MenuItem? Recom_MergeVertices()
    {
        List<Vertex> veryCloseTo = new();
        foreach (var j in Vertex.All)
        {
            if (j != Subject && Subject.DistanceTo(j) <= Settings.VertexMergeDistance && Tools.QualifiesForMerge(Subject, j)) veryCloseTo.Add(j);
        }
        veryCloseTo.Sort(new C(Subject));

        if (veryCloseTo.Count == 0) return null;
        if (veryCloseTo.Count == 1)
        {
            if (veryCloseTo[0].Id == '_') return null;
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
            if (cj.Id == '_') continue;
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


    MenuItem? Recom_MountVertex()
    {
        List<dynamic> veryCloseTo = new();
        foreach (var container in new List<dynamic>().Concat(Circle.All).Concat(Segment.All)) // container is IHasFormula<Formula>
        {
            if (!container.Contains(Subject) && !container.HasMounted(Subject) && container.Formula.DistanceTo(Subject) < Settings.VertexMountDistance && Tools.QualifiesForMount(Subject, container))
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
                    // To prevent a stack overflow, in case of a circum-circle mounted on an incircle,
                    // We have to mount the circle on the vertex instead of the other way around
                    if (Subject.Roles.Has(Role.TRIANGLE_CircumCircleCenter)) {
                        foreach (Triangle t in Subject.Roles.Access<Triangle>(Role.TRIANGLE_CircumCircleCenter))
                        {
                            if (veryCloseTo[0] == t.Incircle)
                            {
                                // TODO: Workaround, reveisit later
                                var circle = new Circle(Subject.GetMock(), t.Incircle.Radius);
                                t.Incircle.Formula.OnChange.Add(() =>
                                {
                                    if (!circle.Radius.RoughlyEquals(t.Incircle.Radius)) circle.Radius = t.Incircle.Radius;
                                });
                                
                                t.Incircle?.Center.Roles.AddToRole(Role.CIRCLE_On, circle);
                                t.ParentBoard.Children.Remove(circle);
                                t.ParentBoard.Children.Remove(circle.Ring);

                                // Still add the role, just quietly
                                if (Subject.Roles.Underlying.ContainsKey(Role.CIRCLE_On)) Subject.Roles.Underlying[Role.CIRCLE_On].Add(t.Incircle!);
                                else
                                {
                                    Subject.Roles.Underlying[Role.CIRCLE_On] = new List<object> { t.Incircle! };
                                    Subject.Roles.Count++;
                                }
                            }
                        }
                    }
                    else
                        Subject.Roles.AddToRole(Role.CIRCLE_On, veryCloseTo[0] as Circle);
                }
                else if (veryCloseTo[0] is Segment)
                {
                    Subject.Roles.AddToRole(Role.SEGMENT_On, veryCloseTo[0] as Segment);
                }
                else Log.Write($"{Subject} cannot mount on {veryCloseTo[0]}");
                Subject.UpdateBoardRelations();
                foreach (var vertex in Subject.Relations) vertex.GetSegmentTo(Subject)!.Provider.Regenerate();
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
                foreach (var vertex in Subject.Relations) vertex.GetSegmentTo(Subject)!.Provider.Regenerate();
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
            foreach (var role in Subject.Roles.Underlying.Keys)
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

    MenuItem Debug_BoardPosition()
    {
        return new MenuItem
        {
            Header = $"Position: ({((int)Subject.X)}, {((int)Subject.Y)})"
        };
    }

    MenuItem Debug_ScreenPosition()
    {
        return new MenuItem
        {
            Header = $"On-Screen: ({Subject.ScreenX}, {Subject.ScreenY})"
        };
    }
}
