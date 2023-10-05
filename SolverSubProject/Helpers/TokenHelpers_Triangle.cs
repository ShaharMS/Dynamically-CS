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
    public static TAngle GetOppositeAngle(this TTriangle triangle, TSegment segment)
    {
        if (segment == triangle.V1V2) return triangle.V1V3V2;
        if (segment == triangle.V1V3) return triangle.V1V2V3;
        if (segment == triangle.V2V3) return triangle.V2V1V3;
        throw new ArgumentException("Segment provided must be one of the triangle's sides", nameof(segment));
    }

    public static TAngle GetIsoscelesHeadAngle(this TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Details.Relation.TRIANGLE_ISOSCELES)) 
            throw new ArgumentException("Provided triangle must be an isosceles triangle", nameof(triangle));

        var detail = triangle.ParentPool.AvailableDetails.EnsuredGet(triangle, Details.Relation.TRIANGLE_ISOSCELES);

        return ((TSegment)detail.SideProducts[0]).GetSharedAngleOrThrow((TSegment)detail.SideProducts[1]);
    }

    public static TSegment GetIsoscelesBaseSide(this TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Details.Relation.TRIANGLE_ISOSCELES))
            throw new ArgumentException("Provided triangle must be an isosceles triangle", nameof(triangle));

        var detail = triangle.ParentPool.AvailableDetails.EnsuredGet(triangle, Details.Relation.TRIANGLE_ISOSCELES);
        
        return new[] { triangle.V1V2, triangle.V2V3, triangle.V1V3}.Except(detail.SideProducts.Cast<TSegment>()).First();
    }
}
