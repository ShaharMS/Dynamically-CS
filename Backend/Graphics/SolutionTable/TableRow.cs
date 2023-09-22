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
            border.Child = _vr;
        }
    }
    Border border;

    public SolutionTable Table;

    public TableRowHandle Handle;
    public TableRow(SolutionTable table) : base()
    {
        Width = 200;
        Height = 50;
        Table = table;
        Table.Rows.Add(this);

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
