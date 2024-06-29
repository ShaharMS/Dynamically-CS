using Avalonia;
using Avalonia.Controls;
using Dynamically.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public partial class ResizableBorder : Border
{
    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<ResizableBorder, double>(nameof(X));
    public virtual double X
    {
        get => GetValue(XProperty);
        set
        {
            Canvas.SetLeft(this, value);
            SetValue(XProperty, value);
        }
    }

    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<ResizableBorder, double>(nameof(Y));

    public virtual double Y
    {
        get => GetValue(YProperty);
        set
        {
            Canvas.SetTop(this, value);
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
}
