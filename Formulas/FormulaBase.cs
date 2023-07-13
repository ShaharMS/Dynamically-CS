using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public class FormulaBase : Formula
{
    public List<Action<double, double, double, double>> OnMove = new List<Action<double, double, double, double>>();
    public List<Action> OnChange = new List<Action>();

    public bool queueRemoval = false;

    public virtual double[] SolveForX(double y)
    {
        Log.Write("Unimplemented SolveForX, returning []");
        return Array.Empty<double>();
    }
    public virtual double[] SolveForY(double x)
    {
        Log.Write("Unimplemented SolveForY, returning []");
        return Array.Empty<double>();
    }

    public virtual Point? GetClosestOnFormula(double x, double y)
    {
        Log.Write("Unimplemented GetClosestOnFormula, returning null");
        return null;
    }
    public virtual Point? GetClosestOnFormula(Point point)
    {
        Log.Write("Unimplemented GetClosestOnFormula, returning null");
        return null;
    }
}
