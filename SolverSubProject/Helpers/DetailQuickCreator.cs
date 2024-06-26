﻿using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using HonkSharp.Functional;
using SolverSubProject.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dynamically.Solver.Helpers;

public static class DetailQuickCreator
{
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
    public static Detail Bisects(this TSegment s, TAngle a) => new(s, Relation.BISECTS, a);
    public static Detail MidSegment(this TSegment s, TTriangle t, TSegment s1) => new(s, Relation.MIDSEGMENT, t, s1);
    public static Detail MidSegment(this TSegment s, TQuad q, TSegment s1, TSegment s2) => new(s, Relation.MIDSEGMENT, q, s1, s2);
    public static Detail BisectorIntersection(this TVertex v, TTriangle t) => new(v, Relation.BISECTOR_INTERSECTION, t);
    public static Detail AngleBisectorIntersection(this TVertex v, TTriangle t) => new(v, Relation.ANGLEBISECTOR_INTERSECTION, t);

    public static Detail Incircle(this TCircle c, TTriangle t) => new(c, Relation.INCIRCLE, t);
    public static Detail Incircle(this TCircle c, TQuad q) => new(c, Relation.INCIRCLE, q);
    public static Detail Circumcircle(this TCircle c, TTriangle t) => new(c, Relation.CIRCUMCIRCLE, t);
    public static Detail Circumcircle(this TCircle c, TQuad q) => new(c, Relation.CIRCUMCIRCLE, q);

    public static Detail CircleCenter(this TVertex v, TCircle c) => new(v, Relation.CIRCLE_CENTER, c);

    public static Detail Right(this TAngle a) => new(a, Relation.EQUALS, new TValue(90));
    public static Detail Flat(this TAngle a) => new(a, Relation.EQUALS, new TValue(180));

    public static Detail Congruent(this TTriangle t1, TTriangle t2, (TSegment, TSegment) sides1, (TAngle, TAngle) angles, (TSegment, TSegment) sides2) => new(t1, Relation.TRIANGLE_CONGRUENCY_S_A_S, t2, sides1.Item1, sides1.Item2, angles.Item1, angles.Item2, sides2.Item1, sides2.Item2);
    public static Detail Congruent(this TTriangle t1, TTriangle t2, (TAngle, TAngle) angles1, (TSegment, TSegment) sides, (TAngle, TAngle) angles2) => new(t1, Relation.TRIANGLE_CONGRUENCY_A_S_A, t2, angles1.Item1, angles1.Item2, sides.Item1, sides.Item2, angles2.Item1, angles2.Item2);
    public static Detail Congruent(this TTriangle t1, TTriangle t2, (TSegment, TSegment) sides1, (TSegment, TSegment) sides2, (TSegment, TSegment) sides3) => new(t1, Relation.TRIANGLE_CONGRUENCY_S_S_S, t2, sides1.Item1, sides1.Item2, sides2.Item1, sides2.Item2, sides3.Item1, sides3.Item2);
    public static Detail Congruent(this TTriangle t1, TTriangle t2, (TSegment, TSegment) sides1, (TSegment, TSegment) sides2, (TAngle, TAngle) angles) => new(t1, Relation.TRIANGLE_CONGRUENCY_S_S_A, t2, sides1.Item1, sides1.Item2, sides2.Item1, sides2.Item2, angles.Item1, angles.Item2);

    public static Detail MarkAuxiliary(this ExerciseToken s) => new Detail(s, Relation.NEW) { DefinesAuxiliary = true };


    public static Detail MarkIsosceles(this TTriangle triangle, ValueTuple<TSegment, TSegment> equals) => new(triangle, Relation.TRIANGLE_ISOSCELES) { SideProducts = new() { equals.Item1, equals.Item2} };

