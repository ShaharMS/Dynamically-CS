using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dynamically.Design;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableRowHandle : DraggableGraphic
{
    public SolutionTable Table
    {
        get => Row.Table;
    }

    public TableRow Row;
    public int Serial {
        get => Table.Rows.IndexOf(Row) + 1;
        set {
            Table.MoveRow(Row, value - 1);
            label.Content = $" {value} ";
        }
    }

    private Label label;
    private Border border;
    public TableRowHandle(TableRow row) {
        Row = row;
        label = new Label
        {
            FontSize = 12,
            Foreground = UIColors.SolutionTableBorder,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Height = 20
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
        border.SetPosition(0, 0);
        Children.Add(border);

        OnMoved.Add((_, _ , _, _) => {
            Row.AttemptMovement();
        });
    }

    public override void Render(DrawingContext context)
    {
    }

    public void Refresh() {
        Serial = Serial;
    }
}
