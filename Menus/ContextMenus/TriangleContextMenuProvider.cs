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
        Recommendations = new List<Control?>
        {
            
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
            Header = "Dismentle"
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
            foreach (var item in new[] { Subject.joint3, Subject.joint1, Subject.joint2}) item.RemoveFromBoard();
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
        return new();
    }
}
