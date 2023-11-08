using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class RatioOnSegmentFormula : Formula
{
    SegmentFormula SegmentFormula { get; set; }

    public double Ratio;
    public Point PointOnRatio
    {
        get => new(Ratio * (SegmentFormula.X2 + SegmentFormula.X1), Ratio * (SegmentFormula.Y2 + SegmentFormula.Y1));
    }

    public RatioOnSegmentFormula(SegmentFormula Formula, double ratio) : base()
    {
        SegmentFormula = Formula;
        Formula.OnChange.Add(() => UpdateFollowers());
        this.Ratio = ratio;

    }

    public override Point? GetClosestOnFormula(double x, double y)
    {
        return PointOnRatio;
    }

    public override Point? GetClosestOnFormula(Point point)
    {
        return PointOnRatio;
    }

    public override double[] SolveForX(double y)
    {
        var p = PointOnRatio; // Prevent using getter twice;
        if (y != p.Y) return Array.Empty<double>();
        return new double[1] { p.X };
    }

    public override double[] SolveForY(double x)
    {
        var p = PointOnRatio; // Prevent using getter twice;
        if (x != p.X) return Array.Empty<double>();
        return new double[1] { p.Y };
    }

    public override void Move(double x, double y)
    {
        SegmentFormula.Move(x, y);
    }

    public RayFormula GetPerpendicular()
    {
        return new RayFormula(PointOnRatio, -1 / SegmentFormula.Slope);
    }
}
