using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using HonkSharp.Fluency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{
    public static TVertex[] GetVertices(this TTriangle triangle)
    {
        return new[] { triangle.V1, triangle.V2, triangle.V3 };
    }

    public static TSegment[] GetSegments(this TTriangle triangle)
    {
        return new[] { triangle.V1V2, triangle.V1V3, triangle.V2V3 };
    }

    public static TAngle[] GetAngles(this TTriangle triangle)
    {
        return new[] { triangle.V2V1V3, triangle.V1V2V3, triangle.V1V3V2 };
    }

    public static TAngle GetOppositeAngle(this TTriangle triangle, TSegment segment)
    {
        if (segment == triangle.V1V2) return triangle.V1V3V2;
        if (segment == triangle.V1V3) return triangle.V1V2V3;
        if (segment == triangle.V2V3) return triangle.V2V1V3;
        throw new ArgumentException("provided segment must be one of the triangle's sides", nameof(segment));
    }

    public static TSegment GetOppositeSegment(this TTriangle triangle, TVertex vertex)
    {
        if (vertex == triangle.V1) return triangle.V2V3;
        if (vertex == triangle.V2) return triangle.V1V3;
        if (vertex == triangle.V3) return triangle.V1V2;
        throw new ArgumentException("provided vertex must be one of the triangle's vertices", nameof(vertex));
    }

    public static TSegment GetOppositeSegment(this TTriangle triangle, TAngle angle) => GetOppositeSegment(triangle, angle.Origin);

    public static TVertex GetOppositeVertex(this TTriangle triangle, TSegment segment)
    {
        if (segment == triangle.V1V2) return triangle.V3;
        if (segment == triangle.V1V3) return triangle.V2;
        if (segment == triangle.V2V3) return triangle.V1;
        throw new ArgumentException("provided segment must be one of the triangle's sides", nameof(segment));
    }

    public static TAngle GetAngleOf(this TTriangle triangle, TVertex vertex)
    {
        if (vertex == triangle.V1) return triangle.V2V1V3;
        if (vertex == triangle.V2) return triangle.V1V2V3;
        if (vertex == triangle.V3) return triangle.V1V3V2;
        throw new ArgumentException("provided vertex must be one of the triangle's vertices", nameof(vertex));
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

    public static TSegment[] GetMidSegments(this TTriangle triangle) {
        return triangle.ParentPool.AvailableDetails.GetMany(Relation.MIDSEGMENT, triangle).Select(x => x.Left).Cast<TSegment>().ToArray();
    }

    public static (TSegment midSegment, TSegment opposite)[] GetMidSegmentsWithOpposites(this TTriangle triangle) {
        return triangle.ParentPool.AvailableDetails.GetMany(Relation.MIDSEGMENT, triangle).Select(x => (x.Left, x.SideProducts[0])).Cast<(TSegment midSegment, TSegment opposite)>().ToArray();
    }

    public static TCircle GetIncircle(this TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(Relation.INCIRCLE, triangle))
            throw new ArgumentException("Provided triangle must have an incircle", nameof(triangle));
        return (TCircle)triangle.ParentPool.AvailableDetails.EnsuredGet(Relation.INCIRCLE, triangle).Left;
    }

    public static TCircle GetCircumcircle(this TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(Relation.CIRCUMCIRCLE, triangle))
            throw new ArgumentException("Provided triangle must have a circumcircle", nameof(triangle));
        return (TCircle)triangle.ParentPool.AvailableDetails.EnsuredGet(Relation.CIRCUMCIRCLE, triangle).Left;
    }
