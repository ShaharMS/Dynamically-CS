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
    
    readonly Label label;
    readonly Border border;

    public new double Width {
        get => border.Bounds.Width;
        set => border.Width = value;
    }
    public new double Height {
        get => border.Bounds.Height;
        set => border.Height = value;
    }
    public TableHandle(SolutionTable table) : base(table.ParentBoard) {
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

        OnMoved.Add((x, y, _, _) =>
        {
            Table.SetPosition(x + border.Bounds.Width / 2 - Table.Width / 2, y + 50);
            foreach(var row in Table.Rows) row.RepositionHandle();
        });
    }

    public override double Area() => 0;
    public override void Render(DrawingContext context) {}
}
