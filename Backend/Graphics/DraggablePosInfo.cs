using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public partial class DraggableGraphic
{
    public virtual bool Overlaps(Point point)
    {
        var pos = this.GetPosition();
        return pos.X < point.X && pos.Y < point.Y && pos.X + Width > point.X && pos.Y + Height > point.Y;
    }

    public virtual double Area()
    {
        return Width * Height;
    }
}
