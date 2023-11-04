using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{
    public static (TSegment, Detail?) GetOrCreateSegment(this TVertex from, TVertex to) {
        Validate(from, to);
        var item = from.Segments.Where(x => x.Parts.ContainsMany(from, to));
        if (item.Any()) return (item.First(), null);

        var seg = new TSegment(from, to)
        {
            IsAuxiliary = true,
            ParentPool = from.ParentPool
        }; 
        var detail = seg.MarkAuxiliary();
        seg.ParentPool.AddDetail(detail);
        return (seg, detail);
    }

    public static IEnumerable<TAngle> GetAngles(this TVertex vertex)
    {
        return vertex.ParentPool.Elements.Where(x => x is TAngle angle && angle.Origin == vertex).Cast<TAngle>();
    }
}
