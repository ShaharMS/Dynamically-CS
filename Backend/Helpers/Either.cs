using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

public class Either<L, R>
{
    public dynamic Value;

    public Either(dynamic v)
    {
        Value = v;
    }

    public static implicit operator L(Either<L, R> e) { return (L)(e.Value);}
    public static implicit operator R(Either<L, R> e) { return (R)(e.Value);}
    public static implicit operator Either<L, R>(L v) { return new Either<L, R>(v);}
    public static implicit operator Either<L, R>(R v) { return new Either<L, R>(v);}

}
