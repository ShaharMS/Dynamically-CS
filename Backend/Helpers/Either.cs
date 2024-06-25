using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

public class Either<l, m, r>
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

    public m M() { return (m)Value; }
	public r R() { return (r)Value; }

    public static implicit operator l(Either<l, m, r> e) { return (l)(e.Value);}
    public static implicit operator m(Either<l, m, r> e) { return (m)(e.Value);}
    public static implicit operator r(Either<l, m, r> e) { return (r)(e.Value);}
#pragma warning disable CS8604
    public static implicit operator Either<l, m, r>(l v) { return new Either<l, m, r>(v);}
    public static implicit operator Either<l, m, r>(m v) { return new Either<l, m, r>(v);}
    public static implicit operator Either<l, m, r>(r v) { return new Either<l, m, r>(v);}
#pragma warning restore CS8604
}
