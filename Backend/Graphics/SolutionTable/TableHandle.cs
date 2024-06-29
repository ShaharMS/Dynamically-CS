using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Dynamically.Backend.Helpers;
using Dynamically.Design;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class TableHandle : DraggableGraphic
{
    public SolutionTable Table;
    
    readonly Label label;
    readonly Border border;

    readonly Image image;

    public new double Width {
        get => image.Bounds.Width;
        set => image.Width = value;
    }
    public new double Height {
        get => image.Bounds.Height;
        set => image.Height = value;
    }
    public TableHandle(SolutionTable table) : base(table.ParentBoard) {
        Table = table;
        image = new Image
        {
            Source = new Bitmap("Assets/Light/Geometry/SolutionTable/handle.png"),
        };
        Children.Add(image);

        OnMoved.Add((x, y, _, _) =>
        {
            Table.SetPosition(x + image.Bounds.Width / 2 - Table.Width / 2, y + 50);
            foreach(var row in Table.Rows) row.RepositionHandle();
        });
    }

    public override double Area() => 0;
    public override void Render(DrawingContext context) {}
}
