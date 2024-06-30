using Avalonia.Controls;
using Dynamically.Backend.Geometry;
using Dynamically.Containers;
using Dynamically.Geometry;
using Dynamically.Geometry.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dynamically.Menus.ContextMenus;

public class BoardContextMenuProvider : ContextMenuProvider
{
    public Board Subject { get => _sub; set => _sub = value; }

    public BoardContextMenuProvider(Board board, ContextMenu menu)
    {
        Subject = board;
        Menu = menu;
        Name = Subject.ToString(true);
        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (Settings.Debug) AddDebugInfo();
    }

    public override void Regenerate()
    {
        // Only contains defaults, no need to refresh
    }

    public override void GenerateDefaults()
    {
        Defaults = new List<Control>()
        {
            Defaults_CreateVertex(),
            Defaults_CreateSegment(),
            Defaults_CreateTriangle(),
            Defaults_CreateQuadrilateral(),
            Defaults_CreateCircle(),
        };
    }


    // -------------------------------------------------------
    // ------------------------Defaults-----------------------
    // -------------------------------------------------------
    public Control Defaults_CreateVertex()
    {
        MenuItem item = new MenuItem
        {
            Header = "Create Vertex"
        };
        item.Click += (sender, e) =>
        {
            _ = new Vertex(Subject, Subject.MousePosition);
        };
        return item;
    }

    public Control Defaults_CreateSegment()
    {
        MenuItem item = new MenuItem
        {
            Header = "Create Segment"
        };
        item.Click += (sender, e) =>
        {
            var p = Subject.MousePosition;
            _ = new Segment(new Vertex(Subject, p), new Vertex(Subject, p));
        };
        return item;
    }

    public Control Defaults_CreateCircle()
    {
        MenuItem item = new MenuItem
        {
            Header = "Create Circle"
        };
        item.Click += (sender, e) =>
        {
            var p = Subject.MousePosition;
            _ = new Circle(new Vertex(Subject, p), 100);
        };
        return item;
    }

    public Control Defaults_CreateTriangle()
    {
        MenuItem item = new MenuItem
        {
            Header = "Create Triangle"
        };
        item.Click += (sender, e) =>
        {
            var p = Subject.MousePosition;
            _ = new Triangle(
                new Vertex(Subject, new Avalonia.Point(p.X - 100, p.Y)),
                new Vertex(Subject, new Avalonia.Point(p.X + 100, p.Y)),
                new Vertex(Subject, new Avalonia.Point(p.X, p.Y - 200)));
        };
        return item;
    }

    public Control Defaults_CreateQuadrilateral()
    {
        MenuItem item = new MenuItem
        {
            Header = "Create Quadrilateral"
        };
        item.Click += (sender, e) =>
        {
            var p = Subject.MousePosition;
            _ = new Quadrilateral(new Vertex(Subject, new Avalonia.Point(p.X + 100, p.Y)),
                                  new Vertex(Subject, new Avalonia.Point(p.X - 100, p.Y)),
                                  new Vertex(Subject, new Avalonia.Point(p.X, p.Y - 100)),
                                  new Vertex(Subject, new Avalonia.Point(p.X, p.Y + 100)));
        };
        return item;
    }
}
