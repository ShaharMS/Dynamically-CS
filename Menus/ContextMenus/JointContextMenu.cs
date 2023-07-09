using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;

namespace Dynamically.Menus.ContextMenus;

public class JointContextMenu : ContextMenu
{
    public Joint Subject;

    public List<MenuItem> CurrentItems = new();

    public JointContextMenu(Joint subject) {
        Subject = subject;
        Subject.ContextMenu = this;
        ContextMenuOpening += EvaluateSuggestions;
    }

    public void EvaluateSuggestions(object? sender, EventArgs e) {
        // First, Basics:

    }

    void Generate_Rename() {
        var field = new TextBox
        {
            NewLine = "",
            AcceptsTab = false,
            AcceptsReturn = false,
            Text = Subject.id.ToString(),
            Watermark = "Letter"
        };
        field.SelectAll();
        field.KeyDown += (sender, e) => {
            if (e.Key == Key.Enter) {
                Subject.id = field.Text.ToCharArray()[0];
            } else if (field.Text.Length > 1) {
                field.Text = e.Key.ToString().ToUpper()[..1];
            }
        };
    }
}
