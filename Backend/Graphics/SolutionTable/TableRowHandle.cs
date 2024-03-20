using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CSharpMath.Atom.Atoms;
using Dynamically.Design;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableRowHandle : DraggableGraphic
{
    public SolutionTable Table
    {
        get => Row.Table;
    }

    public TableRow Row;
    public int Serial {
        get => Table.Rows.IndexOf(Row) + 1;
        set {
            Table.MoveRow(Row, value - 1);
            label.Content = $" {value} ";
        }
    }

    private readonly Label label;
    private readonly Image image;
    public new double Width {
        get => image.Bounds.Width;
        set => image.Width = value;
    }
    public new double Height {
        get => image.Bounds.Height;
        set => image.Height = value;
    }
    public TableRowHandle(TableRow row) : base(row.Table.ParentBoard) {
        Row = row;
        label = new Label
        {
            FontSize = 12,
            Foreground = UIColors.SolutionTableBorder,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Height = 20
        };
        image = new Image {
            Source = new Bitmap("Assets/Light/Geometry/SolutionTable/empty_handle.png"),
        };
        image.SetPosition(0, 0);
        Children.Add(image);

        OnMoved.Add((_, _ , _, _) => {
            Row.AttemptMovement();
            foreach (TableRow row in Table.Rows) row.RepositionHandle();
        });
        OnDragged.Add((_, _, _, _) => {
            Row.RepositionHandle();
            Table.Refresh();
        });
    }

    public override double Area() => 0;
    public override void Render(DrawingContext context) {}

    public void Refresh() {
        Serial = Serial;
    }
}
