﻿using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static class DetailQuickCreator
{
    public static Detail Connect(this TVertex v1, TVertex v2) => new(v1, Relation.CONNECTED, v2);
    public static Detail On(this TVertex v1, TSegment s1) => new(v1, Relation.ON, s1);
    public static Detail Middle(this TVertex v1, TSegment s1) => new(v1, Relation.MIDDLE, s1);
    public static Detail Tangent(this TSegment s, TCircle c) => new(s, Relation.TANGENT, c);
    public static Detail Tangent(this TSegment s, TCircle c, TVertex contact) => new(s, Relation.TANGENT, c, contact);
    public static Detail Parallel(this TSegment s1, TSegment s2) => new(s1, Relation.PARALLEL, s2);
    public static Detail Perpendicular(this TSegment s1, TSegment s2) => new(s1, Relation.PERPENDICULAR, s2);
    public static Detail Perpendicular(this TSegment s1, TSegment s2, TVertex c) => new(s1, Relation.PERPENDICULAR, s2, c);
    public static Detail Intersects(this TSegment s1, TSegment s2) => new(s1, Relation.INTERSECTS, s2);
    public static Detail Intersects(this TSegment s1, TSegment s2, TVertex c) => new(s1, Relation.INTERSECTS, s2, c);
    public static Detail Bisects(this TSegment s1, TSegment s2) => new(s1, Relation.BISECTS, s2);
    public static Detail Bisects(this TVertex s1, TSegment s2) => new(s1, Relation.BISECTS, s2);
    public static Detail Bisects(this TSegment s1, TSegment s2, TVertex c) => new(s1, Relation.BISECTS, s2, c);

    public static Detail Auxiliary(this ExerciseToken s) => new(s, Relation.AUXILIARY);
    public static Detail Circle(this TVertex v) => new(v, Relation.CIRCLE);
    public static Detail ValueOf(this ExerciseToken s) => new(s, Relation.VALUEOF);

    public static Detail Assign(this ExerciseToken a, ExerciseToken b) => new(a, Relation.ASSIGN, b);

    public static Detail Right(this TAngle a) => new(a, Relation.EQUALS, new TValue(90, TValueKind.Degrees));
    public static Detail Flat(this TAngle a) => new(a, Relation.EQUALS, new TValue(180, TValueKind.Degrees));

    public static Detail EqualsVal(this ExerciseToken a, ExerciseToken b) => new(a, Relation.EQUALS, b);
    public static Detail Different(this TAngle a, TAngle b) => new(a, Relation.NOTEQUALS, b);
    public static Detail Larger(this ExerciseToken a, ExerciseToken b) => new(a, Relation.LARGER, b);
    public static Detail Smaller(this ExerciseToken a, ExerciseToken b) => new(a, Relation.SMALLER, b);
    public static Detail LargerEquals(this ExerciseToken a, ExerciseToken b) => new(a, Relation.EQLARGER, b);
    public static Detail SmallerEquals(this ExerciseToken a, ExerciseToken b) => new(a, Relation.EQSMALLER, b);

    public static Detail AddReferences(this Detail detail, params Detail[] Refs)
    {
        detail.References.AddRange(Refs);
        return detail;
    }


    public static bool Has(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.Any(x => x.Operator == r && x.Left == a && x.Right == b);

    public static Detail? Get(this List<Detail> avaliableDetails, ExerciseToken a, Relation r, ExerciseToken b) => avaliableDetails.FirstOrDefault(x => x.Operator == r && x.Left == a && x.Right == b);

    /// <summary>
    /// NOTICE - Don't use this method without testing with the <c>Has</c> method.
    /// </summary>
    /// <param name="avaliableDetails"></param>
    /// <param name="a"></param>
    /// <param name="r"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Detail EnsuredGet(this List<Detail> avaliableDetails, ExerciseToken a, Relation r, ExerciseToken b) => avaliableDetails.First(x => x.Operator == r && x.Left == a && x.Right == b);
}
