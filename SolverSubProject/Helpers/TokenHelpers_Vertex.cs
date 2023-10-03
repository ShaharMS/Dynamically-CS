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
    public static TSegment GetOrCreateSegment(this TVertex from, TVertex to) {
        Validate(from, to);
        var item = from.Segments.Where(x => x.Parts.ContainsMany(from, to));
        if (item.Any()) return item.First();
        return new TSegment(from, to) {
            IsAuxiliary = true,
            ParentPool = from.ParentPool
        };
    }
}
