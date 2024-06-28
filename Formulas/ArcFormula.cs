namespace Dynamically.Formulas;

using Dynamically.Backend;
using Dynamically.Shapes;

public class ArcFormula : CircleFormula
{
    public double StartRadians { get; set; }
    public double StartDegrees { get => StartRadians.ToDegrees(); set => StartRadians = value.ToRadians(); }
    public double EndRadians { get; set; }
    public double EndDegrees { get => EndRadians.ToDegrees(); set => EndRadians = value.ToRadians(); }

    public ArcFormula(double radius, double centerX, double centerY) : base(radius, centerX, centerY) { }

    public ArcFormula(Arc arc) : this(arc.Radius, arc.Center.X, arc.Center.Y)
    {
        StartDegrees = arc.StartDegrees;
        EndDegrees = arc.EndDegrees;
    }
}