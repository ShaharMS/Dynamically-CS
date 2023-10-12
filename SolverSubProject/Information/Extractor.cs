using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Helpers;
using Dynamically.Solver.Information.BuildingBlocks;

namespace SolverSubProject.Information;

/// <summary>
/// The heart of the auto-solver - here reside the methods used to infer further information.
/// <br/>
/// Categories are sorted by the potential detail, for example, a check for similar peripheral angles on arc will be placed under "Angle", and not "Circle".
/// The ordering within each category is only by the reason's index.
/// <br/>
/// Parallels & perpendiculars will appear under "Angle", not "Length".
/// <br/>
/// The reason used to infer each detail is marked in every method as an attribute.
/// </summary>
public class Extractor
{
    static Detail[] E() => Array.Empty<Detail>();

    /*                                                                                                   
                                                                                        
                ███                                        ███████                     
               █:::█                                       █:::::█                     
              █:::::█     ████ ████████      ████████   ███ █::::█     ████████████    
             █::█ █::█    █::::::::::::██  █::::::::::::::█ █::::█  █::::::█████:::::██
            █::█   █::█     █::::████::::██::::█     █:::█  █::::█ █:::::::█████::::::█
           █:::::::::::█    █:::█    █:::██::::█     █:::█  █::::█ █::::::███████████  
          █::█       █::█   █:::█    █:::██::::::█████:::█ █::::::██::::::::█          
         █::█         █::█  █:::█    █:::█ █:::::::::::::█ █::::::█ █::::::::████████  
        ████           ████ █████    █████   ████████::::█ ████████    ██████████████  
                                           █████:      █:::█                             
                                            █:::::███:::::█                             
                                              ███::::::███                              
                                                 ██████                                 
    */

    [Reason(Reason.ADJACENT_ANGLES_180)]
    public static IEnumerable<Detail> EvaluateAdjacentAngles(TAngle angle)
    {
        if (!angle.HasAdjacentAngles()) return E();

        var adjacentAngles = angle.GetAdjacentAngles();
        return adjacentAngles.Select(a => a.EqualsVal(new TValue($"180\\deg - {a}") { ParentPool = a.ParentPool })).ToArray();
    }

    [Reason(Reason.VERTEX_ANGLES_EQUAL)]
    public static IEnumerable<Detail> EvaluateVertexAngle(TAngle angle)
    {
        if (!angle.HasVertexAngle()) return E();
        var vertexAngle = angle.GetVertexAngle();
        if (vertexAngle == null) return E();
        return new[] { vertexAngle.EqualsVal(angle) };
    }

    
    [Reason(Reason.TRIANGLE_EQUAL_SIDES_EQUAL_ANGLES)]
    public static IEnumerable<Detail> EqualSidesEqualAngles(TTriangle triangle)
    {
        var details = new List<Detail>();
        if (triangle.ParentPool.AvailableDetails.Has(triangle.V1V2, Relation.EQUALS, triangle.V1V3))
        {
            details.Add(
                triangle.V1V3V2.EqualsVal(triangle.V1V2V3).AddReferences(triangle.ParentPool.AvailableDetails.EnsuredGet(triangle.V1V2, Relation.EQUALS, triangle.V1V3))
            );
        }
        if (triangle.ParentPool.AvailableDetails.Has(triangle.V1V2, Relation.EQUALS, triangle.V2V3))
        {
            details.Add(
                triangle.V1V3V2.EqualsVal(triangle.V2V1V3).AddReferences(triangle.ParentPool.AvailableDetails.EnsuredGet(triangle.V1V2, Relation.EQUALS, triangle.V2V3))
            );
        }
        if (triangle.ParentPool.AvailableDetails.Has(triangle.V1V3, Relation.EQUALS, triangle.V2V3))
        {
            details.Add(
                triangle.V1V2V3.EqualsVal(triangle.V2V1V3).AddReferences(triangle.ParentPool.AvailableDetails.EnsuredGet(triangle.V1V3, Relation.EQUALS, triangle.V2V3))
            );
        }
        return details.ToArray();
    }

