using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Helpers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class AngleContextMenuProvider : ContextMenuProvider
{
    public Angle Subject { get => _sub; set => _sub = value; }
    public AngleContextMenuProvider(Angle angle, ContextMenu menu)
    {
        Subject = angle;
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
            Defaults_Label(),
            Defaults_Invert(),
            Defaults_CreateInverted(),
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control>
        {
            Sgest_AngleBisector()
        };
    }

    public override void GenerateRecommendations()
    {
        Recommendations = new List<Control?> {
            Recom_MakeRightAngle(),
            Recom_MakeFlatAngle()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
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

    MenuItem Defaults_Label()
    {
        var deg = new MenuItem
        {
            Header = "Degrees (Exact) " + (Subject.TextDisplayMode == AngleTextDisplay.DEGREES_EXACT ? "✓" : "")
        };
        deg.Click += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.DEGREES_EXACT;
        };

        var degR = new MenuItem
        {
            Header = "Degrees (Rounded) " + (Subject.TextDisplayMode == AngleTextDisplay.DEGREES_ROUND ? "✓" : "")
        };
        degR.Click += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.DEGREES_ROUND;
        };

        var stepDeg = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 360,
            AllowSpin = true,
            Increment = 1,
            Value = Subject.Degrees
        };
        stepDeg.ValueChanged += (s, e) =>
        {
            Subject.Label.Content = stepDeg.Value.ToString();
        };
        var degG = new MenuItem
        {
            Header = "Degrees (Custom) " + (Subject.TextDisplayMode == AngleTextDisplay.DEGREES_GIVEN ? "✓" : ""),
            Items = new List<Control>
            {
                new VDock
                {
                    ChildrenQueued = new List<Control>
                    {
                        new HDock
                        {
                            ChildrenQueued = new List<Control>
                            {
                                new Label
                                {
                                    Content = "Value: "
                                },
                                stepDeg
                            }
                        },
                        new Label
                        {
                            Content = "Changing this will not affect the \nangle on screen, but will be \ntaken into account in when solving",
                            IsEnabled = false,
                            Opacity = 0.7,
                            FontSize = 14
                        }
                    }
                }
            }
        };
        degG.SubmenuOpened += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.DEGREES_GIVEN;
        };


        var rad = new MenuItem
        {
            Header = "Radians (Exact) " + (Subject.TextDisplayMode == AngleTextDisplay.RADIANS_EXACT ? "✓" : "")
        };
        rad.Click += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.RADIANS_EXACT;
        };

        var radR = new MenuItem
        {
            Header = "Radians (Rounded) " + (Subject.TextDisplayMode == AngleTextDisplay.RADIANS_ROUND ? "✓" : "")
        };
        radR.Click += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.RADIANS_ROUND;
        };

        var stepRad = new NumericUpDown
        {
            Minimum = 0,
            Maximum = Math.PI * 2,
            AllowSpin = true,
            Increment = Math.PI / 180,
            Value = Subject.Radians,
        };
        stepRad.ValueChanged += (s, e) =>
        {
            Subject.Label.Content = stepRad.Value.ToString();
        };
        var radG = new MenuItem
        {
            Header = "Radians (Custom) " + (Subject.TextDisplayMode == AngleTextDisplay.RADIANS_GIVEN ? "✓" : ""),
            Items = new List<Control>
            {
                new VDock
                {
                    ChildrenQueued = new List<Control>
                    {
                        new HDock
                        {
                            ChildrenQueued = new List<Control>
                            {
                                new Label
                                {
                                    Content = "Value: "
                                },
                                stepRad
                            }
                        },
                        new Label
                        {
                            Content = "Changing this will not affect the \nangle on screen, but will be \ntaken into account in when solving",
                            IsEnabled = false,
                            Opacity = 0.7,
                            FontSize = 14
                        }
                    }
                }
            }
        };
        radG.SubmenuOpened += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.RADIANS_GIVEN;
        };

        var none = new MenuItem
        {
            Header = "Nothing " + (Subject.TextDisplayMode == AngleTextDisplay.NONE ? "✓" : "")
        };
        none.Click += (s, e) => { Subject.TextDisplayMode = AngleTextDisplay.NONE; };

        var paramField = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Watermark = "Letter",
            MaxLength = 1,
            Name = "PARAM_VALUE"
        };
        paramField.SelectAll();
        paramField.Focus();
        paramField.KeyDown += (s, e) =>
        {
            if (e.Key != Key.Enter) return;
            Subject.Label.Content = paramField.Text;
            // Hide hack
            var prev = Subject.ContextMenu;
            Subject.ContextMenu = null;
            Subject.ContextMenu = prev;
        };

        var hBar = new HDock
        {
            LastChildFill = true
        };
        hBar.Children.Add(new Label { Content = "Parameter:" });
        hBar.Children.Add(paramField);

        var param = new MenuItem
        {
            Header = "Parameter" + (Subject.TextDisplayMode == AngleTextDisplay.PARAM ? "✓" : ""),
            Items = new Control[] { hBar, new Label { Content = "(Press `enter` to confirm)" } }
        };
        param.SubmenuOpened += (s, e) =>
        {
            Subject.TextDisplayMode = AngleTextDisplay.PARAM;
        };

        return new MenuItem
        {
            Header = "Angle Label",
            Items = new Control[]
            {
                deg,
                degR,
                degG,
                rad,
                radR,
                radG,
                none,
                param,
            }
        };
    }

    MenuItem Defaults_Invert()
    {
        var invert = new MenuItem
        {
            Header = "Invert Angle"
        };
        invert.Click += (sender, e) =>
        {
            Subject.Large = !Subject.Large;
        };
        return invert;
    }

    MenuItem Defaults_CreateInverted()
    {
        var swap = new MenuItem
        {
            Header = "Create Inverted Angle"
        };
        swap.Click += (sender, e) =>
        {
            _ = new Angle(Subject.Vertex1, Subject.Center, Subject.Vertex2, !Subject.Large);
        };
        return swap;
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


    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    private MenuItem Sgest_AngleBisector()
    {
        if (Subject.BisectorRay.Followers.Count == 0)
        {
            var item = new MenuItem
            {
                Header = "Create Bisector",
            };
            item.Click += (_, _) => {
                Subject.GenerateBisector();
                Regenerate();
            };
            return item;
        }
        else if (Subject.BisectorRay.Followers.Count == 1)
        {
            return new MenuItem
            {
                Header = $"Angle Bisector {Subject.Center.GetConnectionTo(Subject.BisectorRay.Followers[0])}",
                Items = Subject.Center.GetConnectionTo(Subject.BisectorRay.Followers[0])?.Provider.Items ?? new List<Control> {
                    new Label {
                        Content = "Error: Angle bisector not found."
                    }
                }
            };
        }
        else
        {
            var retrieveBisectors = () =>
            {
                var items = new List<MenuItem>();
                foreach (var follower in Subject.BisectorRay.Followers)
                {
                    items.Add(new MenuItem
                    {
                        Header = $"Angle Bisector {Subject.Center.GetConnectionTo(follower)}",
                        Items = Subject.Center.GetConnectionTo(follower)?.Provider.Items ?? new List<Control> {
                            new Label {
                                Content = "Error: Angle bisector not found."
                            }
                        }
                    });
                }
                return items;
            };
            return new MenuItem
            {
                Header = $"Angle Bisectors",
                Items = retrieveBisectors()
            };
        }
    }

    // -------------------------------------------------------
    // ----------------------Recommended----------------------
    // -------------------------------------------------------

    private MenuItem? Recom_MakeRightAngle()
    {
        if (Subject.Degrees < 80 || Subject.Degrees > 100 || Subject.Degrees.RoughlyEquals(90)) return null;

        var m = new MenuItem
        {
            Header = $"{(Subject.Degrees.IsBoundedBy(85, 95) ? "★ " : "")}Make Right Angle"
        };
        m.Click += (sender, e) =>
        {
            // Grab the two vertices, and manipulate them symmetrically
            // But first, find out how to manipulate:

            var l1 = Subject.Center.GetConnectionTo(Subject.Vertex1)!.Length;
            var l2 = Subject.Center.GetConnectionTo(Subject.Vertex2)!.Length;

            var d1 = Subject.Center.DegreesTo(Subject.Vertex1);
            var d2 = Subject.Center.DegreesTo(Subject.Vertex2);

            // If the first segment is "deeper" than the second, we drag it closer for 95deg, and further for 85deg
            int direction = (d1 > d2) && (d1 < 270 && d1 > 90) ? 1 : -1;

            var offset = (90 - Subject.Degrees) / 2;
            Log.WriteVar(d1, d2, direction, offset);

            Subject.Vertex1.X = Subject.Center.X + l1 * Math.Cos((d1 + direction * offset) * Math.PI / 180);
            Subject.Vertex1.Y = Subject.Center.Y + l1 * Math.Sin((d1 + direction * offset) * Math.PI / 180);

            Subject.Vertex2.X = Subject.Center.X + l2 * Math.Cos((d2 - direction * offset) * Math.PI / 180);
            Subject.Vertex2.Y = Subject.Center.Y + l2 * Math.Sin((d2 - direction * offset) * Math.PI / 180);

            Subject.Vertex1.DispatchOnMovedEvents();
            Subject.Vertex2.DispatchOnMovedEvents();
            Subject.Provider.Regenerate();

        };
        return m;
    }
    private MenuItem? Recom_MakeFlatAngle()
    {
        if (Subject.Degrees < 170 || Subject.Degrees > 190 || Subject.Degrees.RoughlyEquals(180)) return null;

        var m = new MenuItem
        {
            Header = $"{(Subject.Degrees.IsBoundedBy(175, 185) ? "★ " : "")}Make Flat Angle"
        };
        m.Click += (sender, e) =>
        {
            // Grab the two vertices, and manipulate them symmetrically
            // But first, find out how to manipulate:

            var l1 = Subject.Center.GetConnectionTo(Subject.Vertex1)!.Length;
            var l2 = Subject.Center.GetConnectionTo(Subject.Vertex2)!.Length;

            var d1 = Subject.Center.DegreesTo(Subject.Vertex1);
            var d2 = Subject.Center.DegreesTo(Subject.Vertex2);

            // If the first segment is "deeper" than the second, we drag it closer for 95deg, and further for 85deg
            int direction = (d1 > d2) && (d1 < 270 && d1 > 180) ? 1 : -1;

            var offset = (180 - Subject.Degrees) / 2;
            Log.WriteVar(d1, d2, direction, offset);

            Subject.Vertex1.X = Subject.Center.X + l1 * Math.Cos((d1 + direction * offset) * Math.PI / 180);
            Subject.Vertex1.Y = Subject.Center.Y + l1 * Math.Sin((d1 + direction * offset) * Math.PI / 180);

            Subject.Vertex2.X = Subject.Center.X + l2 * Math.Cos((d2 - direction * offset) * Math.PI / 180);
            Subject.Vertex2.Y = Subject.Center.Y + l2 * Math.Sin((d2 - direction * offset) * Math.PI / 180);

            Subject.Vertex1.DispatchOnMovedEvents();
            Subject.Vertex2.DispatchOnMovedEvents();
            Subject.Provider.Regenerate();

        };
        return m;
    }
}
