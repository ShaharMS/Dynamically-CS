using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class TriangleContextMenuProvider : ContextMenuProvider
{
    public Triangle Subject {get => _sub; set => _sub = value; }
    public TriangleContextMenuProvider(Triangle triangle, ContextMenu menu)
    {
        Subject = triangle;
        Menu = menu;
        Name = Subject.ToString(true);
        GetAdjacentElements();
        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }

    public override void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            Defaults_Rotate(),
            Defaults_ChangeType(),
            Defaults_Dismantle(),
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control?>
        {
            Sgest_GenerateCircumCircle(),
            Sgest_GenerateInCircle()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
    }

    public override void GenerateRecommendations()
    {
        Recommendations = Recom_ChangeType();
        Recommendations = Recommendations.Concat(new List<Control?>
        {

        }.FindAll((c) => c != null).Cast<Control>()).ToList();
    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            Debug_Type()
        };
    }


    public override void Regenerate()
    {
        base.Regenerate();
        Name = Subject.ToString(true);
        Subject.ContextMenu = new ContextMenu
        {
            Items = Items
        };
    }


    // -------------------------------------------------------
    // ------------------------Defaults-----------------------
    // -------------------------------------------------------

    MenuItem Defaults_Rotate()
    {
        var rotate = new MenuItem
        {
            Header = "Rotate"
        };
        rotate.Click += (sender, e) =>
        {
            Point p1 = new(Subject.joint1.X, Subject.joint1.Y), p2 = new(Subject.joint2.X, Subject.joint2.Y), p3 = new(Subject.joint3.X, Subject.joint3.Y);
            Point rotationCenter = Subject.GetIncircleCenter();
            double dist1 = Subject.joint1.DistanceTo(rotationCenter), dist2 = Subject.joint2.DistanceTo(rotationCenter), dist3 = Subject.joint3.DistanceTo(rotationCenter);
            double initialRotationRad = rotationCenter.RadiansTo(MainWindow.Mouse.GetPosition(null));

            double rad1 = rotationCenter.RadiansTo(p1), rad2 = rotationCenter.RadiansTo(p2), rad3 = rotationCenter.RadiansTo(p3);

            void Move(object? sender, PointerEventArgs args)
            {
                var currentRotation = rotationCenter.RadiansTo(args.GetPosition(null)) - initialRotationRad;
                Subject.joint1.X = rotationCenter.X + dist1 * Math.Cos(rad1 + currentRotation); Subject.joint1.Y = rotationCenter.Y + dist1 * Math.Sin(rad1 + currentRotation);
                Subject.joint2.X = rotationCenter.X + dist2 * Math.Cos(rad2 + currentRotation); Subject.joint2.Y = rotationCenter.Y + dist2 * Math.Sin(rad2 + currentRotation);
                Subject.joint3.X = rotationCenter.X + dist3 * Math.Cos(rad3 + currentRotation); Subject.joint3.Y = rotationCenter.Y + dist3 * Math.Sin(rad3 + currentRotation);
                Subject.joint1.DispatchOnMovedEvents(); Subject.joint2.DispatchOnMovedEvents(); Subject.joint3.DispatchOnMovedEvents();
            }

            void Finish(object? sender, PointerPressedEventArgs arg)
            {
                MainWindow.Instance.PointerMoved -= Move;
                MainWindow.Instance.PointerPressed -= Finish;
            }

            MainWindow.Instance.PointerMoved += Move;
            MainWindow.Instance.PointerPressed += Finish;
        };


        return rotate;
    }

    MenuItem Defaults_ChangeType()
    {
        var eq = new MenuItem
        {
            Header = "Equilateral " + (Subject.Type == TriangleType.EQUILATERAL ? "✓" : "")
        };
        eq.Click += (s, e) => { Subject.Type = TriangleType.EQUILATERAL; Regenerate(); };

        var iso = new MenuItem
        {
            Header = "Isosceles " + (Subject.Type == TriangleType.ISOSCELES ? "✓" : "")
        };
        iso.Click += (s, e) => { Subject.Type = TriangleType.ISOSCELES; Regenerate(); };
        var r = new MenuItem
        {
            Header = "Right " + (Subject.Type == TriangleType.RIGHT ? "✓" : "")
        };
        r.Click += (s, e) => { Subject.Type = TriangleType.RIGHT; Regenerate(); }; 
        var isor = new MenuItem
        {
            Header = "Isosceles-Right " + (Subject.Type == TriangleType.ISOSCELES_RIGHT ? "✓" : "")
        };
        isor.Click += (s, e) => { Subject.Type = TriangleType.ISOSCELES_RIGHT; Regenerate(); };
        var s = new MenuItem
        {
            Header = "Scalene " + (Subject.Type == TriangleType.SCALENE ? "✓" : "")
        };
        s.Click += (s, e) => { Subject.Type = TriangleType.SCALENE; Regenerate(); };


        return new MenuItem
        {
            Header = "Change Type",
            Items = new[] {eq, iso, r, isor, s }
        };
    }

    MenuItem Defaults_Dismantle()
    {
        var remove = new MenuItem
        {
            Header = "Dismantle"
        };
        remove.Click += (sender, e) =>
        {
            Subject.Dismantle();
        };
        return remove;
    }

    MenuItem Defaults_Remove()
    {
        var remove = new MenuItem
        {
            Header = $"Remove {Subject.joint1}, {Subject.joint2} & {Subject.joint3}"
        };
        remove.Click += (sender, e) =>
        {
            foreach (var item in new[] { Subject.joint3, Subject.joint1, Subject.joint2 }) item.RemoveFromBoard();
        };
        return remove;
    }

    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    MenuItem Sgest_GenerateCircumCircle()
    {
        if (Subject.circumcircle != null)
        {
            return new MenuItem
            {
                Header = $"Circum-Circle {Subject.circumcircle}",
                Items = Subject.circumcircle.Provider.ItemsWithoutAdjacents
            };
        }
        var circum = new MenuItem
        {
            Header = "Generate Circum-Circle",
        };
        circum.Click += (sender, e) =>
        {
            Subject.GenerateCircumCircle();
        };
        return circum;
    }
    MenuItem Sgest_GenerateInCircle()
    {

        if (Subject.incircle != null)
        {
            return new MenuItem
            {
                Header = $"Incircle {Subject.incircle}",
                Items = Subject.incircle.Provider.ItemsWithoutAdjacents

            };
        }
        var incirc = new MenuItem
        {
            Header = "Generate Incircle"
        };
        incirc.Click += (sender, e) =>
        {
            Subject.GenerateInCircle();
        };
        return incirc;
    }

    // -------------------------------------------------------
    // ---------------------Recommendations-------------------
    // -------------------------------------------------------

    List<Control> Recom_ChangeType()
    {
        var suggestions = Subject.SuggestTypes();
        var list = new List<Control>();

        foreach ((TriangleType type, string details, double confidence) in suggestions)
        {
            switch (type)
            {
                case TriangleType.EQUILATERAL:
                    var eq = new MenuItem
                    {
                        Header = "★ Make Equilateral",
                        Tag = confidence
                    };
                    eq.Click += (sender, e) =>
                    {
                        Subject.Type = TriangleType.EQUILATERAL;
                        Regenerate();
                    };
                    list.Add(eq);
                    break;
                case TriangleType.ISOSCELES_RIGHT:
                    var ir = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + confidence + ") " : "")}{(confidence > 0.7 ? "★ " : "")}Make Isosceles-Right {details}",
                        Tag = confidence
                    };
                    ir.Click += (sender, e) =>
                    {
                        var ang = details.Split(",")[0].Remove(0, 1);
                        var main = Vertex.GetJointById(ang[1]);
                        var v1 = Vertex.GetJointById(ang[0]);
                        var v2 = Vertex.GetJointById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.ForceType(TriangleType.ISOSCELES_RIGHT, v1, main, v2);
                        Regenerate();
                    };
                    list.Add(ir);
                    break;
                case TriangleType.ISOSCELES:
                    var iso = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + confidence + ") " : "")}{(confidence > 0.7 ? "★ " : "")}Make Isosceles {details}",
                        Tag = confidence
                    };
                    iso.Click += (sender, e) =>
                    {
                        var joints = string.Join("", details.Split(" = "));
                        var dict = new Dictionary<char, int>();
                        foreach (char c in joints) _ = (dict.ContainsKey(c) ? dict[c]++ : dict[c] = 1);
                        Vertex? main = null, v1 = null, v2 = null;

                        foreach (KeyValuePair<char, int> pair in dict)
                        {
                            if (pair.Value == 1)
                            {
                                if (v1 == null) v1 = Vertex.GetJointById(pair.Key);
                                else if (v2 == null) v2 = Vertex.GetJointById(pair.Key);
                            }
                            else main = Vertex.GetJointById(pair.Key);
                        }

                        if (v1 == null || main == null || v2 == null) return;
                        Subject.ForceType(TriangleType.ISOSCELES, v1, main, v2);
                        Regenerate();
                    };
                    list.Add(iso);
                    break;
                case TriangleType.RIGHT:
                    var r = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + confidence + ") " : "")}{(confidence > 0.7 ? "★ " : "")}Make Right {details}",
                        Tag = confidence
                    };
                    r.Click += (sender, e) =>
                    {
                        var ang = details.Remove(0, 1);
                        var main = Vertex.GetJointById(ang[1]);
                        var v1 = Vertex.GetJointById(ang[0]);
                        var v2 = Vertex.GetJointById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.ForceType(TriangleType.RIGHT, v1, main, v2);
                        Regenerate();
                    };
                    list.Add(r);
                    break;
                case TriangleType.SCALENE: break;
            }
        }

        list = list.OrderBy(m => (double?)m.Tag ?? 0.0).ToList();

        return list;
    }

    // -------------------------------------------------------
    // --------------------------Debug------------------------
    // -------------------------------------------------------

    MenuItem Debug_Type()
    {
        return new MenuItem
        {
            Header = $"Type: {Subject.Type}"
        };
    }
}
