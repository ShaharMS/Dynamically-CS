using Avalonia.Controls;
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
    public Triangle Subject;
    public TriangleContextMenuProvider(Triangle triangle, ContextMenu menu)
    {
        Subject = triangle;
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
            Defaults_Dismantle(),
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control>
        {
        };
    }

    public override void GenerateRecommendations()
    {
        Recommendations = Recom_ChangeType();
        Recommendations = (List<Control>)Recommendations.Concat(new List<Control?>
        {

        }.FindAll((c) => c != null).Cast<Control>());
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


    // -------------------------------------------------------
    // ---------------------Recommendations-------------------
    // -------------------------------------------------------

    List<Control> Recom_ChangeType()
    {
        var suggestions = Subject.SuggestTypes();
        var list = new List<Control>();

        foreach ((TriangleType type, string details, double confidence) suggestion in suggestions)
        {
            switch (suggestion.type)
            {
                case TriangleType.EQUILATERAL:
                    var eq = new MenuItem
                    {
                        Header = "★ Make Equilateral",
                        Tag = suggestion.confidence
                    };
                    eq.Click += (sender, e) =>
                    {
                        Subject.MakeEquilateralRelativeToABC(Subject.joint1, Subject.joint2, Subject.joint3);
                    };
                    list.Add(eq);
                    break;
                case TriangleType.ISOSCELES_RIGHT:
                    var ir = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + suggestion.confidence + ") " : "")}{(suggestion.confidence > 0.7 ? "★ " : "")}Make Isosceles-Right {suggestion.details}",
                        Tag = suggestion.confidence
                    };
                    ir.Click += (sender, e) =>
                    {
                        var ang = suggestion.details.Split(",")[0].Remove(0, 1);
                        var main = Joint.GetJointById(ang[1]);
                        var v1 = Joint.GetJointById(ang[0]);
                        var v2 = Joint.GetJointById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.MakeRightRelativeToABC(v1, main, v2);
                        Subject.MakeIsoscelesRelativeToABC(v1, main, v2);
                    };
                    list.Add(ir);
                    break;
                case TriangleType.ISOSCELES:
                    var iso = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + suggestion.confidence + ") " : "")}{(suggestion.confidence > 0.7 ? "★ " : "")}Make Isosceles {suggestion.details}",
                        Tag = suggestion.confidence
                    };
                    iso.Click += (sender, e) =>
                    {
                        var joints = string.Join("", suggestion.details.Split(" = "));
                        Joint? main = null, v1 = null, v2 = null;
                        foreach (char c in joints)
                        {
                            var j = Joint.GetJointById(c);
                            if (v1 == j)
                            {
                                main = j;
                                v1 = null;
                            }
                            else if (v2 == j)
                            {
                                main = j;
                                v1 = null;
                            }
                            else if (v1 == null) v1 = j;
                            else if (v2 == null) v2 = j;
                            else if (main == null) main = j;
                        }
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.MakeIsoscelesRelativeToABC(v1, main, v2);
                    };
                    list.Add(iso);
                    break;
                case TriangleType.RIGHT:
                    var r = new MenuItem
                    {
                        Header = $"{(MainWindow.Debug ? "(" + suggestion.confidence + ") " : "")}{(suggestion.confidence > 0.7 ? "★ " : "")}Make Right {suggestion.details}",
                        Tag = suggestion.confidence
                    };
                    r.Click += (sender, e) =>
                    {
                        var ang = suggestion.details.Remove(0, 1);
                        var main = Joint.GetJointById(ang[1]);
                        var v1 = Joint.GetJointById(ang[0]);
                        var v2 = Joint.GetJointById(ang[2]);
                        if (v1 == null || main == null || v2 == null) return;
                        Subject.MakeRightRelativeToABC(v1, main, v2);
                    };
                    list.Add(r); 
                    break;
                case TriangleType.SCALENE: break;
            }
        }

        list.OrderBy(m => (double?)m.Tag ?? 0.0);

        return list;
    }
}
