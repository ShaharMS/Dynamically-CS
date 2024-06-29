using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Geometry.Basics;

namespace Dynamically.Geometry;

public partial class Quadrilateral {

    public static List<(Vertex, Vertex)> GetValidQuadrilateralSides(Vertex A, Vertex B, Vertex C, Vertex D)
    {
        Log.Write("Checking quadrilateral: " + A + " " + B + " " + C + " " + D);
        // First check: vertices not on single line
        var triplets = new[] {(A, B, C), (A, C, D), (A, D, B), (B, C, D)};
        foreach (var (a, b, c) in triplets)
        {
            Log.WriteVar(a, b, c);
            var segments = new[] { (a.GetSegmentTo(b), c), (b.GetSegmentTo(c), a), (c.GetSegmentTo(a), b) };
            foreach (var (segment, potential) in segments)
            {
                Log.WriteVar(segment, potential);
                if (segment == null) continue;
                // first case - vertex on line
                if (segment.RayFormula.Followers.Contains(potential)) return new List<(Vertex, Vertex)>();
                if (segment.Formula.Followers.Contains(potential)) return new List<(Vertex, Vertex)>();
                if (segment.MiddleFormula.Followers.Contains(potential)) return new List<(Vertex, Vertex)>();
                Log.WriteVar(segment.Roles.Underlying);
                // second case - within circles
                if (segment.Roles.Has(Role.CIRCLE_Diameter) && potential.Roles.Has(Role.CIRCLE_Center))
                {
                    foreach (var circle in segment.Roles.Access<Circle>(Role.CIRCLE_Diameter))
                    {
                        Log.WriteVar(circle, potential);
                        if (circle.Contains(potential)) return new List<(Vertex, Vertex)>();
                    }
                }
            }
        }

        var candidates = new List<((Vertex, Vertex), (Vertex, Vertex))>();

        foreach (var pairs in new[] {((A, B), (C, D)), ((A, C), (B, D)), ((A, D), (B, C))}) {
            var s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item1.Item2);
            var s2 = new SegmentFormula(pairs.Item2.Item1, pairs.Item2.Item2);

            if (s1.Intersect(s2) == null) candidates.Add(pairs);
        }

        foreach (var pairs in candidates) {
            var attempt1s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item2.Item1);
            var attempt1s2 = new SegmentFormula(pairs.Item1.Item2, pairs.Item2.Item2);
        
            if (attempt1s1.Intersect(attempt1s2) == null) {
                return new List<(Vertex, Vertex)>{
                    pairs.Item1, 
                    pairs.Item2,
                    (pairs.Item1.Item1, pairs.Item2.Item1),
                    (pairs.Item1.Item2, pairs.Item2.Item2)
                };
            }

