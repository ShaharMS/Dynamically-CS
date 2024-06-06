using Avalonia.Controls;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Interfaces;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class ContextMenuProvider
{

#pragma warning disable CS8618

    protected dynamic _sub;
    public string Name;
#pragma warning restore CS8618

    public virtual List<Control> Items
    {
        get
        {
            GetAdjacentElements();
            GenerateDefaults();
            GenerateSuggestions();
            GenerateRecommendations();
#pragma warning disable CA1806
            var list = new List<Control>();
            new TextSeparator(Name, list);
            list = list.Concat(Defaults.ToList()).ToList();
            new TextSeparator("Suggestions", list);
            list = list.Concat(Suggestions.ToList()).ToList();
            new TextSeparator("Recommended", list);
            list = list.Concat(Recommendations.ToList()).ToList();
            if (AdjacentElements.Count > 0) 
            {
                new TextSeparator("Others", list);
                list = list.Concat(AdjacentElements.ToList()).ToList();
            }
            if (MainWindow.Debug)
            {
                AddDebugInfo();
                new TextSeparator("Debug", list);
                list = list.Concat(Debugging.ToList()).ToList();
            }
#pragma warning restore CA1806

            return list;
        }
    }

    public virtual List<Control> ItemsWithoutAdjacents
    {
        get
        {
#pragma warning disable CA1806
            GenerateDefaults();
            GenerateSuggestions();
            GenerateRecommendations();
            var list = new List<Control>();
            new TextSeparator(Name, list);
            list = list.Concat(Defaults.ToList()).ToList();
            new TextSeparator("Suggestions", list);
            list = list.Concat(Suggestions.ToList()).ToList();
            new TextSeparator("Recommended", list);
            list = list.Concat(Recommendations.ToList()).ToList();
            if (MainWindow.Debug)
            {
                AddDebugInfo();
                new TextSeparator("Debug", list);
                list = list.Concat(Debugging.ToList()).ToList();
            }
#pragma warning restore CA1806

            return list;
        }
    }

    public List<Control> Defaults = new();
    public List<Control> Suggestions = new();
    public List<Control> Recommendations = new();
    public List<Control> AdjacentElements = new();
    public List<Control> Debugging = new();
#pragma warning disable CS8618
    public ContextMenu Menu;
#pragma warning restore CS8618

    public virtual void GenerateDefaults()
    {
        Defaults.Clear();
    }

    public virtual void GenerateSuggestions()
    {
        Suggestions.Clear();
    }

    public virtual void GenerateRecommendations()
    {
        Recommendations.Clear();
    }

    public virtual void AddDebugInfo()
    {
        Debugging.Clear();
    }

    public virtual void GetAdjacentElements() {
        AdjacentElements.Clear();
        var list = new List<ISupportsAdjacency>();
        if (MainWindow.Mouse == null) return;
        var mouse = MainWindow.Mouse.GetPosition(MainWindow.Instance.MainBoard);
        list = list.Concat(Vertex.All).Concat(Segment.All).Concat(Circle.All).Concat(Triangle.All).Concat(Quadrilateral.All).ToList();
        foreach (var item in list) {
            if (!item.Equals(_sub) && item.Overlaps(mouse)) {
                AdjacentElements.Add(new MenuItem {
                    Header = item is IStringifyable ? ((IStringifyable)item).ToString(true) : item.ToString(),
                    Items = ((dynamic)item).Provider.ItemsWithoutAdjacents
                });
            }
        }  
    }

    public virtual void Regenerate()
    {
        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        // Adjacent elements retrieved at click time
        if (MainWindow.Debug) AddDebugInfo();
    }
}
