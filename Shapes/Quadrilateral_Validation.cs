using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;

namespace Dynamically.Shapes;

public partial class Quadrilateral {

    public static List<(Joint, Joint)> GetValidQuadrilateralSides(Joint A, Joint B, Joint C, Joint D)
    {
        var candidates = new List<((Joint, Joint), (Joint, Joint))>();

        foreach (var pairs in new[] {((A, B), (C, D)), ((A, C), (B, D)), ((A, D), (B, C))}) {
            var s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item1.Item2);
            var s2 = new SegmentFormula(pairs.Item2.Item1, pairs.Item2.Item2);

            if (s1.Intersect(s2) == null) candidates.Add(pairs);
        }

        foreach (var pairs in candidates) {
            var attempt1s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item2.Item1);
            var attempt1s2 = new SegmentFormula(pairs.Item1.Item2, pairs.Item2.Item2);
        
            if (attempt1s1.Intersect(attempt1s2) == null) {
                return new List<(Joint, Joint)>{
                    pairs.Item1, 
                    pairs.Item2,
                    (pairs.Item1.Item1, pairs.Item2.Item1),
                    (pairs.Item1.Item2, pairs.Item2.Item2)
                };
            }

            var attempt2s1 = new SegmentFormula(pairs.Item1.Item1, pairs.Item2.Item2);
            var attempt2s2 = new SegmentFormula(pairs.Item1.Item2, pairs.Item2.Item1);

            if (attempt2s1.Intersect(attempt2s2) == null) {
                return new List<(Joint, Joint)>{
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
            quad.angle1Joints = new HashSet<Joint> { s1.joint1, s1.joint2, s2.joint1, s2.joint2 }.ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s3, s4, true);
            quad.angle1Joints = new HashSet<Joint> { s3.joint1, s3.joint2, s4.joint1, s4.joint2 }.ToArray();
            if (s2.SharesJointWith(s3))
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
                quad.angle3Joints = new HashSet<Joint>{s3.joint1, s3.joint2, s2.joint1, s2.joint2}.ToArray();
                quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
                quad.angle4Joints = new HashSet<Joint>{s1.joint1, s1.joint2, s4.joint1, s4.joint2}.ToArray();
            }
            else
            {
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
                quad.angle3Joints = new HashSet<Joint> { s4.joint1, s4.joint2, s2.joint1, s2.joint2 }.ToArray();
                quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
                quad.angle4Joints = new HashSet<Joint> { s3.joint1, s3.joint2, s1.joint1, s1.joint2 }.ToArray();
            }
        }
        else
        { // there is only one case in which s1 does not share with s2. in which case, s3 and s4 much share with both s1 and s2.
            quad._degrees1 = () => Tools.GetDegreesBetweenConnections(s1, s3, true);
            quad.angle1Joints = new HashSet<Joint> { s1.joint1, s1.joint2, s3.joint1, s3.joint2 }.ToArray();
            quad._degrees2 = () => Tools.GetDegreesBetweenConnections(s2, s4, true);
            quad.angle1Joints = new HashSet<Joint> { s4.joint1, s4.joint2, s2.joint1, s2.joint2 }.ToArray();
            quad._degrees3 = () => Tools.GetDegreesBetweenConnections(s2, s3, true);
            quad.angle1Joints = new HashSet<Joint> { s3.joint1, s3.joint2, s2.joint1, s2.joint2 }.ToArray();
            quad._degrees4 = () => Tools.GetDegreesBetweenConnections(s1, s4, true);
            quad.angle1Joints = new HashSet<Joint> { s1.joint1, s1.joint2, s4.joint1, s4.joint2 }.ToArray();
        }
    }
}