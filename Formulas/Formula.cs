using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Formulas;

public interface Formula
{
    public double[] SolveForX(double y);
    public double[] SolveForY(double x);

    public Point? GetClosestOnFormula(double x, double y);
    public Point? GetClosestOnFormula(Point point);
}

