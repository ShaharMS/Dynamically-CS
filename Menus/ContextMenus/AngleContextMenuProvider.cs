using Avalonia.Controls;
using Avalonia.Input;
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
    public Angle Subject;
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
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control>
        {
        };
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
            Header = "Nothing " + (Subject.TextDisplayMode == AngleTextDisplay.DEGREES_EXACT ? "✓" : "")
        };
        none.Click += (s, e) => { Subject.TextDisplayMode = AngleTextDisplay.DEGREES_EXACT; };

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
}
