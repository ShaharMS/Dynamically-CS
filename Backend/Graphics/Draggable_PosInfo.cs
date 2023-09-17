using Avalonia;
using Avalonia.Rendering.SceneGraph;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public partial class DraggableGraphic
{

    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<DraggableGraphic, double>(nameof(X));
    public virtual double X
    {
        get => GetValue(XProperty);
        set
        {
            SetLeft(this, value);
            SetValue(XProperty, value);
        }
    }

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<DraggableGraphic, double>(nameof(Y));

    public virtual double Y
    {
        get => GetValue(YProperty);
        set
        {
            SetTop(this, value);
            SetValue(YProperty, value);
        }
    }

    public double ScreenX
    {
        get => this.GetPosition().X;
    }
    public double ScreenY
    { 
        get => this.GetPosition().Y; 
    }
    public virtual bool Overlaps(Point point)
    {
        return VisualTree.HitTest(this, point) != null;
    }

    public virtual double Area()
    {
        return Width * Height;
    }

    public virtual double GetClosenessToCenter(Point point)
    {
        return point.DistanceTo(ScreenX, ScreenY);
    }
}
