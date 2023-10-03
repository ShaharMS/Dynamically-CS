using Dynamically.Backend;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{

    public static TAngle GetAngle(this TVertex Center, TVertex v1, TVertex v2) {
        Validate(Center, v1, v2);
        if (!Center.ParentPool.Elements.Any(x => x is TAngle angle && angle.Parts.ContainsMany(Center, v1, v2))) {
            return new TAngle(Center, v1, v2) {
                ParentPool = Center.ParentPool
            };
        }
        return (TAngle)Center.ParentPool.Elements.Where(x => x is TAngle angle && angle.Parts.ContainsMany(Center, v1, v2)).First();
    }

    public static bool HasAdjacentAngles(this TAngle angle) => angle.Segment1?.AllMounts.Where(x => x is TVertex).Count() > 0 || angle.Segment2?.AllMounts.Where(x => x is TVertex).Count() > 0;

    public static TAngle[] GetAdjacentAngles(this TAngle angle)
    {
        if (!HasAdjacentAngles(angle)) return Array.Empty<TAngle>();

        return Array.Empty<TAngle>();
    }

    public static bool HasVertexAngle(this TAngle angle) => angle.Segment1?.AllMounts.Where(x => x is TVertex).Count() > 0 && angle.Segment2?.AllMounts.Where(x => x is TVertex).Count() > 0;

}
