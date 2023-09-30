using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Helpers.Containers;
using Dynamically.Backend.Interfaces;
using Dynamically.Screens;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class CircleContextMenuProvider : ContextMenuProvider
{
    public Circle Subject { get => _sub; set => _sub = value; }
    public CircleContextMenuProvider(Circle circle, ContextMenu menu)
    {
        Subject = circle;
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
            Defaults_Dismantle(),
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control>
        {
            Sgest_CreateRadius(),
            Sgest_CreateDiameter(),
        };
    }


    public override void GenerateRecommendations()
    {
        Recommendations = new List<Control?>
        {
            Recom_MarkIntersection()
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
            Header = $"Remove {Subject.center.ToString(true)}"
        };
        remove.Click += (sender, e) =>
        {
            Subject.center.RemoveFromBoard();
        };
        return remove;
    }

    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------

    MenuItem Sgest_CreateRadius()
    {
        var text = "Create Radius";

        var item = new MenuItem
        {
            Header = text,
        };
        item.Click += (sender, e) =>
        {
            var j = new Joint(BigScreen.Mouse.GetPosition(null));
            j.Roles.AddToRole(Role.CIRCLE_On, Subject);
            MainWindow.BigScreen.HandleCreateConnection(Subject.center, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { Subject })));
        };

        return item;
    }

    MenuItem Sgest_CreateDiameter()
    {

        var item = new MenuItem
        {
            Header = "Create Diameter",
        };
        item.Click += (sender, e) =>
        {
            var j = new Joint(BigScreen.Mouse.GetPosition(null));
            j.Roles.AddToRole(Role.CIRCLE_On, Subject);
            MainWindow.BigScreen.HandleCreateConnection(Subject.center, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { Subject })));

            var j1 = new Joint(j.X - (j.X - Subject.center.X) * 2, j.Y - (j.Y - Subject.center.Y) * 2);
            j1.Roles.AddToRole(Role.CIRCLE_On, Subject);
            j1.Connect(Subject.center);

            j.Connect(j1).Roles.AddToRole(Role.CIRCLE_Diameter, Subject);
        };

        return item;
    }


    MenuItem? Recom_MarkIntersection()
    {
        List<dynamic> scanList = Segment.all.ToList<dynamic>().Concat(Circle.all).ToList();

        scanList.Remove(Subject);

        List<MenuItem> intersections = new();
        foreach (var element in scanList)
        {
            if (element is Segment s && !new[] { s.joint1, s.joint2 }.Select(j => Subject.Formula.Intersect(s.Formula)?.Where(p => p.RoughlyEquals(j)).Count() != 0).Contains(false)) continue;
            if (element is Circle circle && ((Point)circle.center).Equals(Subject.center)) continue;
            if (Subject.Formula.Intersects(element.Formula))
            {
                var item = new MenuItem
                {
                    Header = $"Mark Intersection(s) With {(element is IStringifyable ? element.ToString(true) : element.ToString())}"
                };
                item.Click += (_, _) =>
                {
                    var intersections = Subject.Formula.Intersect(element.Formula);
                    foreach (var p in intersections)
                    {
                        var j = new Joint(p);
                        j.X = p.X; j.Y = p.Y;
                        if (element is Circle) j.Roles.AddToRole<Circle>(Role.CIRCLE_On, element);
                        else j.Roles.AddToRole<Segment>(Role.SEGMENT_On, element);
                        j.Roles.AddToRole(Role.CIRCLE_On, Subject);
                    }
                };
                intersections.Add(item);
            }
        }

        if (intersections.Count == 0) return null;
        else if (intersections.Count == 1) return intersections[0];
        else return new MenuItem
        {
            Header = "Mark Intersections...",
            Items = intersections
        };
    }
}
