using System.Reflection.Metadata;
using Avalonia;
using Avalonia.Controls;
using CSharpMath.Atom.Atoms;
using CSharpMath.Avalonia;
using CSharpMath.Editor;
using Dynamically.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Containers;
using Dynamically.Backend.Helpers;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableRow : Canvas
{
    HDock _vr;
    public HDock VisualRow
    {
        get => _vr;
        set
        {
            _vr = value;
            _vr.PropertyChanged += (s, e) => {
                if (e.Property.Name != nameof(_vr.Bounds)) return;
                
                var newBounds = _vr.Bounds;
                var oldBounds = (Rect)e.OldValue!;
                if (_vr.Bounds.Width == oldBounds.Width && newBounds.Height == oldBounds.Height) return;
                Height = _vr.Bounds.Height + 1; // border bottom
                Width = _vr.Bounds.Width;
                Table.Refresh();
            };
            border.Child = _vr;
        }
    }
    readonly Border border;

    public SolutionTable Table;

    public TableRowHandle Handle;

    public TableRow(SolutionTable table) : base()
    {
        _vr = new HDock();
        Width = table.Width;
        Height = 20;

        Table = table;

        border = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = UIColors.SolutionTableBorder,
            Background = UIColors.SolutionTableFill,
            Child = VisualRow,
        };

        Handle = new TableRowHandle(this);
        EventHandler placeHandle = null!;
        placeHandle = (_, _) =>
        {
            Log.Write(this.GetPosition().ToString(), Height, Handle.Height);
            if (double.IsNaN(this.GetPosition().Y)) return;
            Handle.X = this.GetPosition().X - 50;
            Handle.Y = this.GetPosition().Y - MainWindow.Instance.MainBoard.GetPosition().Y + Height / 2 - Handle.Height / 2;
            LayoutUpdated -= placeHandle;
        };
        LayoutUpdated += placeHandle;
        
        border.SetPosition(0, 0);
        Children.Add(border);
        MainWindow.Instance.MainBoard.AddChild(Handle);
    }

    public void AttemptMovement()
    {
        Handle.X = this.GetPosition().X - 50;
        int index = 0;

        foreach (TableRow row in Table.Rows) {
            if (row == this) continue;
            if (row.Handle.Y < Handle.Y) index++;
        }

        if (index != Table.Rows.IndexOf(this)) Table.MoveRow(this, index);
    }
    public void RepositionHandle() {
        if (!Handle.CurrentlyDragging) {
            Handle.X = this.GetPosition().X - 50;
            Handle.Y = this.GetPosition().Y - MainWindow.Instance.MainBoard.GetPosition().Y + Height / 2 - Handle.Height / 2;
        }
    }
}
