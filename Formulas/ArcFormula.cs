namespace Dynamically.Formulas;

using Avalonia;
using Dynamically.Backend;
using Dynamically.Shapes;
using System;
using System.Linq;

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

    public override double[] SolveForX(double y)
    {
        return (from solution in base.SolveForX(y)
               where (CenterX, CenterY).DegreesTo(solution, y) > StartDegrees && (CenterX, CenterY).DegreesTo(solution, y) < EndDegrees
               select solution).ToArray();
    }

    public override double[] SolveForY(double x)
    {
        return (from solution in base.SolveForY(x)
               where (CenterX, CenterY).DegreesTo(x, solution) > StartDegrees && (CenterX, CenterY).DegreesTo(x, solution) < EndDegrees
               select solution).ToArray();
    }

    public override Point? GetClosestOnFormula(double px, double py)
    {
        var solution = base.GetClosestOnFormula(px, py);
        if (solution != null && 
            (CenterX, CenterY).DegreesTo(solution.Value.X, solution.Value.Y) < StartDegrees && 
            (CenterX, CenterY).DegreesTo(solution.Value.X, solution.Value.Y) > EndDegrees)
        {
            var deg = (CenterX, CenterY).DegreesTo(solution.Value.X, solution.Value.Y);
            var startDiff = Math.Abs(StartDegrees - deg);
            var endDiff = Math.Abs(EndDegrees - deg);
            if (startDiff < endDiff) return new Point(CenterX + Radius * Math.Cos(StartRadians), CenterY + Radius * Math.Sin(StartRadians));
            return new Point(CenterX + Radius * Math.Cos(EndRadians), CenterY + Radius * Math.Sin(EndRadians));
        }

        return null;
    }
    public override Point? GetClosestOnFormula(Point p) => GetClosestOnFormula(p.X, p.Y);
}