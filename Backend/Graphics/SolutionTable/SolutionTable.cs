using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CSharpMath.Avalonia;
using Dynamically.Backend.Helpers.Containers;
using Dynamically.Design;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class SolutionTable : DraggableGraphic
{
    public List<TableRow> Rows = new();

    public Canvas VisualList;

    public bool HasFroms;

    Border border;

    public SolutionTable(bool hasFroms = false) : base()
    {
        HasFroms = hasFroms;
        VisualList = new Canvas {
            Width = 200
        };

        border = new Border
        {
            Child = VisualList,
            BorderThickness = new Thickness(3, 3, 3, 3),
            Background = /*UIColors.SolutionTableFill*/ new SolidColorBrush(Colors.Gray),
            BorderBrush = /*UIColors.SolutionTableBorder*/ new SolidColorBrush(Colors.Red),
        };

        Children.Add(border);

        for (int i = 0; i < 4; i++) AddRow(
            new TableRow(this)
            {
                VisualRow = new HDock
                {
                    ChildrenQueued = new List<Control>
                    {
                        new Label { Content = "Statement" },
                        new Label { Content = "Reason" },
                        new Label { Content = "From" }
                    }
                }
            }
        );
    }

    public override void Render(DrawingContext context) {}
    public override double Area() => 0;

    public void MoveRow(int from, int toBefore)
    {
        var _row = Rows[from];
        Rows.RemoveAt(from);
        Rows.Insert(toBefore, _row);

        Refresh();
    }

    public void MoveRow(TableRow _row, int toBefore)
    {
        if (Rows.Count <= toBefore) return;
        Rows.Remove(_row);
        Rows.Insert(toBefore, _row);

        Refresh();
    }

    public void InsertRow(TableRow row, int index) {
        Rows.Insert(index, row);
        VisualList.Children.Add(row);
        row.Handle.Refresh();
        Refresh();
    }
    public void AddRow(TableRow row) {

        Rows.Add(row);
        VisualList.Children.Add(row);
        row.Handle.Refresh();
        Refresh();
    }

    public void Refresh() 
    {
        double totalHeight = 0;
        foreach (TableRow r in Rows) 
        {
            Canvas.SetTop(r, totalHeight);
            totalHeight += r.Height;
        }
        VisualList.Height = totalHeight;
        Log.Write(Rows.Count);
    }
}