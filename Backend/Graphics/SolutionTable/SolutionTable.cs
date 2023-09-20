using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CSharpMath.Avalonia;

namespace Dynamically.Backend.Graphics;

public class SolutionTable : DataGrid
{
    public bool hasFroms;

    private List<Control> Elements = new();
    public SolutionTable(bool hasFroms = false) : base()
    {
        this.hasFroms = hasFroms;

        CanUserResizeColumns = true;
        CanUserSortColumns = false;
        CanUserReorderColumns = false;

        Foreground = new SolidColorBrush(Colors.White);
        Background = new SolidColorBrush(Colors.Red); 

        this.SetPosition(0, 0);


        DataGridColumnHeader Statements = new DataGridColumnHeader
        {
            Content = "Statements"
        }, Reasons = new DataGridColumnHeader
        {
            Content = "Reasons"
        }, Froms = new DataGridColumnHeader
        {
            Content = "From"
        };

        Add((Statements, 0, 0), (Reasons, 1, 0));
        if (hasFroms) Add(Froms, 2, 0);
    }

    private void Add(Control cell, int x, int y)
    {
        Grid.SetColumn(cell, x);
        Grid.SetRow(cell, y);

        Elements.Add(cell);
        Items = Elements;
    }
    private void Add(params (Control cell, int x, int y)[] cells)
    {
        foreach ((Control cell, int x, int y) cell in cells)
        {
            Grid.SetColumn(cell.cell, cell.x);
            Grid.SetRow(cell.cell, cell.y);

            Elements.Add(cell.cell);
        }
        
        Items = Elements;
    }

    public (DataGridCell statement, DataGridCell reason, DataGridCell from) GenericAdd(string state, string res, IEnumerable<int> fm)
    {
        var f1 = new MathView
        {
            LaTeX = state
        };
        var f2 = new AutoCompleteBox
        {
            Text = res
        };
        var f3 = new TextBox
        {
            Text = Log.StringifyCollection(fm)
        };

        var statement = new DataGridCell
        {
            Content = f1
        };

        var reason = new DataGridCell
        {
            Content = f2
        };

        var from = new DataGridCell
        {
            Content = f3
        };

        return (statement, reason, from);
    }
    
}
