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
        Segment s1 = quad.con1, s2 = quad.con2, s3 = quad.con3, s4 = quad.con4;
        if (s1.SharesJointWith(s2))
        {
            quad._degrees1 = () => Tools.GetDegreesBetweenConnections(s1, s2, true);
            quad.angle1Joints = new HashSet<Vertex> { s1.joint1, s1.joint2, s2.joint1, s2.joint2 }.Where(x => x != s1.GetSharedJoint(s2)).ToList().InsertR(1, s1.GetSharedJoint(s2)).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s3, s4, true);
            quad.angle2Joints = new HashSet<Vertex> { s3.joint1, s3.joint2, s4.joint1, s4.joint2 }.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
            if (s2.SharesJointWith(s3))
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
                quad.angle3Joints = new HashSet<Vertex>{s3.joint1, s3.joint2, s2.joint1, s2.joint2}.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
                quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
                quad.angle4Joints = new HashSet<Vertex>{s1.joint1, s1.joint2, s4.joint1, s4.joint2}.Where(x => x != s1.GetSharedJoint(s4)).ToList().InsertR(1, s1.GetSharedJoint(s4)).ToArray();
            }
            else
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
                quad.angle3Joints = new HashSet<Vertex> { s4.joint1, s4.joint2, s2.joint1, s2.joint2 }.Where(x => x != s4.GetSharedJoint(s2)).ToList().InsertR(1, s4.GetSharedJoint(s2)).ToArray<Vertex>();
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
                quad.angle4Joints = new HashSet<Vertex> { s3.joint1, s3.joint2, s1.joint1, s1.joint2 }.Where(x => x != s1.GetSharedJoint(s3)).ToList().InsertR(1, s1.GetSharedJoint(s3)).ToArray();
            }
        }
        else
        { // there is only one case in which s1 does not share with s2. in which case, s3 and s4 must share with both s1 and s2.
            quad._degrees1 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
            quad.angle1Joints = new HashSet<Vertex> { s1.joint1, s1.joint2, s3.joint1, s3.joint2 }.Where(x => x != s1.GetSharedJoint(s3)).ToList().InsertR(1, s1.GetSharedJoint(s3)).ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
            quad.angle2Joints = new HashSet<Vertex> { s4.joint1, s4.joint2, s2.joint1, s2.joint2 }.Where(x => x != s4.GetSharedJoint(s2)).ToList().InsertR(1, s4.GetSharedJoint(s2)).ToArray();
            quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
            quad.angle3Joints = new HashSet<Vertex> { s3.joint1, s3.joint2, s2.joint1, s2.joint2 }.Where(x => x != s3.GetSharedJoint(s2)).ToList().InsertR(1, s3.GetSharedJoint(s2)).ToArray();
            quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
            quad.angle4Joints = new HashSet<Vertex> { s1.joint1, s1.joint2, s4.joint1, s4.joint2 }.Where(x => x != s1.GetSharedJoint(s4)).ToList().InsertR(1, s1.GetSharedJoint(s4)).ToArray();
        }
    }

    static void AssignSegmentData(Quadrilateral quad)
    {
        if (!quad.con1.SharesJointWith(quad.con3))
        {
            quad.opposites = new Tuple<Segment, Segment>[] { new(quad.con1, quad.con3), new(quad.con2, quad.con4) };
            quad.adjacents = new Tuple<Segment, Segment>[] { new(quad.con1, quad.con2), new(quad.con3, quad.con4), new(quad.con3, quad.con2), new(quad.con1, quad.con4) };
        }
        else if (!quad.con1.SharesJointWith(quad.con2))
        {
            quad.opposites = new Tuple<Segment, Segment>[] { new(quad.con1, quad.con2), new(quad.con3, quad.con4) };
            quad.adjacents = new Tuple<Segment, Segment>[] { new(quad.con1, quad.con3), new(quad.con2, quad.con4), new(quad.con3, quad.con2), new(quad.con1, quad.con4) };
        } 
        else
        {
            quad.opposites = new Tuple<Segment, Segment>[] { new(quad.con2, quad.con3), new(quad.con1, quad.con4) };
            quad.adjacents = new Tuple<Segment, Segment>[] { new(quad.con1, quad.con3), new(quad.con3, quad.con4), new(quad.con1, quad.con2), new(quad.con2, quad.con4) };
        }
    }
}