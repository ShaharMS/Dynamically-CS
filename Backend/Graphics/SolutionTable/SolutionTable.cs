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

public class SolutionTable : DraggableGraphic
{
    public List<TableRow> Rows = new();

    public VDock VisualList;

    public bool HasFroms;

    Border border;

    public SolutionTable(bool hasFroms = false) : base()
    {
        HasFroms = hasFroms;
        VisualList = new VDock
        {
            ChildrenQueued = new List<Control>
            {
                new TableRow(this)
                {
                    VisualRow = new HDock
                    {
                        ChildrenQueued = new List<Control>
                        {
                            new Label { Content = "Statement" },
                            new Label { Content = "Reason" },
                            new Label { Content = "From" }
                        }
                    }
                },new TableRow(this)
                {
                    VisualRow = new HDock
                    {
                        ChildrenQueued = new List<Control>
                        {
                            new Label { Content = "Statement" },
                            new Label { Content = "Reason" },
                            new Label { Content = "From" }
                        }
                    }
                },
            }
        };
        border = new Border
        {
            Child = VisualList,
            BorderThickness = new Thickness(3, 3, 3, 3),
            Background = /*UIColors.SolutionTableFill*/ new SolidColorBrush(Colors.Gray),
            BorderBrush = /*UIColors.SolutionTableBorder*/ new SolidColorBrush(Colors.Red),
        };

        Children.Add(border);
    }

    public override void Render(DrawingContext context)
    {
    }

    public void MoveRow(int from, int toBefore)
    {
        
    }
}
