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
    public static TAngle GetOppositeAngle(this TQuad quad, TAngle angle)
    {
        Validate(quad, angle);
        if (angle == quad.V1V2V3) return quad.V1V4V3;
        if (angle == quad.V1V4V3) return quad.V1V2V3;
        if (angle == quad.V2V3V4) return quad.V2V1V4;
        if (angle == quad.V2V1V4) return quad.V2V3V4;
        throw new ArgumentException("provided angle must be one of the quad's angles", nameof(angle));
    }

    public static TSegment[] GetMidSegments(this TQuad quad)
    {
        return quad.ParentPool.AvailableDetails.GetMany(Relation.MIDSEGMENT, quad).Select(x => x.Left).Cast<TSegment>().ToArray();
    }

    public static (TSegment midSegment, TSegment[] opposites)[] GetMidSegmentsWithOpposites(this TQuad quad)
    {
        return quad.ParentPool.AvailableDetails.GetMany(Relation.MIDSEGMENT, quad).Select(x => (x.Left, quad.Sides.Except(x.SideProducts).ToArray())).Cast<(TSegment midSegment, TSegment[] opposites)>().ToArray();
    }

    public static TSegment[] GetTrapezoidBases(this TQuad trapezoid)
    {
        return trapezoid.ParentPool.AvailableDetails.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID).SideProducts.Cast<TSegment>().ToArray();
    }
    public static TSegment[] GetTrapezoidSides(this TQuad trapezoid)
    {
        return trapezoid.Sides.Except(trapezoid.ParentPool.AvailableDetails.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID).SideProducts.Cast<TSegment>()).ToArray();
    }
}
