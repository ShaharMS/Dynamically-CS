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

public class SolutionTable : Canvas
{
    public List<TableRow> Rows = new();

    public Canvas VisualList;

    public bool HasFroms;

    Border border;

    public TableHandle Handle;

    public new double Width
    {
        get => VisualList.Width + 6; // Border gutter
        set => VisualList.Width = value - 6; // Border gutter
    }
    public SolutionTable(bool hasFroms = false, double x = 200, double y = 200) : base()
    {
        HasFroms = hasFroms;
        VisualList = new Canvas
        {
            Width = 200
        };

        this.SetPosition(x, y);

        border = new Border
        {
            Child = VisualList,
            BorderThickness = new Thickness(3, 3, 3, 3),
            Background = UIColors.SolutionTableFill,
            BorderBrush = UIColors.SolutionTableBorder
        };

        Handle = new TableHandle(this);

        EventHandler placeHandle = null!;
        placeHandle = (_, _) =>
        {
            Handle.X = x + Width / 2 - Handle.Width / 2;
            Handle.Y = y - 50;
            LayoutUpdated -= placeHandle;
        };
        LayoutUpdated += placeHandle;


        Children.Add(border);
        MainWindow.BigScreen.Children.Add(Handle);

        for (int i = 0; i < 4; i++) AddRow(
            new TableRow(this)
            {
                VisualRow = new HDock
                {
                    ChildrenQueued = new List<Control>
                    {
                        new TextBox { AcceptsReturn = true, Text = $"Statement{i + 1}" },
                        new TextBox { AcceptsReturn = true, Text = $"Reason{i + 1}" },
                        new TextBox { AcceptsReturn = true, Text = $"From{i + 1}" }
                    }
                }
            }
        );
    }

    public override void Render(DrawingContext context) { }
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

    public void InsertRow(TableRow row, int index)
    {
        Rows.Insert(index, row);
        VisualList.Children.Add(row);
        row.Handle.Refresh();
        Refresh();
    }
    public void AddRow(TableRow row)
    {

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
            r.RepositionHandle();
            totalHeight += r.Height;
        }
        VisualList.Height = totalHeight;
        Log.Write(Rows.Count);
    }
}
