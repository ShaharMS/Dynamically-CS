using Dynamically.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TValue : ExerciseToken
{
    public Either<string, double> Value { get; private set; }

    public TValueKind Kind { get; private set; }

    public TValue(TValueKind kind)
    {
        Value = 0;
        Kind = kind;
    }

    public TValue(double value, TValueKind kind) : this(kind)
    {
        Value = value;
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
