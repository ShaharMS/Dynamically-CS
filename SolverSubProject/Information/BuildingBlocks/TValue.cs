using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TValue : ExerciseToken
{
    /// <summary>
    /// Contains a latex representation of the value. usable via AngouriMath
    /// </summary>
    public string Value { get; private set; }

    public TValueKind Kind { get; private set; }

    public TValue(TValueKind kind)
    {
        Value = "";
        Kind = kind;
    }

    public TValue(double value, TValueKind kind) : this(kind)
    {
        Value = value + "";
    }

    public TValue(string value, TValueKind kind) : this(kind)
    {
        Value = value;
    }

}
public enum TValueKind
{
    Length,
    Degrees,
    Radians,
    Parameter
}
