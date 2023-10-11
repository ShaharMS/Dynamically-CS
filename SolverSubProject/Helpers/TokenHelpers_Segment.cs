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
    public static TVertex GetSharedVertexOrThrow(this TSegment s1, TSegment s2)
    {
        Validate(s1, s2);
        if (!s1.Parts.Intersect(s2.Parts).Any()) throw new ArgumentException($"{s1} and {s2} don't share a vertex");
        return (TVertex)s1.Parts.Intersect(s2.Parts).First();
    }

    public static TVertex? GetSharedVertex(this TSegment s1, TSegment s2)
    {
        Validate(s1, s2);
        return (TVertex?)s1.Parts.Intersect(s2.Parts).FirstOrDefault();
    }

    public static TAngle GetSharedAngleOrThrow(this TSegment s1, TSegment s2)
    {
        Validate(s1, s2);
        if (!s1.Parts.Intersect(s2.Parts).Any()) throw new ArgumentException($"{s1} and {s2} don't share an angle");
        if (s1.Parts.Intersect(s2.Parts).Count() > 1) throw new ArgumentException($"{s1} and {s2} share more than one angle");


        var sharedVertex = (TVertex)s1.Parts.Intersect(s2.Parts).First();
        var others = s1.Parts.Except(s2.Parts).Cast<TVertex>().ToList();
        var potentialAngle = s1.ParentPool.Elements.FirstOrDefault(x => x is TAngle angle && angle.Origin == sharedVertex && angle.Parts.ContainsMany(others));

        return (TAngle?)potentialAngle ?? new TAngle(sharedVertex, others[0], others[2]) { ParentPool = s1.ParentPool };
    }

    public static bool HasIntersectionPoint(this TSegment segment, TSegment other)
    {
        Validate(segment, other);

        if (!segment.ParentPool.AvailableDetails.Has(segment, Details.Relation.INTERSECTS, other)) return false;
        var detail = segment.ParentPool.AvailableDetails.EnsuredGet(segment, Details.Relation.INTERSECTS, other);
        return detail.SideProducts.Count == 1;
    }

    public static TVertex GetIntersectionPoint(this TSegment segment, TSegment other) 
    { 
        Validate(segment, other);
        if (!segment.ParentPool.AvailableDetails.Has(segment, Relation.INTERSECTS, other))
        {
            throw new ArgumentException($"{segment} and {other} don't share an intersection point");
        }

        var detail = segment.ParentPool.AvailableDetails.EnsuredGet(segment, Relation.INTERSECTS, other);
        return (TVertex)detail.SideProducts.First();
    }

    public static IEnumerable<TSegment> GetBisectors(this TSegment segment) {
        foreach (var x in segment.ParentPool.AvailableDetails.GetMany(Relation.BISECTS, segment)) 
            if (x.Left is TSegment s) yield return s;
    }

    public static bool IsBisecting(this TSegment segment, ExerciseToken element)
    {
        Validate(segment, element);
        return segment.ParentPool.AvailableDetails.Has(segment, Relation.BISECTS, element);
    }

    public static IEnumerable<TSegment> GetIntersectors(this TSegment segment) {
        foreach (var x in
                segment.ParentPool.AvailableDetails.GetMany((Relation.INTERSECTS, Relation.BISECTS), segment).Concat(
                    segment.ParentPool.AvailableDetails.GetMany(segment, Relation.INTERSECTS, Relation.BISECTS)
                ).ToHashSet()
            ) yield return (TSegment)x.Left;
    }

    public static bool IsIntersecting(this TSegment segment, ExerciseToken element)
    {
        Validate(segment, element);
        return segment.ParentPool.AvailableDetails.Has(segment, Relation.INTERSECTS, element);
    }

    public static IEnumerable<TSegment> GetParallels(this TSegment segment)
    {
        foreach (var x in
                segment.ParentPool.AvailableDetails.GetMany(Relation.PARALLEL, segment).Concat(
                    segment.ParentPool.AvailableDetails.GetMany(segment, Relation.PARALLEL)
                ).ToHashSet()
            ) yield return (TSegment)x.Left;
    }

    public static bool IsParallel(this TSegment segment, TSegment element)
    {
        Validate(segment, element);
        return segment.ParentPool.AvailableDetails.Has(segment, Relation.PARALLEL, element);
    }
}
