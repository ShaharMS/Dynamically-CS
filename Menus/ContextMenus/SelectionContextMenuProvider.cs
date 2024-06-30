using Avalonia.Controls;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public class SelectionContextMenuProvider : ContextMenuProvider
{
    public Selection Subject { get => _sub; set => _sub = value; }

    public override List<Control> Items => base.ItemsWithoutAdjacents;
    public SelectionContextMenuProvider(Selection s, ContextMenu menu)
    {
        Subject = s;
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
            Defaults_GenerateExercise(),
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

    MenuItem Defaults_GenerateExercise()
    {
        return new MenuItem
        {
            Header = "Generate Exercise..."
        };
    }
    MenuItem Defaults_Remove()
    {
        var remove = new MenuItem
        {
            Header = "Remove Elements"
        };
        remove.Click += (sender, e) =>
        {
            foreach (dynamic item in Subject.EncapsulatedElements)
            {
                item.RemoveFromBoard();
            }

            Subject.Cancel();
        };
        return remove;
    }
}


    // -------------------------------------------------------
    // -----------------------Suggestions---------------------
    // -------------------------------------------------------