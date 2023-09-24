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

    double _stWidth;
    public double StatementsWidth
    {
        get => _stWidth;
        set { var prev = _stWidth; _stWidth = _stWidth.Max(value); if (prev != _stWidth) Refresh(); }
    }
    double _rsWidth;
    public double ReasonsWidth
    {
        get => _rsWidth;
        set {var prev = _rsWidth; _rsWidth = _rsWidth.Max(value); if (prev != _rsWidth) Refresh(); }
    }
    double _fsWidth;
    public double FromsWidth
    {
        get => _fsWidth;
        set {var prev = _fsWidth; _fsWidth = _fsWidth.Max(value); if (prev != _fsWidth) Refresh(); }
    }


    public Canvas VisualList;

    public bool HasFroms;

    ResizableBorder border;

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
            Width = 600
        };

        this.SetPosition(x, y);

        border = new ResizableBorder
        {
            Child = VisualList,
            BorderThickness = new Thickness(3, 3, 3, 3),
            Background = UIColors.SolutionTableFill,
            BorderBrush = UIColors.SolutionTableBorder
        };

        if (!hasFroms )
        {
            FromsWidth = 0;
            StatementsWidth = VisualList.Width / 2;
            ReasonsWidth = VisualList.Width / 2;
        }
        else
        {
            StatementsWidth = VisualList.Width * 2 / 5;
            ReasonsWidth = VisualList.Width * 2 / 5;
            FromsWidth = VisualList.Width * 1 / 5;
        }

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
            if (!double.IsNaN(r.VisualRow.Children[0].Bounds.Width) && r.VisualRow.Children[0].Bounds.Width > StatementsWidth) _stWidth = r.VisualRow.Children[0].Bounds.Width;
            if (!double.IsNaN(r.VisualRow.Children[1].Bounds.Width) && r.VisualRow.Children[1].Bounds.Width > ReasonsWidth) _rsWidth = r.VisualRow.Children[1].Bounds.Width;
            if (HasFroms && !double.IsNaN(r.VisualRow.Children[2].Bounds.Width) && r.VisualRow.Children[2].Bounds.Width > FromsWidth) _fsWidth = r.VisualRow.Children[2].Bounds.Width;
            r.RepositionHandle();
            totalHeight += r.Height;
        }
        foreach (TableRow r in Rows)
        {
            ((dynamic)r.VisualRow.Children[0]).Width = StatementsWidth;
            ((dynamic)r.VisualRow.Children[1]).Width = ReasonsWidth;
            if (HasFroms) ((dynamic)r.VisualRow.Children[2]).Width = FromsWidth;
        }
        VisualList.Height = totalHeight;
        VisualList.Width = StatementsWidth + ReasonsWidth + FromsWidth; 
    }
}
