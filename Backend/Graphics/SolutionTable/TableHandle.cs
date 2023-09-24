using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dynamically.Design;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableHandle : DraggableGraphic
{
    public SolutionTable Table;
    
    private Label label;
    private Border border;
    public TableHandle(SolutionTable table) {
        Table = table;
        label = new Label
        {
            FontSize = 12,
            Foreground = UIColors.SolutionTableBorder,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Height = 20,
            Content = "⭤"
        };
        border = new Border {
            Child = label,
            BorderBrush = UIColors.SolutionTableBorder,
            BorderThickness = new Thickness(2),
            Background = UIColors.SolutionTableFill,
            CornerRadius = new CornerRadius(50),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Children.Add(border);

        OnMoved.Add((_, _, _, _) =>
        {
            Table.SetPosition(X + Width / 2 - Table.Width / 2, Y + 50);
        });
    }

    public override double Area() => 0;
}
