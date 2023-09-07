using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Dynamically.Shapes;

namespace Dynamically.Menus.ContextMenus;

public class QuadrilateralContextMenuProvider : ContextMenuProvider
{
    
    public Quadrilateral Subject;
    public QuadrilateralContextMenuProvider(Quadrilateral joint, ContextMenu menu)
    {
        Subject = joint;
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
            Defaults_Rename(),
            Defaults_Anchored(),
            Defaults_Connect(),
            Defaults_Disconnect(),
            Defaults_Dismount(),
            Defaults_Remove(),
        };
    }

    public override void GenerateSuggestions()
    {
        //Log.Write("Eval");
        Suggestions = new List<Control>
        {
            Suggestions_MarkAngle()
        };
    }

    public override void GenerateRecommendations()
    {
        Recommendations = new List<Control?>
        {
            Recom_MergeJoints(),
            Recom_MountJoints()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            Debug_DisplayRoles()
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
}
