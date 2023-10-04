using Dynamically.Backend;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{
    public static TVertex GetSharedVertexOrThrow(this TSegment s1, TSegment s2) {
        Validate(s1, s2);
        if (!s1.Parts.Intersect(s2.Parts).Any()) throw new ArgumentException($"{s1} and {s2} don't share a vertex");
        return (TVertex)s1.Parts.Intersect(s2.Parts).First();
    }

    public static TVertex? GetSharedVertex(this TSegment s1, TSegment s2)
    {
        Validate(s1, s2);
        return (TVertex?)s1.Parts.Intersect(s2.Parts).FirstOrDefault();
    }
}
