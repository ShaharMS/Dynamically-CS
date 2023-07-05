using Avalonia;
using Mathos;
using Mathos.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Formulas;

public interface Formula
{
    public double[] SolveForX(double y);
    public double[] SolveForY(double x);

    public Point? GetClosestOnFormula(double x, double y);
}

public class ChangeListener
{
    public List<Action<double, double, double, double>> onMove = new List<Action<double, double, double, double>>();
    public List<Action> onChange = new List<Action>();
}