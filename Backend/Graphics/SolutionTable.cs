using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CSharpMath.Avalonia;

namespace Dynamically.Backend.Graphics;

public class SolutionTable : DataGrid
{
    public bool hasFroms;

    public DataGridTemplateColumn Statements;

    public DataGridTemplateColumn Reasons;
    
    public DataGridTemplateColumn Froms;
    public SolutionTable(bool hasFroms = false) : base()
    {
        this.hasFroms = hasFroms;

        CanUserResizeColumns = true;
        CanUserSortColumns = false;
        CanUserReorderColumns = false;



        Statements = new DataGridTemplateColumn {
            Header = "Statements"
        };
        Reasons = new DataGridTemplateColumn {
            Header = "Reasons"
        };
        Froms = new DataGridTemplateColumn {
            Header = "From"
        };

        Columns.Add(Statements); Columns.Add(Reasons);
        if (hasFroms) Columns.Add(Froms);

    }

    public (DataGridCell statement, DataGridCell reason, DataGridCell from) GenericAdd(string state, string res, IEnumerable<int> fm)
    {
        var field = new MathView
        {
            LaTeX = state,
            
        };

        return (null, null, null);
    }
    
}
