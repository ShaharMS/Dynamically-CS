using AngouriMath;
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
    public Entity Value { get; private set; }


    public TValue()
    {
        Value = "";
    }

    public TValue(double value)
    {
        Value = value + "";
    }

    public TValue(string value)
    {
        Value = value;
    }

    public bool Equals(TValue? obj)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }


    /// <summary>
    /// TValue > TValue
    /// </summary>
    public static bool operator >(TValue a, TValue b) {
        throw new NotImplementedException();
    }

    /// <summary>
    /// TValue < TValue
    /// </summary>
    public static bool operator <(TValue a, TValue b) {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// TValue >= TValue
    /// </summary>
    public static bool operator >=(TValue a, TValue b) {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// TValue <= TValue
    /// </summary>
    public static bool operator <=(TValue a, TValue b) {
        throw new NotImplementedException();
    }

    public static bool operator ==(TValue a, TValue b) {
        return a.Equals(b);
    }

    public static bool operator !=(TValue a, TValue b) {
        return !a.Equals(b);
    }
}
