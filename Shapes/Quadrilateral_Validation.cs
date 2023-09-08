using System;
using System.Collections.Generic;
using System.Numerics;
using Avalonia;
using Dynamically.Backend.Geometry;
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

    void AssignAngles(Quadrilateral quad)
    {
        Segment s1 = quad.con1, s2 = quad.con2, s3 = quad.con3, s4 = quad.con4;
        if (s1.SharesJointWith(s2))
        {
            quad.angle1 = new Ang
        }
        else
        {
             
        }
    }
}