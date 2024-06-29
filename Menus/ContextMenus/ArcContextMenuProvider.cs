using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Backend.Roles;
using Dynamically.Containers;
using Dynamically.Formulas;
using Dynamically.Geometry;
using Dynamically.Geometry.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class ArcContextMenuProvider : ContextMenuProvider
{
    public Arc Subject { get => _sub; set => _sub = value; }
    public ArcContextMenuProvider(Arc arc, ContextMenu menu)
    {
        Subject = arc;
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
            Defaults_HideCenter()
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


    MenuItem Defaults_HideCenter()
    {
        var hide = new MenuItem
        {
            Header = Subject.Center.Hidden ? $"Show Center ({Subject.Center})" : $"Hide Center ({Subject.Center})"
        };
        hide.Click += (sender, e) =>
        {
            Subject.Center.Hidden = !Subject.Center.Hidden;
            Regenerate();
        };
        return hide;
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
            var j = new Vertex(Subject.ParentBoard, Subject.ParentBoard.MousePosition);
            j.Roles.AddToRole(Role.CIRCLE_On, Subject);
            Subject.ParentBoard.HandleCreateSegment(Subject.Center, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { Subject })));
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
            var j = new Vertex(Subject.ParentBoard, Subject.ParentBoard.MousePosition);
            j.Roles.AddToRole(Role.CIRCLE_On, Subject);
            Subject.ParentBoard.HandleCreateSegment(Subject.Center, j, RoleMap.QuickCreateMap((Role.CIRCLE_On, new[] { Subject })));

            var j1 = new Vertex(Subject.ParentBoard, j.X - (j.X - Subject.Center.X) * 2, j.Y - (j.Y - Subject.Center.Y) * 2);
            j1.Roles.AddToRole(Role.CIRCLE_On, Subject);
            j1.Connect(Subject.Center);

            j.Connect(j1).Roles.AddToRole(Role.CIRCLE_Diameter, Subject);
        };

        return item;
    }


    MenuItem? Recom_MarkIntersection()
    {
        List<dynamic> scanList = Segment.All.ToList<dynamic>().Concat(Circle.All).ToList();

        scanList.Remove(Subject);

        List<MenuItem> intersections = new();
        foreach (var element in scanList)
        {
            if (element is Segment s && (Subject.Formula.Followers.ContainsMany(s.Vertex1, s.Vertex2) || s.Formula.Followers.Intersect(Subject.Formula.Followers).Count() >= (s.Formula.Intersect(Subject.Formula)?.Length ?? 0))) continue;
            if (element is Circle circle && (((Point)circle.Center).RoughlyEquals(Subject.Center) || circle.Formula.Followers.Intersect(Subject.Formula.Followers).Count() >= (circle.Formula.Intersect(Subject.Formula)?.Length ?? 0))) continue;
            if (Subject.Formula.Intersects(element.Formula))
            {
                var item = new MenuItem
                {
                    Header = $"Mark Intersection(s) With {(element is IStringifyable ? element.ToString(true) : element.ToString())}"
                };
                item.Click += (_, _) =>
                {
                    Point[] intersections = Subject.Formula.Intersect(element.Formula);
                    List<Vertex> existing = Subject.Formula.Followers
                        .Where(x => x is Vertex)
                        .Cast<Vertex>()
                        .Intersect(((IHasFormula<Formula>)element).Formula.Followers
                            .Where(x => x is Vertex)
                            .Cast<Vertex>()
                        ).ToList();
                    foreach (var p in intersections)
                    {
                        if (existing.ContainsRoughly(p)) continue;
                        var j = new Vertex(Subject.ParentBoard, p)
                        {
                            X = p.X,
                            Y = p.Y
                        };
                        if (element is Circle) j.Roles.AddToRole<Circle>(Role.CIRCLE_On, element);
                        else j.Roles.AddToRole<Segment>(Role.SEGMENT_On, element);
                        j.Roles.AddToRole(Role.CIRCLE_On, Subject);
                    }

                    element.Provider.Regenerate();
                    Subject.Provider.Regenerate();
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
