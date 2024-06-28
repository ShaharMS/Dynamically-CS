using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Arc : IHasFormula<CircleFormula>
{
    public CircleFormula Formula { get; set; }

    public bool Contains(Vertex joint)
    {
        return Center == joint || StartEdge == joint || EndEdge == joint;
    }

    public bool Contains(Segment segment)
    {
        return false;
    }

    public bool HasMounted(Vertex joint)
    {
        return false;
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public override double Area()
    {
        return Radius * Radius * Math.PI * (TotalDegrees / 360);
    }
}