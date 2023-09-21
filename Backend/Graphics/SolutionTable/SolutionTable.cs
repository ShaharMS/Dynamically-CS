using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using CSharpMath.Avalonia;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class SolutionTable : DraggableGraphic
{
    public List<TableRow> Rows;
    public bool hasFroms;
    public SolutionTable(bool hasFroms = false) : base()
    {
        this.hasFroms = hasFroms;

        Background = new SolidColorBrush(Colors.Red);

    }

    public void MoveRow(int from, int toBefore)
    {
        throw new NotImplementedException();
    }
}
