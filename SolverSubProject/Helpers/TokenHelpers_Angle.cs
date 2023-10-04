using Dynamically.Backend;
using Dynamically.Solver.Information.BuildingBlocks;
using HonkSharp.Fluency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{

    public static TAngle GetAngle(this TVertex Center, TVertex v1, TVertex v2)
    {
        Validate(Center, v1, v2);
        if (!Center.ParentPool.Elements.Any(x => x is TAngle angle && angle.Parts.ContainsMany(Center, v1, v2)))
        {
            return new TAngle(Center, v1, v2)
            {
                ParentPool = Center.ParentPool
            };
        }
        return (TAngle)Center.ParentPool.Elements.Where(x => x is TAngle angle && angle.Parts.ContainsMany(Center, v1, v2)).First();
    }

    public static bool HasAdjacentAngles(this TAngle angle) => angle.Segment1?.AllMounts.Where(x => x is TVertex).Count() > 0 || angle.Segment2?.AllMounts.Where(x => x is TVertex).Count() > 0;

    public static TAngle[] GetAdjacentAngles(this TAngle angle)
    {
        if (!HasAdjacentAngles(angle)) return Array.Empty<TAngle>();

        var p1 = angle.Segment1?.MountsBeforeFirstOrAfterLast(angle.Other1).FirstOrDefault();
        var o1 = angle.Parts.ToList().RemoveMany(angle.Origin, angle.Other1).First();
        var p2 = angle.Segment2?.MountsBeforeFirstOrAfterLast(angle.Other2).FirstOrDefault();
        var o2 = angle.Parts.ToList().RemoveMany(angle.Origin, angle.Other2).First();

        return new[]
        {
            p1 != null ? GetAngle(angle.Origin, p1, (TVertex)o1) : null,
            p2 != null ? GetAngle(angle.Origin, p2, (TVertex)o2) : null,
        }.Where(x => x != null).Cast<TAngle>().ToArray();
    }

    public static bool HasVertexAngle(this TAngle angle) => angle.Segment1?.MountsBeforeFirstOrAfterLast(angle.Other1).Count > 0 && angle.Segment2?.MountsBeforeFirstOrAfterLast(angle.Other2).Count > 0;

    public static TAngle? GetVertexAngle(this TAngle angle)
    {
        if (angle.Segment1 == null || angle.Segment2 == null) return null;

        try
        {
            return GetAngle(
                        angle.Origin,
                        angle.Segment1.MountsBeforeFirstOrAfterLast(angle.Other1).First(),
                        angle.Segment2.MountsBeforeFirstOrAfterLast(angle.Other2).First()
                    );
        } catch (InvalidOperationException)
        {
            return null;
        }


    }
}
