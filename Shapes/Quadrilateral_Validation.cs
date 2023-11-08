using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Backend;

namespace Dynamically.Shapes;

public partial class Quadrilateral {

    public static List<(Vertex, Vertex)> GetValidQuadrilateralSides(Vertex A, Vertex B, Vertex C, Vertex D)
    {
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

        return new();
    }

    static void AssignAngles(Quadrilateral quad)
    {
        Segment s1 = quad.Con1, s2 = quad.Con2, s3 = quad.Con3, s4 = quad.Con4;
        if (s1.SharesJointWith(s2))
        {
            quad._degrees1 = () => Tools.GetDegreesBetweenConnections(s1, s2, true);
            quad.Angle1Joints = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s1.GetSharedJoint(s2)).ToList().InsertR(1, s1.GetSharedJoint(s2)).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s3, s4, true);
            quad.Angle2Joints = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s4.Vertex1, s4.Vertex2 }.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
            if (s2.SharesJointWith(s3))
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
                quad.Angle3Joints = new HashSet<Vertex>{s3.Vertex1, s3.Vertex2, s2.Vertex1, s2.Vertex2}.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
                quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
                quad.Angle4Joints = new HashSet<Vertex>{s1.Vertex1, s1.Vertex2, s4.Vertex1, s4.Vertex2}.Where(x => x != s1.GetSharedJoint(s4)).ToList().InsertR(1, s1.GetSharedJoint(s4)).ToArray();
            }
            else
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
                quad.Angle3Joints = new HashSet<Vertex> { s4.Vertex1, s4.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s4.GetSharedJoint(s2)).ToList().InsertR(1, s4.GetSharedJoint(s2)).ToArray<Vertex>();
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
                quad.Angle4Joints = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s1.Vertex1, s1.Vertex2 }.Where(x => x != s1.GetSharedJoint(s3)).ToList().InsertR(1, s1.GetSharedJoint(s3)).ToArray();
            }
        }
        else
        { // there is only one case in which s1 does not share with s2. in which case, s3 and s4 must share with both s1 and s2.
            quad._degrees1 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
            quad.Angle1Joints = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s3.Vertex1, s3.Vertex2 }.Where(x => x != s1.GetSharedJoint(s3)).ToList().InsertR(1, s1.GetSharedJoint(s3)).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
            quad.Angle2Joints = new HashSet<Vertex> { s4.Vertex1, s4.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s4.GetSharedJoint(s2)).ToList().InsertR(1, s4.GetSharedJoint(s2)).ToArray();
            quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
            quad.Angle3Joints = new HashSet<Vertex> { s3.Vertex1, s3.Vertex2, s2.Vertex1, s2.Vertex2 }.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
            quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
            quad.Angle4Joints = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s4.Vertex1, s4.Vertex2 }.Where(x => x != s1.GetSharedJoint(s4)).ToList().InsertR(1, s1.GetSharedJoint(s4)).ToArray();
        }
    }

    static void AssignSegmentData(Quadrilateral quad)
    {
        if (!quad.Con1.SharesJointWith(quad.Con3))
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Con1, quad.Con3), new(quad.Con2, quad.Con4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Con1, quad.Con2), new(quad.Con3, quad.Con4), new(quad.Con3, quad.Con2), new(quad.Con1, quad.Con4) };
        }
        else if (!quad.Con1.SharesJointWith(quad.Con2))
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Con1, quad.Con2), new(quad.Con3, quad.Con4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Con1, quad.Con3), new(quad.Con2, quad.Con4), new(quad.Con3, quad.Con2), new(quad.Con1, quad.Con4) };
        } 
        else
        {
            quad.Opposites = new Tuple<Segment, Segment>[] { new(quad.Con2, quad.Con3), new(quad.Con1, quad.Con4) };
            quad.Adjacents = new Tuple<Segment, Segment>[] { new(quad.Con1, quad.Con3), new(quad.Con3, quad.Con4), new(quad.Con1, quad.Con2), new(quad.Con2, quad.Con4) };
        }
    }
}