    public static Detail MarkParallelogram(this TQuad quad) => new(quad, Relation.QUAD_PARALLELOGRAM) { SideProducts = new() {quad.V1V2, quad.V3V4, quad.V2V3, quad.V1V4} };
    public static Detail MarkRhombus(this TQuad quad) => new(quad, Relation.QUAD_RHOMBUS);
    public static Detail MarkRectangle(this TQuad quad) => new(quad, Relation.QUAD_RECTANGLE) { SideProducts = new() {quad.V1V2, quad.V3V4, quad.V2V3, quad.V1V4} };
    public static Detail MarkTrapezoid(this TQuad quad, ValueTuple<TSegment, TSegment> parallels) => new(quad, Relation.QUAD_TRAPEZOID) { SideProducts = parallels.ToArray<ExerciseToken>().ToList()};
    public static Detail MarkSquare(this TQuad quad) => new(quad, Relation.QUAD_SQUARE);
    public static Detail MarkKite(this TQuad quad, TAngle headAngle) => new(quad, Relation.QUAD_KITE) { SideProducts = new() { headAngle, quad.GetOppositeAngle(headAngle)} };
    public static Detail MarkIsoscelesTrapezoid(this TQuad quad, ValueTuple<TSegment, TSegment> parallels) => new(quad, Relation.QUAD_ISOSCELES_TRAPEZOID) { SideProducts = parallels.ToArray<ExerciseToken>().Concat(quad.Sides.Except(parallels.ToArray<ExerciseToken>()).ToArray<ExerciseToken>()).ToList() };



    public static Detail EqualsVal(this TValue a, TValue b) => new(a, Relation.EQUALS, b);
    public static Detail EqualsVal(this ExerciseToken a, ExerciseToken b) => new(new TValue(a.Id), Relation.EQUALS, new TValue(b.Id));
    public static Detail Different(this TAngle a, TAngle b) => new(a, Relation.NOTEQUALS, b);
    public static Detail Larger(this ExerciseToken a, ExerciseToken b) => new(a, Relation.LARGER, b);
    public static Detail Smaller(this ExerciseToken a, ExerciseToken b) => new(a, Relation.SMALLER, b);
    public static Detail LargerEquals(this ExerciseToken a, ExerciseToken b) => new(a, Relation.EQLARGER, b);
    public static Detail SmallerEquals(this ExerciseToken a, ExerciseToken b) => new(a, Relation.EQSMALLER, b);

    public static Detail AddReferences(this Detail detail, params Detail?[] Refs)
    {
        detail.References.AddRange(Refs.Where(x => x != null).Cast<Detail>());
        foreach (var item in Refs)
        {
            if (item == null) continue;
            item.ReferencedBy.Add(detail);
        }
        return detail;
    }

    public static Detail AddReferences(this Detail detail, IEnumerable<Detail> Refs) => AddReferences(detail, Refs.ToArray());

    public static Detail AddSideProducts(this Detail detail, params ExerciseToken[] SideProducts) {
        detail.SideProducts.AddRange(SideProducts);
        return detail;
    }

    public static Detail MarkReasonExplicit(this Detail s, Reason reason) { s.Reason = reason; return s; }


