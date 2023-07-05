using Avalonia;
using Dynamically.Formulas;
using GeometryBackend;
using GraphicsBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;
public class Circle : EllipseBase
{
    public Joint center;

    public double radius
    {
        get => distanceSum / 2;
        set => distanceSum = value * 2;
    }

    public CircleFormula formula;

    public Circle(Joint center, double radius) : base(center, center, radius * 2)
    {
        this.radius = radius;
        this.center = center;
        formula = new CircleFormula(radius, center.x, center.y);
        this.center.PropertyChanged += center_OnChange;
        onDistanceSumChange = updateFormula;
    }

    public void updateFormula()
    {
        formula.centerX = center.x;
        formula.centerY = center.y;
        formula.radius = radius;
    }
    void center_OnChange(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(center.Margin))
        {
            updateFormula();
        }
    }
}

