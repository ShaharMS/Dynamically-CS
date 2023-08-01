using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

public class Either<l, r>
{
    public dynamic Value;

    public Either(dynamic v)
    {
        Value = v;
    }

    public bool Is<T>() {
        return Value is T;
    }

    public l L() { return (l)Value; }
	public r R() { return (r)Value; }

    public static implicit operator l(Either<l, r> e) { return (l)(e.Value);}
    public static implicit operator r(Either<l, r> e) { return (r)(e.Value);}
    public static implicit operator Either<l, r>(l v) { return new Either<l, r>(v);}
    public static implicit operator Either<l, r>(r v) { return new Either<l, r>(v);}

}
