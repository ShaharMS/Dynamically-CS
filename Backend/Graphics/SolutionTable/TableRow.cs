using Dynamically.Backend.Helpers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableRow : HDock
{
    public string Statement;

    public string Reason;

    public HashSet<int> From = new();

    public TableRow(string statementLatex, string reason, IEnumerable<int> from)
    {
        Statement = statementLatex;
        Reason = reason;
        From = from.ToHashSet();
    }

}
