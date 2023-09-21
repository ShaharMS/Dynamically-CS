using CSharpMath.Avalonia;
using CSharpMath.Editor;
using Dynamically.Backend.Helpers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableRow : HDock
{   
    private MathView _statement;
    public string Statement {
        get => _statement.LaTeX ?? "";
        set => _statement.LaTeX = value;
    }

    public string Reason;

    public HashSet<int> From = new();

    public TableRow(string statementLatex, string reason, IEnumerable<int> from)
    {
        Statement = statementLatex;
        Reason = reason;
        From = from.ToHashSet();

    }

}
