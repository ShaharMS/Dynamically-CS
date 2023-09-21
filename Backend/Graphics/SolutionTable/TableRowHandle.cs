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
    
    public TableRow Row;
    public int Serial {
        get => Row.Table.Rows.IndexOf(Row) + 1;
        set {
            Row.Table.MoveRow(Row.Table.Rows.IndexOf(Row), value - 1);
            label.Content = value;
        }
    }

    private Label label;
    private Border border;
    public TableRowHandle(TableRow row) {
        Row = row;
        label = new Label
        {
            FontSize = 12,
            Foreground = UIColors.SolutionTableBorder
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
        Serial = Serial;
        Children.Add(border);

        OnMoved.Add((_, _ , _, _) => {
            Row.AttemptMovement();
            
        });
    }


    public override void Render(DrawingContext context)
    {
        
    }
}
