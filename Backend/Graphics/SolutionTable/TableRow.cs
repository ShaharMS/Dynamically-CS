using Avalonia;
using Avalonia.Controls;
using CSharpMath.Atom.Atoms;
using CSharpMath.Avalonia;
using CSharpMath.Editor;
using Dynamically.Backend.Helpers.Containers;
using Dynamically.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    Border border;

    public SolutionTable Table;

    public TableRowHandle Handle;
    public TableRow(SolutionTable table) : base()
    {
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
        
        border.SetPosition(0, 0);
        Handle.SetPosition(-50,  Height / 2 - Handle.Height / 2);
        Children.Add(border);
        Children.Add(Handle);
    }

    public void AttemptMovement()
    {
        Handle.SetPosition(-50, Height / 2 - Handle.Height / 2);
    }
}
