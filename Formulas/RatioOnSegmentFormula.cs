using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class RatioOnSegmentFormula : FormulaBase, Formula
{
    SegmentFormula SegmentFormula { get; set; }

    public double ratio;
    public Point pointOnRatio {
        get => new Point(ratio * (SegmentFormula.p2.X - SegmentFormula.p1.X), ratio * (SegmentFormula.p2.Y - SegmentFormula.p1.Y));
    }

    public RatioOnSegmentFormula(SegmentFormula Formula, double ratio)
    {
        SegmentFormula = Formula;
        this.ratio = ratio;

    }

    public override Point? GetClosestOnFormula(double x, double y)
    {
        return pointOnRatio;
    }

    public override Point? GetClosestOnFormula(Point point)
    {
        return pointOnRatio;
    }

    public override double[] SolveForX(double y)
    {
        var p = pointOnRatio; // Prevent using getter twice;
        if (y != p.Y) return Array.Empty<double>();
        return new double[1] { p.X };
    }

    public override double[] SolveForY(double x)
    {
        var p = pointOnRatio; // Prevent using getter twice;
        if (x != p.X) return Array.Empty<double>();
        return new double[1] { p.Y };
    }
}
