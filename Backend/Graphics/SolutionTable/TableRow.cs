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
    public SolutionTable Table;
    HDock row = new();
    Border border;
    TableRowHandle Handle; // todo
    public TableRow(SolutionTable table, double height = double.NaN, double width = double.NaN) : base()
    {
        border = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = UIColors.SolutionTableBorder,
            Background = UIColors.SolutionTableFill,
            Child = row,
            MinHeight = height,
            Width = width
        };

        Handle = new TableRowHandle(this);
        
        border.SetPosition(0, 0);
        Handle.SetPosition(-50, height != double.NaN ? height : 0);
        Children.Add(border);
    }

    public void AttemptMovement()
    {
        Handle.SetPosition(-50, Height / 2 - Handle.Height / 2);
    }
}
