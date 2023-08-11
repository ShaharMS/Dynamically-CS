using Avalonia.Controls;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class ContextMenuProvider
{
    public List<Control> Items
    {
        get
        {
#pragma warning disable CA1806
            var list = Defaults.ToList();
            new TextSeparator("Suggestions", list);
            list = list.Concat(Suggestions.ToList()).ToList();
            new TextSeparator("Recommended", list);
            list = list.Concat(Recommendations.ToList()).ToList();
            if (MainWindow.Debug)
            {
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

    public virtual void Regenerate()
    {

        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }
}