    public static bool Has(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.Any(x => x.Operator == r && x.Left == a && x.Right == b);
    public static bool Has(this List<Detail> availableDetails, ExerciseToken a, Relation r) => availableDetails.Any(x => x.Operator == r && x.Left == a);
    public static bool Has(this List<Detail> availableDetails, Relation r, ExerciseToken b) => availableDetails.Any(x => x.Operator == r && x.Right == b);

    public static bool UnorderedHas(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.Any(x => x.Operator == r && ((x.Left == a && x.Right == b) || (x.Left == b && x.Right == a)));
    public static bool UnorderedHas(this List<Detail> availableDetails, ExerciseToken a, Relation r) => availableDetails.Any(x => x.Operator == r && (x.Left == a || x.Right == a));

    public static Detail? Get(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.FirstOrDefault(x => x.Operator == r && x.Left == a && x.Right == b);
    public static Detail? Get(this List<Detail> availableDetails, ExerciseToken a, Relation r) => availableDetails.FirstOrDefault(x => x.Operator == r && x.Left == a);
    public static Detail? Get(this List<Detail> availableDetails, Relation r, ExerciseToken b) => availableDetails.FirstOrDefault(x => x.Operator == r && x.Right == b);


    public static Detail[] GetMany(this List<Detail> availableDetails, ExerciseToken a, Relation r) => availableDetails.Where(x => x.Operator == r && x.Left == a).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, Relation r, ExerciseToken b) => availableDetails.Where(x => x.Operator == r && x.Right == b).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, Relation r, params ExerciseToken[] b) => availableDetails.Where(x => x.Operator == r && b.Contains(x.Right)).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, ITuple r, ExerciseToken b) => availableDetails.Where(x => r.ToArray<Relation>().Contains(x.Operator) && x.Right == b).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, ExerciseToken a) => availableDetails.Where(x => x.Left == a).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, ExerciseToken a, params Relation[] rs) => availableDetails.Where(x => rs.Contains(x.Operator) && x.Left == a).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, ExerciseToken a, Relation r, params ExerciseToken[] bs) => availableDetails.Where(x => x.Operator == r && x.Left == a && bs.Contains(x.Right)).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, ITuple a, Relation r) => availableDetails.Where(x => x.Operator == r && a.ToArray<ExerciseToken>().Contains(x.Left)).ToArray();
    public static Detail[] GetMany(this List<Detail> availableDetails, params Relation[] rs) => availableDetails.Where(x => rs.Contains(x.Operator)).ToArray();

    /// <summary>
    /// NOTICE - Don't use this method without testing with the <c>Has</c> method.
    /// </summary>
    /// <param name="availableDetails"></param>
    /// <param name="a"></param>
    /// <param name="r"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Detail EnsuredGet(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.First(x => x.Operator == r && x.Left == a && x.Right == b);

    /// <summary>
    /// NOTICE - Don't use this method without testing with the <c>Has</c> method.
    /// </summary>
    /// <param name="availableDetails"></param>
    /// <param name="a"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    /// 
    public static Detail EnsuredGet(this List<Detail> availableDetails, ExerciseToken a, Relation r) => availableDetails.First(x => x.Operator == r && x.Left == a);
    /// <summary>
    /// NOTICE - Don't use this method without testing with the <c>Has</c> method.
    /// </summary>
    /// <param name="availableDetails"></param>
    /// <param name="r"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// 
    public static Detail EnsuredGet(this List<Detail> availableDetails, Relation r, ExerciseToken b) => availableDetails.First(x => x.Operator == r && x.Right == b);


    public static Detail? UnorderedGet(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.FirstOrDefault(x => x.Operator == r && ((x.Left == a && x.Right == b) || (x.Left == b && x.Right == a)));
    public static Detail[] UnorderedGetMany(this List<Detail> availableDetails, ExerciseToken a, Relation r, params ExerciseToken[] b) => availableDetails.Where(x => x.Operator == r && ((x.Left == a && b.Contains(x.Right)) || (b.Contains(x.Left) && x.Right == a))).ToArray();
    public static Detail EnsuredUnorderedGet(this List<Detail> availableDetails, ExerciseToken a, Relation r, ExerciseToken b) => availableDetails.First(x => x.Operator == r && ((x.Left == a && x.Right == b) || (x.Left == b && x.Right == a)));


    public static bool Similar(this Detail a, Detail b)
    {
        if (a.Operator != b.Operator) return false;
        if (new[] { a.Left, a.Right }.Except(new[] { b.Left, b.Right }).Any()) return false;
        return a.Operator switch
        {
            Relation.CONGRUENT or Relation.SIMILAR or Relation.EQUALS or Relation.PERPENDICULAR => 
                (a.Left == b.Left && a.Right == b.Right) || (a.Left == b.Right && a.Right == b.Left),
            Relation.INTERSECTS => 
                ((a.Left == b.Left && a.Right == b.Right) || (a.Left == b.Right && a.Right == b.Left)) && a.SideProducts[0] == b.SideProducts[0],
            _ => 
                a.Left == b.Left && a.Right == b.Right,
        };
    }

    class DE : IEqualityComparer<Detail>
    {
        public DE() : base() { }
        public bool Equals(Detail? x, Detail? y) => x != null && y != null && x.Similar(y);
        public int GetHashCode(Detail obj) => obj.GetHashCode();
    }
    public static IEnumerable<Detail> FilterSimilars(this IEnumerable<Detail> en) => new HashSet<Detail>(en, new DE()).AsEnumerable();
}