    [Reason(Reason.ISOSCELES_BASE_ANGLES_EQUAL)]
    public static IEnumerable<Detail> EquateIsoscelesBaseAngles(TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Relation.TRIANGLE_ISOSCELES)) return E();

        var equalSides = triangle.ParentPool.AvailableDetails.EnsuredGet(triangle, Relation.TRIANGLE_ISOSCELES);
        if (equalSides.SideProducts.Count != 2) throw new Exception("Invalid Isosceles triangle - 2 equal sides must be provided");

        TAngle opposite1 = triangle.GetOppositeAngle((TSegment)equalSides.SideProducts[0]);
        TAngle opposite2 = triangle.GetOppositeAngle((TSegment)equalSides.SideProducts[1]);

        var detail = opposite1.EqualsVal(opposite2).AddReferences(equalSides);
        return new[] { detail };
    }

    // public static IEnumerable<Detail> BiggerSideLargerAngleAt(TTriangle triangle)

    [Reason(Reason.TRIANGLE_ANGLE_SUM_180)]
    public static IEnumerable<Detail> EvaluateAngleSumWithinTriangle(TTriangle triangle)
    {
        var detail1 = triangle.V2V1V3.EqualsVal(new TValue($"180\\deg - {triangle.V1V2V3} - {triangle.V1V3V2}") { ParentPool = triangle.ParentPool });
        var detail2 = triangle.V1V2V3.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3} - {triangle.V1V3V2}") { ParentPool = triangle.ParentPool });
        var detail3 = triangle.V1V3V2.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3} - {triangle.V1V2V3}") { ParentPool = triangle.ParentPool });

        return new[] { detail1, detail2, detail3 };
    }

    [Reason(Reason.OUTSIDE_ANGLE_EQUALS_TWO_OTHER_TRIANGLE_ANGLES)]
    public static IEnumerable<Detail> EvaluateOuterAnglesOfTriangle(TTriangle triangle)
    {
        var details = new List<Detail>();
        foreach (TVertex vertex in triangle.GetVertices())
        {
            var oppositeSegment = triangle.GetOppositeSegment(vertex);
            var angle1 = triangle.GetAngleOf(oppositeSegment.V1);
            var angle2 = triangle.GetAngleOf(oppositeSegment.V2);

            var segments = triangle.GetSegments().Except(new[] { oppositeSegment });

            var currentSegment = segments.ElementAt(0);
            var otherSegment = segments.ElementAt(1);

            var currentSegmentExtensionVertex = currentSegment.GetMountsOnExtension(vertex).FirstOrDefault();
            var thirdAngleVertex = otherSegment.Parts.First(x => x != vertex);
            if (currentSegmentExtensionVertex != null)
            {
                var angle = vertex.GetAngle(currentSegmentExtensionVertex, (TVertex)thirdAngleVertex);
                details.Add(angle.EqualsVal(new TValue($"{angle1} + {angle2}")));
            }

            currentSegment = segments.ElementAt(1);
            otherSegment = segments.ElementAt(0);

            currentSegmentExtensionVertex = currentSegment.GetMountsOnExtension(vertex).FirstOrDefault();
            thirdAngleVertex = otherSegment.Parts.First(x => x != vertex);
            if (currentSegmentExtensionVertex != null)
            {
                var angle = vertex.GetAngle(currentSegmentExtensionVertex, (TVertex)thirdAngleVertex);
                details.Add(angle.EqualsVal(new TValue($"{angle1} + {angle2}")));
            }
        }

        return details.ToArray();
    }

    [Reason(Reason.MIDSEGMENT_PARALLEL_OTHER_TRIANGLE_SIDE)]
    public static IEnumerable<Detail> ParallelizeMidSegmentToSide(TTriangle triangle)
    {
        var midSegments = triangle.GetMidSegmentsWithOpposites();
        return midSegments.Select(x => x.midSegment.Parallel(x.opposite).AddReferences(triangle.ParentPool.AvailableDetails.EnsuredGet(x.midSegment, Relation.MIDSEGMENT, triangle))).ToArray();
    }

    /*                                                                                                                             

        █████████                                                             ███      █████          
        █:::::::█                                                          █::::█      █:::█          
          █:::█             ██████████    ████ ███████      ██████   ███████::::████    █::█ ████     
          █:::█          █::::█████:::::███:::::::::::██  █::::::::::::██:::::::::::    █:::::::::██  
          █:::█         █:::::█████::::::█  █::::███::::██::::█   █:::█    █::::█       █::::█  █::::█
          █:::█         █::::███████████    █:::█   █:::██::::█   █:::█    █::::█       █:::█    █:::█
        ██:::::█████:::██::::::█            █:::█   █:::██::::::███:::█    █:::::██:::█ █:::█    █:::█
        █::::::::::::::█  ██:::::::::::█    █:::█   █:::█  █::::::::::█      ██::::::██ █:::█    █:::█
        ████████████████    ████████████    █████   █████   ██████::::█        ██████   █████    █████
                                                         █████    █:::█                               
                                                          █:::::█:::::█                               
                                                            ███::::███                                
    */

    // public static IEnumerable<Detail> LargerAngleBiggerSideAt(TTriangle triangle)

    [Reason(Reason.TRIANGLE_SUM_TWO_SIDES_LARGER_THIRD)]
    public static IEnumerable<Detail> TriangleSumTwoSidesLargerThird(TTriangle triangle)
    {
        return new[]
        {
            triangle.V1V2.Smaller(new TValue($"{triangle.V1V3} + {triangle.V2V3}")),
            triangle.V1V3.Smaller(new TValue($"{triangle.V1V2} + {triangle.V2V3}")),
            triangle.V2V3.Smaller(new TValue($"{triangle.V1V2} + {triangle.V1V3}"))
        };
    }

    [Reason(Reason.LINE_BISECTS_SIDE_PARALLEL_OTHER_BISECTS_THIRD)]
    public static IEnumerable<Detail> MidsegmentProperties_A(TTriangle triangle)
    {
        foreach (TSegment side in triangle.Sides)
        {
            var bisectors = side.GetBisectors();
            var otherSides = triangle.Sides.Except(side);
            foreach (TSegment otherSide in otherSides)
            {
                foreach (TSegment bisector in bisectors)
                {
                    if (bisector.IsParallel(otherSide)) 
                        yield return bisector.Bisects(otherSides.Except(otherSide).First()).AddReferences(
                            triangle.ParentPool.AvailableDetails.EnsuredGet(bisector, Relation.BISECTS, side),
                            triangle.ParentPool.AvailableDetails.Get(bisector, Relation.PARALLEL, otherSide) ?? triangle.ParentPool.AvailableDetails.Get(otherSide, Relation.PARALLEL, bisector) ?? throw new Exception("`Parallel` detail used, but not found")
                        );
                }
            }
        }
    }

    [Reason(Reason.LINE_INTERSECTS_SIDES_PARALLEL_THIRD_HALF_THIRD_LENGTH_IS_MIDSEGMENT)]
    public static IEnumerable<Detail> ExtractMidSegments(TTriangle triangle)
    {
        foreach (TSegment side in triangle.Sides)
        {
            var intersectors = side.GetIntersectors();
            var otherSides = triangle.Sides.Except(side);
            foreach (TSegment otherSide in otherSides)
            {
                foreach (TSegment intersector in intersectors)
                {
                    if (intersector.IsParallel(otherSide) && intersector.GetValue() == new TValue($"{intersector.GetValue()} / 2"))
                        yield return intersector.MidSegment(triangle).AddReferences(
                            triangle.ParentPool.AvailableDetails.Get(intersector, Relation.INTERSECTS, otherSide) ?? triangle.ParentPool.AvailableDetails.Get(otherSide, Relation.PARALLEL, intersector) ?? throw new Exception("`Intersect` detail used, but not found"),
                            triangle.ParentPool.AvailableDetails.Get(intersector, Relation.PARALLEL, otherSide) ?? triangle.ParentPool.AvailableDetails.Get(otherSide, Relation.PARALLEL, intersector) ?? throw new Exception("`Parallel` detail used, but not found"),
                            triangle.ParentPool.AvailableDetails.EnsuredGet(intersector, Relation.EQUALS, new TValue($"{intersector.GetValue()} / 2"))
                        );
                }
            }
        }
    }

    /*                                                                                                                                       

        ██████████████                ███                                             ████             
        █::██::::██::█                                                                █::█             
        ███  █::█  ██████   █████   ██████   █████████   ████ ███████     ███████  ███ ██   ████████  
             █::█     █:::::::::::█  █:::█   ███████:::█ █:::::::::::██  █:::::::::::█ █:█  █::████::██
             █::█      █:::█   █:::█ █:::█     █████:::█   █::::███::::██:::█    █::█  █:█ █:::████:::█
             █::█      █:::█   █████ █:::█   ██::::::::█   █:::█   █:::██:::█    █::█  █:█ █:::::::::█ 
             █::█      █:::█         █:::█ █::::   █:::█   █:::█   █:::██::::█   █::█  █:█ █:::█       
           █::::::█    █:::█        █:::::██::::███::::█   █:::█   █:::█ █::::::::::█ █:::█ █::::█████  
           ████████    █████        ███████  ███████  ███  █████   █████  ███████:::█ █████  █████████  
                                                                        ████:     █::█                  
                                                                         █::::██::::█                  
                                                                          ███:::::█                    
                                                                             █████:                     
    */

    [Reason(Reason.ISOSCELES_PERPENDICUAL_ANGLEBISECTOR_BISECTOR)]
    public static IEnumerable<Detail> MergedAngleBisectorPerpendicularAndBisector(TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Relation.TRIANGLE_ISOSCELES)) return E();

        var headAngle = triangle.GetIsoscelesHeadAngle();
        var baseSide = triangle.GetIsoscelesBaseSide();

        var potentials = triangle.ParentPool.AvailableDetails.GetMany((Relation.BISECTS, Relation.PERPENDICULAR), baseSide).Where(x => x.Right is TSegment);
        var potentials2 = triangle.ParentPool.AvailableDetails.GetMany(baseSide, Relation.PERPENDICULAR);
        var potentials3 = triangle.ParentPool.AvailableDetails.GetMany(Relation.BISECTS, headAngle);

        var details = new List<Detail>();

        foreach (var potential in potentials)
        {
            if (potential.Left is not TSegment) continue;
            var seg = (potential.Left as TSegment)!;
            if (potential.Operator != Relation.PERPENDICULAR) details.Add(seg.Perpendicular(baseSide).AddReferences(potential));
            if (potential.Operator != Relation.BISECTS) details.Add(seg.Bisects(baseSide).AddReferences(potential));
            details.Add(seg.Bisects(headAngle).AddReferences(potential));
        }
        foreach (var potential in potentials2)
        {
            if (potential.Right is not TSegment) continue;
            var seg = (potential.Right as TSegment)!;

            details.Add(seg.Bisects(baseSide).AddReferences(potential));
            details.Add(seg.Bisects(headAngle).AddReferences(potential));
        }
        foreach (var potential in potentials3)
        {
            if (potential.Left is not TSegment) continue;
            var seg = (potential.Left as TSegment)!;

            details.Add(seg.Bisects(baseSide).AddReferences(potential));
            details.Add(seg.Perpendicular(baseSide).AddReferences(potential));
        }
        return details.ToArray();
    }

    [Reason(Reason.TRIANGLE_ANGLEBISECTOR_PERPENDICULAR_IS_ISOSCELES)]
    public static IEnumerable<Detail> IsTriangleIsosceles_A(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;
        var angleBisectors = all.GetMany(Relation.BISECTS, triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2);
        if (!angleBisectors.Any()) return E();
        var perpendiculars = all.GetMany(Relation.PERPENDICULAR, triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2).Concat(all.GetMany((triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2), Relation.PERPENDICULAR)).FilterSimilars();

        foreach (var bisectorDetail in angleBisectors)
        {
            var seg = (bisectorDetail.Left as TSegment)!;
            if (perpendiculars.Any(x => x.Right == seg || x.Left == seg))
            {
                return new[] { new Detail(triangle, Relation.TRIANGLE_ISOSCELES).AddReferences(bisectorDetail, perpendiculars.First(x => x.Right == seg || x.Left == seg)) };
            }
        }

        return E();
    }

    [Reason(Reason.TRIANGLE_ANGLEBISECTOR_BISECTOR_IS_ISOSCELES)]
    public static IEnumerable<Detail> IsTriangleIsosceles_B(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;
        var angleBisectors = all.GetMany(Relation.BISECTS, triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2);
        if (!angleBisectors.Any()) return E();
        var bisectors = all.GetMany(Relation.BISECTS, triangle.V1V2, triangle.V2V3, triangle.V1V3);

        foreach (var angleBisectorDetail in angleBisectors)
        {
            var seg = (angleBisectorDetail.Left as TSegment)!;
            if (bisectors.Any(x => x.Left == seg))
            {
                return new[] { new Detail(triangle, Relation.TRIANGLE_ISOSCELES).AddReferences(angleBisectorDetail, bisectors.First(x => x.Right == seg || x.Left == seg)) };
            }
        }

        return E();
    }



    [Reason(Reason.TRIANGLE_PERPENDICULAR_BISECTOR_IS_ISOSCELES)]
    public static IEnumerable<Detail> IsTriangleIsosceles_C(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;
        var bisectors = all.GetMany(Relation.BISECTS, triangle.V1V2, triangle.V2V3, triangle.V1V3);
        if (!bisectors.Any()) return E();
        var perpendiculars = all.GetMany(Relation.PERPENDICULAR, triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2).Concat(all.GetMany((triangle.V1V2V3, triangle.V2V1V3, triangle.V1V3V2), Relation.PERPENDICULAR)).FilterSimilars();

        foreach (var bisectorDetail in bisectors)
        {
            var seg = (bisectorDetail.Left as TSegment)!;
            if (perpendiculars.Any(x => x.Right == seg || x.Left == seg))
            {
                return new[] { new Detail(triangle, Relation.TRIANGLE_ISOSCELES).AddReferences(bisectorDetail, perpendiculars.First(x => x.Right == seg || x.Left == seg)) };
            }
        }

        return E();
    }

    [Reason(Reason.TRIANGLE_CONGRUENCY_S_A_S)]
    public static IEnumerable<Detail> TriangleCongruencyVia_SAS(TTriangle triangle1, TTriangle triangle2)
    {
        TokenHelpers.Validate(triangle1, triangle2);
        var all = triangle1.ParentPool.AvailableDetails;
        var anglesOfT1 = triangle1.GetAngles();
        var anglesOfT2 = triangle2.GetAngles();
        var equals = new List<(TAngle, TAngle)>();
        foreach (var angle1 in anglesOfT1)
        {
            foreach (var angle2 in anglesOfT2) 
            {
                if (angle1.GetValue() == angle2.GetValue())
                    equals.Add((angle1, angle2));
            }
        }

        foreach ((TAngle a1, TAngle a2) in equals) {
            if (a1.Segment1 == null || a1.Segment2 == null || a2.Segment1 == null || a2.Segment2 == null) continue;
            if (a1.Segment1.GetValue() == a2.Segment1.GetValue() && a1.Segment2.GetValue() == a2.Segment2.GetValue()) {
                yield return triangle1.Congruent(triangle2, (a1.Segment1, a2.Segment1), (a1, a2), (a1.Segment2, a2.Segment2));
            }
            else if (a1.Segment1.GetValue() == a2.Segment2.GetValue() && a1.Segment2.GetValue() == a2.Segment1.GetValue()) {
                yield return triangle1.Congruent(triangle2, (a1.Segment1, a2.Segment2), (a1, a2), (a1.Segment2, a2.Segment1));
            }
        }
    }

    /*                                                                            
                                                                            ████████
           █████████████                                                    █::::::█
         ██:::::::::::::██                                                  █::::::█
        █::::::█   █::::::█ ██████    ██████    █████████████       █████████:::::█ 
        █:::::█     █:::::█ █::::█    █::::█    █████████:::::█  █::::::::::::::::█ 
        █:::::█     █:::::█ █::::█    █::::█      ███████:::::█ █::::::█    █:::::█ 
        █:::::█  ████:::::█ █::::█    █::::█   █::::████::::::█ █:::::█     █:::::█ 
        █:::::::██::::::::█ █:::::::::::::::███::::█    █:::::█ █::::::█████::::::██
           ██:::::::::::█     ██::::::::██:::█ █::::::::::██:::█  █:::::::::███::::█
             █████████:::██     ████████  ████  ██████████  ████   █████████   █████
                     █::::█
                      ██████                                                        
    */


    /*                                                                                                 

               █████████  █████                                ███████                     
           █:::::::::::█  █████                                █:::::█                     
          █::::█████:::█                                       █:::::█                     
         █::::█    █████ ██████ ████   ███████      ██████████  ██::::█     ████████████    
        █::::█            █:::█ █::::::::::::::█  █:::::::::::█  █::::█  █::::::█████:::::██
        █::::█            █:::█ ██:::::████:::::██::::::███:::█  █::::█ █::::::█     █:::::█
        █::::█            █:::█  █::::█          █::::█          █::::█ █::::::███████████  
          █::::█████:::█ █:::::█ █::::█          █::::::███:::█ █::::::██::::::::█          
           █:::::::::::█ █:::::█ █::::█           █:::::::::::█ █::::::█ █::::::::████████  
               █████████ ███████ ██████             ██████████ █████████    ██████████████ 
    */
}
