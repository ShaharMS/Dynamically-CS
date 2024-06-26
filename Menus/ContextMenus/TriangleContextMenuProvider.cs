﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Geometry.Basics;


namespace Dynamically.Menus.ContextMenus;

public class TriangleContextMenuProvider : ContextMenuProvider
{
    public Triangle Subject {get => _sub; set => _sub = value; }
    public TriangleContextMenuProvider(Triangle triangle, ContextMenu menu)
    {
        Subject = triangle;
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
            Point p1 = new(Subject.Vertex1.X, Subject.Vertex1.Y), p2 = new(Subject.Vertex2.X, Subject.Vertex2.Y), p3 = new(Subject.Vertex3.X, Subject.Vertex3.Y);
            Point rotationCenter = Subject.Formula.Center;
            double dist1 = Subject.Vertex1.DistanceTo(rotationCenter), dist2 = Subject.Vertex2.DistanceTo(rotationCenter), dist3 = Subject.Vertex3.DistanceTo(rotationCenter);
            double initialRotationRad = rotationCenter.RadiansTo(Subject.ParentBoard.MousePosition);

            double rad1 = rotationCenter.RadiansTo(p1), rad2 = rotationCenter.RadiansTo(p2), rad3 = rotationCenter.RadiansTo(p3);

            void Move(object? sender, PointerEventArgs args)
            {
                var currentRotation = rotationCenter.RadiansTo(args.GetPosition(null)) - initialRotationRad;

                Subject.Vertex1.X = rotationCenter.X + dist1 * Math.Cos(rad1 + currentRotation); Subject.Vertex1.Y = rotationCenter.Y + dist1 * Math.Sin(rad1 + currentRotation);
                Subject.Vertex2.X = rotationCenter.X + dist2 * Math.Cos(rad2 + currentRotation); Subject.Vertex2.Y = rotationCenter.Y + dist2 * Math.Sin(rad2 + currentRotation);
                Subject.Vertex3.X = rotationCenter.X + dist3 * Math.Cos(rad3 + currentRotation); Subject.Vertex3.Y = rotationCenter.Y + dist3 * Math.Sin(rad3 + currentRotation);
                Subject.Vertex1.DispatchOnMovedEvents(); Subject.Vertex2.DispatchOnMovedEvents(); Subject.Vertex3.DispatchOnMovedEvents();
            }

            void Finish(object? sender, PointerPressedEventArgs arg)
            {
                Subject.ParentBoard.Window.PointerMoved -= Move;
                Subject.ParentBoard.Window.PointerPressed -= Finish;
            }

            Subject.ParentBoard.Window.PointerMoved += Move;
            Subject.ParentBoard.Window.PointerPressed += Finish;
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
            Header = $"Remove {Subject.Vertex1}, {Subject.Vertex2} & {Subject.Vertex3}"
        };
        remove.Click += (sender, e) =>
        {
            foreach (var item in new[] { Subject.Vertex3, Subject.Vertex1, Subject.Vertex2 }) item.RemoveFromBoard();
        };
        return remove;
    }

    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    MenuItem Sgest_GenerateCircumCircle()
    {
        if (Subject.Circumcircle != null)
        {
            return new MenuItem
            {
                Header = $"Circum-Circle {Subject.Circumcircle}",
                Items = Subject.Circumcircle.Provider.ItemsWithoutAdjacents
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

        if (Subject.Incircle != null)
        {
            return new MenuItem
            {
                Header = $"Incircle {Subject.Incircle}",
                Items = Subject.Incircle.Provider.ItemsWithoutAdjacents

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
        suggestions.Sort((a, b) => b.confidence.CompareTo(a.confidence));
        var list = new List<Control>();

        foreach ((TriangleType type, string details, double confidence) in suggestions)
        {
            switch (type)
            {
                case TriangleType.EQUILATERAL:
                    var eq = new MenuItem
                    {
                        Header = $"{(Settings.Debug ? "(" + string.Format("{0:0.00}", confidence) + ") " : "")}{(confidence > 0.95 ? "★ " : "")}Make Equilateral",
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
                        Header = $"{(Settings.Debug ? "(" + string.Format("{0:0.00}", confidence) + ") " : "")}{(confidence > 0.95 ? "★ " : "")}Make Isosceles-Right {details}",
                        Tag = confidence
                    };
                    ir.Click += (sender, e) =>
                    {
                        var ang = details.Split(",")[0].Remove(0, 1);
                        var main = Vertex.GetVertexById(ang[1]);
                        var v1 = Vertex.GetVertexById(ang[0]);
                        var v2 = Vertex.GetVertexById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.ForceType(TriangleType.ISOSCELES_RIGHT, v1, main, v2);
                        Regenerate();
                    };
                    list.Add(ir);
                    break;
                case TriangleType.ISOSCELES:
                    var iso = new MenuItem
                    {
                        Header = $"{(Settings.Debug ? "(" + string.Format("{0:0.00}", confidence) + ") " : "")}{(confidence > 0.95 ? "★ " : "")}Make Isosceles {details}",
                        Tag = confidence
                    };
                    iso.Click += (sender, e) =>
                    {
                        var vertices = string.Join("", details.Split(" = "));
                        var dict = new Dictionary<char, int>();
                        foreach (char c in vertices) _ = (dict.ContainsKey(c) ? dict[c]++ : dict[c] = 1);
                        Vertex? main = null, v1 = null, v2 = null;

                        foreach (KeyValuePair<char, int> pair in dict)
                        {
                            if (pair.Value == 1)
                            {
                                if (v1 == null) v1 = Vertex.GetVertexById(pair.Key);
                                else if (v2 == null) v2 = Vertex.GetVertexById(pair.Key);
                            }
                            else main = Vertex.GetVertexById(pair.Key);
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
                        Header = $"{(Settings.Debug ? "(" + string.Format("{0:0.00}", confidence) + ") " : "")}{(confidence > 0.95 ? "★ " : "")}Make Right {details}",
                        Tag = confidence
                    };
                    r.Click += (sender, e) =>
                    {
                        var ang = details.Remove(0, 1);
                        var main = Vertex.GetVertexById(ang[1]);
                        var v1 = Vertex.GetVertexById(ang[0]);
                        var v2 = Vertex.GetVertexById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.ForceType(TriangleType.RIGHT, v1, main, v2);
                        Regenerate();
                    };
                    list.Add(r);
                    break;
                case TriangleType.SCALENE: break;
            }
        }

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