            var attempt2s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item2.Item2);
            var attempt2s2 = new SegmentFormula(pairs.Item1.Item2, pairs.Item2.Item1);

            if (attempt2s1.Intersect(attempt2s2) == null) {
                return new List<(Vertex, Vertex)>{
                    pairs.Item1, 
                    pairs.Item2,
                    (pairs.Item1.Item1, pairs.Item2.Item2),
                    (pairs.Item1.Item2, pairs.Item2.Item1)
                };
            }
        }


        return new List<(Vertex, Vertex)>(); // Not a valid quadrilateral
    }

    static void AssignAngles(Quadrilateral quad)
    {
        Segment s1 = quad.Segment1, s2 = quad.Segment2, s3 = quad.Segment3, s4 = quad.Segment4;
        if (s1.SharesVertexWith(s2))
        {
            quad._degrees1 = () => Tools.GetDegreesBetweenSegments(s1, s2, true);
            quad.Angle1Vertices = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s1.GetSharedVertex(s2)).ToList().InsertR(1, s1.GetSharedVertex(s2)!).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenSegments(s3, s4, true);
            quad.Angle2Vertices = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s4.Vertex1, s4.Vertex2 }.Where(x => x != s3.GetSharedVertex(s2)).ToList().InsertR(1, s3.GetSharedVertex(s2)!).ToArray();
            if (s2.SharesVertexWith(s3))
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenSegments(s2, s3, true);
                quad.Angle3Vertices = new HashSet<Vertex>{s3.Vertex1, s3.Vertex2, s2.Vertex1, s2.Vertex2}.Where(x => x != s3.GetSharedVertex(s2)).ToList().InsertR(1, s3.GetSharedVertex(s2)!).ToArray();
                quad._degrees4 = () => Tools.GetDegreesBetweenSegments(s1, s4, true);
                quad.Angle4Vertices = new HashSet<Vertex>{s1.Vertex1, s1.Vertex2, s4.Vertex1, s4.Vertex2}.Where(x => x != s1.GetSharedVertex(s4)).ToList().InsertR(1, s1.GetSharedVertex(s4)!).ToArray();
            }
            else
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenSegments(s2, s4, true);
                quad.Angle3Vertices = new HashSet<Vertex> { s4.Vertex1, s4.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s4.GetSharedVertex(s2)).ToList().InsertR(1, s4.GetSharedVertex(s2)!).ToArray<Vertex>();
                quad._degrees3 = () => Tools.GetDegreesBetweenSegments(s1, s3, true);
                quad.Angle4Vertices = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s1.Vertex1, s1.Vertex2 }.Where(x => x != s1.GetSharedVertex(s3)).ToList().InsertR(1, s1.GetSharedVertex(s3)!).ToArray();
            }
        }
        else
        { // there is only one case in which s1 does not share with s2. in which case, s3 and s4 must share with both s1 and s2.
            quad._degrees1 = () => Tools.GetDegreesBetweenSegments(s1, s3, true);
            quad.Angle1Vertices = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s3.Vertex1, s3.Vertex2 }.Where(x => x != s1.GetSharedVertex(s3)).ToList().InsertR(1, s1.GetSharedVertex(s3)!).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenSegments(s2, s4, true);
            quad.Angle2Vertices = new HashSet<Vertex> { s4.Vertex1, s4.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s4.GetSharedVertex(s2)).ToList().InsertR(1, s4.GetSharedVertex(s2)!).ToArray();
            quad._degrees3 = () => Tools.GetDegreesBetweenSegments(s2, s3, true);
            quad.Angle3Vertices = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s3.GetSharedVertex(s2)).ToList().InsertR(1, s3.GetSharedVertex(s2)!).ToArray();
            quad._degrees4 = () => Tools.GetDegreesBetweenSegments(s1, s4, true);
            quad.Angle4Vertices = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s4.Vertex1, s4.Vertex2 }.Where(x => x != s1.GetSharedVertex(s4)).ToList().InsertR(1, s1.GetSharedVertex(s4)!).ToArray();
        }
    }

    static void AssignSegmentData(Quadrilateral quad)
    {
        if (!quad.Segment1.SharesVertexWith(quad.Segment3))
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Segment1, quad.Segment3), new(quad.Segment2, quad.Segment4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Segment1, quad.Segment2), new(quad.Segment3, quad.Segment4), new(quad.Segment3, quad.Segment2), new(quad.Segment1, quad.Segment4) };
        }
        else if (!quad.Segment1.SharesVertexWith(quad.Segment2))
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Segment1, quad.Segment2), new(quad.Segment3, quad.Segment4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Segment1, quad.Segment3), new(quad.Segment2, quad.Segment4), new(quad.Segment3, quad.Segment2), new(quad.Segment1, quad.Segment4) };
        } 
        else
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Segment2, quad.Segment3), new(quad.Segment1, quad.Segment4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Segment1, quad.Segment3), new(quad.Segment3, quad.Segment4), new(quad.Segment1, quad.Segment2), new(quad.Segment2, quad.Segment4) };
        }
    }
}