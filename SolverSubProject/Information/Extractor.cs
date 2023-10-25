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
/// <br/><br/>Exceptions:
/// 
/// <list type="bullet">
///     <item>Parallels and perpendiculars will appear under "Segment", and not "Angle".</item>
///     <item>if something is specific to a type of shape (for example - kite main diagonal), it would be placed under its related shape, and not category.</item>
/// </list>
/// <br/><br/>
/// The reason used to infer each detail is marked in every method as an attribute.
/// </summary>
public class Extractor
{
    static IEnumerable<Detail> E() => Enumerable.Empty<Detail>();

    /*                                                                                                   
                                                                                        
                █████                                        ███████                     
               █:::::█                                       █:::::█                     
              █:::::::█     ████ ████████      ████████   ███ █::::█     ████████████    
             █:::█ █:::█    █::::::::::::██  █::::::::::::::█ █::::█  █::::::█████:::::██
            █:::█   █:::█     █::::████::::██::::█     █:::█  █::::█ █:::::::█████::::::█
           █:::::::::::::█    █:::█    █:::██::::█     █:::█  █::::█ █::::::███████████  
          █:::█       █:::█   █:::█    █:::██::::::█████:::█ █::::::██::::::::█          
         █:::█         █:::█  █:::█    █:::█ █:::::::::::::█ █::::::█ █::::::::████████  
        █████           █████ █████    █████   ████████::::█ ████████    ██████████████  
                                            ████:      █:::█                             
                                             █::::███:::::█                             
                                               ██::::::███                              
                                                 ██████                                 
    */

    [Reason(Reason.ADJACENT_ANGLES_180)]
    public static IEnumerable<Detail> EvaluateAdjacentAngles(TAngle angle)
    {
        if (!angle.HasAdjacentAngles()) return E();

        var adjacentAngles = angle.GetAdjacentAngles();
        return adjacentAngles.Select(
            a => a.GetValue().EqualsVal(new TValue($"180\\deg - {a.GetValue()}") { ParentPool = a.ParentPool }).AddReferences(a.GetValueDetail())
        ).ToArray();
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
                triangle.V1V3V2.EqualsVal(triangle.V1V2V3).AddReferences(triangle.V1V2.GetEqualityDetail(triangle.V1V3))
            );
        }
        if (triangle.ParentPool.AvailableDetails.Has(triangle.V1V2, Relation.EQUALS, triangle.V2V3))
        {
            details.Add(
                triangle.V1V3V2.EqualsVal(triangle.V2V1V3).AddReferences(triangle.V1V2.GetEqualityDetail(triangle.V2V3))
            );
        }
        if (triangle.ParentPool.AvailableDetails.Has(triangle.V1V3, Relation.EQUALS, triangle.V2V3))
        {
            details.Add(
                triangle.V1V2V3.EqualsVal(triangle.V2V1V3).AddReferences(triangle.V1V3.GetEqualityDetail(triangle.V2V3))
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

        return new[] { opposite1.EqualsVal(opposite2).AddReferences(equalSides) };
    }

    // public static IEnumerable<Detail> BiggerSideLargerAngleAt(TTriangle triangle)

    [Reason(Reason.TRIANGLE_ANGLE_SUM_180)]
    public static IEnumerable<Detail> EvaluateAngleSumWithinTriangle(TTriangle triangle)
    {
        var detail1 = triangle.V2V1V3.EqualsVal(new TValue($"180\\deg - {triangle.V1V2V3.GetValue()} - {triangle.V1V3V2.GetValue()}") { ParentPool = triangle.ParentPool }).AddReferences(triangle.V1V2V3.GetValueDetail(), triangle.V1V3V2.GetValueDetail());
        var detail2 = triangle.V1V2V3.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3.GetValue()} - {triangle.V1V3V2.GetValue()}") { ParentPool = triangle.ParentPool }).AddReferences(triangle.V2V1V3.GetValueDetail(), triangle.V1V3V2.GetValueDetail());
        var detail3 = triangle.V1V3V2.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3.GetValue()} - {triangle.V1V2V3.GetValue()}") { ParentPool = triangle.ParentPool }).AddReferences(triangle.V2V1V3.GetValueDetail(), triangle.V1V2V3.GetValueDetail());

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
                details.Add(angle.EqualsVal(angle1.GetValue() + angle2.GetValue()).AddReferences(angle1.GetValueDetail(), angle2.GetValueDetail()));
            }

            currentSegment = segments.ElementAt(1);
            otherSegment = segments.ElementAt(0);

            currentSegmentExtensionVertex = currentSegment.GetMountsOnExtension(vertex).FirstOrDefault();
            thirdAngleVertex = otherSegment.Parts.First(x => x != vertex);
            if (currentSegmentExtensionVertex != null)
            {
                var angle = vertex.GetAngle(currentSegmentExtensionVertex, (TVertex)thirdAngleVertex);
                details.Add(angle.EqualsVal(angle1.GetValue() + angle2.GetValue()).AddReferences(angle1.GetValueDetail(), angle2.GetValueDetail()));
            }
        }

        return details.ToArray();
    }

    [Reason(Reason.CORRESPONDING_ANGLES_EQUAL)]
    public static IEnumerable<Detail> CorrespondingAnglesOnParallels(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:
                
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() == potential2.GetValue())
                    yield return potential1.EqualsVal(potential2).AddReferences(
                        pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2),
                        potential1.GetEqualityDetail(potential2)
                    );
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
                
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                yield return potential1.EqualsVal(potential2).AddReferences(
                    pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2)
                );
        }
    }

    [Reason(Reason.ALTERNATING_ANGLES_EQUAL)]
    public static IEnumerable<Detail> AlternatingAnglesOnParallels(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:

                              V2
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
                          V1
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                yield return potential1.EqualsVal(potential2).AddReferences(
                    pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2)
                );
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
            
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                yield return potential1.EqualsVal(potential2).AddReferences(
                    pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2)
                );
        }
    }



    [Reason(Reason.COINTERIOR_ANGLES_180)]
    public static IEnumerable<Detail> CointeriorAnglesOnParallels(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:

                              V2
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
                          V1
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                yield return (potential1.GetValue() + potential2.GetValue()).EqualsVal(new TValue(180)).AddReferences(
                    pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2),
                    potential1.GetValueDetail(), potential2.GetValueDetail()
                );
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
            
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                yield return (potential1.GetValue() + potential2.GetValue()).EqualsVal(new TValue(180)).AddReferences(
                    pool.AvailableDetails.EnsuredUnorderedGet(seg1, Relation.PARALLEL, seg2),
                    potential1.GetValueDetail(), potential2.GetValueDetail()
                );
        }
    }



    /*
                                                                                                                                                    
          ██████████ 
        █:::████::::█                                                                           ████         
        █:::█    ███                                                                            █::█          
         █::████       █████████     █████  ██   █████  ███████    ████████   ████ ███████  █████::█████    
          ███::::██  █::█████:::██ █:::::::::██:::::::█::::::::█ █:::████:::███:::::::::::█ █::::::::::█    
                █:::██:::█   █::::██::█   █::█ █:::███::::███:::██::::█  █::::█  █:::████:::█    █::█        
        ███     █:::██::█████████  █::█   █::█ █::█   █::█   █::██:::████████    █::█    █::█    █::█        
        █::██████:::██::::█        █::::███::█ █::█   █::█   █::██:::::█         █::█    █::█    █:::██::█
        █:::::::::██   █::::::::█    ::::::::█ █::█   █::█   █::█  █::::::::█    █::█    █::█     █:::::█
         ██████████     █████████    █████:::█ ████   ████   ████   ████████     ████    ████      █████  
                                   ███    █::█                                                                       
                                    █:::█::::█                                                                       
                                     █::::███                                                                          
    */


    // public static IEnumerable<Detail> LargerAngleBiggerSideAt(TTriangle triangle)

    [Reason(Reason.TRIANGLE_SUM_TWO_SIDES_LARGER_THIRD)]
    public static IEnumerable<Detail> TriangleSumTwoSidesLargerThird(TTriangle triangle)
    {
        return new[]
        {
            triangle.V1V2.Smaller(triangle.V1V3.GetValue() + triangle.V2V3.GetValue()).AddReferences(triangle.V1V3.GetValueDetail(), triangle.V2V3.GetValueDetail()),
            triangle.V1V3.Smaller(triangle.V1V2.GetValue() + triangle.V2V3.GetValue()).AddReferences(triangle.V1V2.GetValueDetail(), triangle.V2V3.GetValueDetail()),
            triangle.V2V3.Smaller(triangle.V1V2.GetValue() + triangle.V1V3.GetValue()).AddReferences(triangle.V1V2.GetValueDetail(), triangle.V1V3.GetValueDetail())
        };
    }

    [Reason(Reason.MIDSEGMENT_PARALLEL_OTHER_TRIANGLE_SIDE)]
    public static IEnumerable<Detail> ParallelizeMidSegmentToSide(TTriangle triangle)
    {
        var midSegments = triangle.GetMidSegmentsWithOpposites();
        return midSegments.Select(x => x.midSegment.Parallel(x.opposite).AddReferences(triangle.ParentPool.AvailableDetails.EnsuredGet(x.midSegment, Relation.MIDSEGMENT, triangle))).ToArray();
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
                            triangle.ParentPool.AvailableDetails.EnsuredUnorderedGet(bisector, Relation.PARALLEL, otherSide)
                        );
                }
            }
        }
    }

    [Reason(Reason.CORRESPONDING_ANGLES_EQUAL_LINES_PARALLEL)]
    public static IEnumerable<Detail> ParallelizeLines_A(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:
                
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() == potential2.GetValue())
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetEqualityDetail(potential2));
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
                
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() == potential2.GetValue())
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetEqualityDetail(potential2));
        }
    }

    [Reason(Reason.ALTERNATING_ANGLES_EQUAL_LINES_PARALLEL)]
    public static IEnumerable<Detail> ParallelizeLines_B(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:

                              V2
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
                          V1
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() == potential2.GetValue())
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetEqualityDetail(potential2));
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
            
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() == potential2.GetValue())
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetEqualityDetail(potential2));
        }
    }

    [Reason(Reason.COINTERIOR_ANGLES_180_LINES_PARALLEL)]
    public static IEnumerable<Detail> ParallelizeLines_C(TSegment seg1, TSegment seg2, TSegment intersector)
    {
        TokenHelpers.Validate(seg1, seg2, intersector);
        var pool = intersector.ParentPool;

        var interSeg1Intersector = intersector.GetOrCreateIntersectionPoint(seg1);
        var interSeg2Intersector = intersector.GetOrCreateIntersectionPoint(seg2);

        if (pool.QuestionDiagram.WillPotentiallyIntersect((seg1.V1.Id, seg2.V1.Id), (seg1.V2.Id, seg2.V2.Id)))
        {
            /*
             This means the segments V1 & v2 are "aligned" with each █████:

                              V2
                              /
                   V1--------/----V2
                            /
                      V1---/-------------V2
                          V1
             */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() + potential2.GetValue() == new TValue(180))
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetValueDetail(), potential2.GetValueDetail());
        }
        else
        {
            /*
             This means the segments V1 & v2 are not "aligned" with each █████:
            
                              V2
                              /
                   V2--------/----V1
                            /
                      V1---/-------------V2
                          V1
            */

            (TAngle, TAngle)[] potentialPairs = new[] {
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V1), interSeg2Intersector.GetAngle(intersector.V2, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V1, seg1.V2), interSeg2Intersector.GetAngle(intersector.V2, seg2.V1)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V1), interSeg2Intersector.GetAngle(intersector.V1, seg2.V2)),
                (interSeg1Intersector.GetAngle(intersector.V2, seg1.V2), interSeg2Intersector.GetAngle(intersector.V1, seg2.V1))
            };
            foreach (var (potential1, potential2) in potentialPairs)
                if (potential1.GetValue() + potential2.GetValue() == new TValue(180))
                    yield return seg1.Parallel(seg2).AddReferences(potential1.GetValueDetail(), potential2.GetValueDetail());
        }
    }

    /*                                                                                                                             

        █████████                                                             ███      █████          
        █:::::::█                                                          █::::█      █:::█          
          █:::█             ██████████    ████ ███████      ██████   ███████::::████    █::█ ████     
          █:::█          █::::█████:::::███:::::::::::██  █::::::::::::██:::::::::::    █:::::::::██  
          █:::█         █:::::█   █::::::█  █::::███::::██::::█   █:::█    █::::█       █::::█  █::::█
          █:::█         █::::███████████    █:::█   █:::██::::█   █:::█    █::::█       █:::█    █:::█
        ██:::::█████:::██::::::█            █:::█   █:::██::::::███:::█    █:::::██:::█ █:::█    █:::█
        █::::::::::::::█  ██:::::::::::█    █:::█   █:::█  █::::::::::█      ██::::::██ █:::█    █:::█
        ████████████████    ████████████    █████   █████   ██████::::█        ██████   █████    █████
                                                         █████    █:::█                               
                                                          █:::::█:::::█                               
                                                            ███::::███                                
    */

    // public static IEnumerable<Detail> LargerAngleBiggerSideAt(TTriangle triangle)

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
                    if (intersector.IsParallel(otherSide) && intersector.GetValue() == otherSide.GetValue() / 2)
                        yield return intersector.MidSegment(triangle, otherSide).AddReferences(
                            triangle.ParentPool.AvailableDetails.EnsuredUnorderedGet(intersector, Relation.INTERSECTS, otherSide),
                            triangle.ParentPool.AvailableDetails.EnsuredUnorderedGet(intersector, Relation.PARALLEL, otherSide),
                            intersector.GetEqualityDetail(otherSide.GetValue() / 2)
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

    [Reason(Reason.ISOSCELES_PERPENDICULAR_ANGLEBISECTOR_BISECTOR)]
    public static IEnumerable<Detail> IsoscelesProperties_A(TTriangle triangle)
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
        TSegment? potential1 = all.GetMany(Relation.BISECTS, triangle.V1V2V3).Select(x => (TSegment)x.Left).Intersect(all.UnorderedGetMany(triangle.V1V3, Relation.PERPENDICULAR).Select(x => (TSegment)x.Left)).FirstOrDefault();
        TSegment? potential2 = all.GetMany(Relation.BISECTS, triangle.V2V1V3).Select(x => (TSegment)x.Left).Intersect(all.UnorderedGetMany(triangle.V2V3, Relation.PERPENDICULAR).Select(x => (TSegment)x.Left)).FirstOrDefault();
        TSegment? potential3 = all.GetMany(Relation.BISECTS, triangle.V1V3V2).Select(x => (TSegment)x.Left).Intersect(all.UnorderedGetMany(triangle.V1V2, Relation.PERPENDICULAR).Select(x => (TSegment)x.Right)).FirstOrDefault();

        if (potential1 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V2V3)).AddReferences(all.EnsuredGet(potential1, Relation.BISECTS, triangle.V1V2V3), all.EnsuredUnorderedGet(potential1, Relation.PERPENDICULAR, triangle.V1V3));
        }
        if (potential2 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V1V3)).AddReferences(all.EnsuredGet(potential2, Relation.BISECTS, triangle.V2V1V3), all.EnsuredUnorderedGet(potential2, Relation.PERPENDICULAR, triangle.V2V3));
        }
        if (potential3 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V3, triangle.V2V3)).AddReferences(all.EnsuredGet(potential3, Relation.BISECTS, triangle.V1V3V2), all.EnsuredUnorderedGet(potential3, Relation.PERPENDICULAR, triangle.V1V2));
        }
    }

    [Reason(Reason.TRIANGLE_ANGLEBISECTOR_BISECTOR_IS_ISOSCELES)]
    public static IEnumerable<Detail> IsTriangleIsosceles_B(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;
        TSegment? potential1 = all.GetMany(Relation.BISECTS, triangle.V1V2V3).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V1V3).Select(x => (TSegment)x.Right)).FirstOrDefault();
        TSegment? potential2 = all.GetMany(Relation.BISECTS, triangle.V2V1V3).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V2V3).Select(x => (TSegment)x.Right)).FirstOrDefault();
        TSegment? potential3 = all.GetMany(Relation.BISECTS, triangle.V1V3V2).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V1V2).Select(x => (TSegment)x.Right)).FirstOrDefault();

        if (potential1 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V2V3)).AddReferences(all.EnsuredGet(potential1, Relation.BISECTS, triangle.V1V2V3), all.EnsuredGet(potential1, Relation.BISECTS, triangle.V1V3));
        }
        if (potential2 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V1V3)).AddReferences(all.EnsuredGet(potential2, Relation.BISECTS, triangle.V2V1V3), all.EnsuredGet(potential2, Relation.BISECTS, triangle.V2V3));
        }
        if (potential3 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V3, triangle.V2V3)).AddReferences(all.EnsuredGet(potential3, Relation.BISECTS, triangle.V1V3V2), all.EnsuredGet(potential3, Relation.BISECTS, triangle.V1V2));
        }
    }

    [Reason(Reason.TRIANGLE_PERPENDICULAR_BISECTOR_IS_ISOSCELES)]
    public static IEnumerable<Detail> IsTriangleIsosceles_C(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;
        TSegment? potential1 = all.UnorderedGetMany(triangle.V1V3, Relation.PERPENDICULAR).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V1V3).Select(x => (TSegment)x.Right)).FirstOrDefault();
        TSegment? potential2 = all.UnorderedGetMany(triangle.V2V3, Relation.PERPENDICULAR).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V2V3).Select(x => (TSegment)x.Right)).FirstOrDefault();
        TSegment? potential3 = all.UnorderedGetMany(triangle.V1V2, Relation.PERPENDICULAR).Select(x => (TSegment)x.Left).Intersect(all.GetMany(Relation.BISECTS, triangle.V1V2).Select(x => (TSegment)x.Right)).FirstOrDefault();

        if (potential1 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V2V3)).AddReferences(all.EnsuredUnorderedGet(potential1, Relation.PERPENDICULAR, triangle.V1V3), all.EnsuredGet(potential1, Relation.BISECTS, triangle.V1V3));
        }
        if (potential2 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V2, triangle.V1V3)).AddReferences(all.EnsuredUnorderedGet(potential2, Relation.PERPENDICULAR, triangle.V2V3), all.EnsuredGet(potential2, Relation.BISECTS, triangle.V2V3));
        }
        if (potential3 != null)
        {
            yield return triangle.MarkIsosceles((triangle.V1V3, triangle.V2V3)).AddReferences(all.EnsuredUnorderedGet(potential3, Relation.PERPENDICULAR, triangle.V1V2), all.EnsuredGet(potential3, Relation.BISECTS, triangle.V1V2));
        }
    }

    [Reason(Reason.TRIANGLE_CONGRUENCY_S_A_S)]
    public static IEnumerable<Detail> TriangleCongruencyVia_SAS(TTriangle triangle1, TTriangle triangle2)
    {
        TokenHelpers.Validate(triangle1, triangle2);
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

        foreach ((TAngle a1, TAngle a2) in equals)
        {
            if (a1.Segment1 == null || a1.Segment2 == null || a2.Segment1 == null || a2.Segment2 == null) continue;
            if (a1.Segment1.GetValue() == a2.Segment1.GetValue() && a1.Segment2.GetValue() == a2.Segment2.GetValue())
            {
                yield return triangle1.Congruent(triangle2, (a1.Segment1, a2.Segment1), (a1, a2), (a1.Segment2, a2.Segment2)).AddReferences(
                    a1.Segment1.GetEqualityDetail(a2.Segment1),
                    a1.GetEqualityDetail(a2),
                    a1.Segment2.GetEqualityDetail(a2.Segment2)

                );
            }
            else if (a1.Segment1.GetValue() == a2.Segment2.GetValue() && a1.Segment2.GetValue() == a2.Segment1.GetValue())
            {
                yield return triangle1.Congruent(triangle2, (a1.Segment1, a2.Segment2), (a1, a2), (a1.Segment2, a2.Segment1)).AddReferences(
                    a1.Segment1.GetEqualityDetail(a2.Segment2),
                    a1.GetEqualityDetail(a2),
                    a1.Segment2.GetEqualityDetail(a2.Segment1)
                );
            }
        }
    }

    [Reason(Reason.TRIANGLE_CONGRUENCY_A_S_A)]
    public static IEnumerable<Detail> TriangleCongruencyVia_ASA(TTriangle triangle1, TTriangle triangle2)
    {
        TokenHelpers.Validate(triangle1, triangle2);
        var sidesOfT1 = triangle1.Sides;
        var sidesOfT2 = triangle2.Sides;
        var equals = new List<(TSegment, TSegment)>();
        foreach (var side1 in sidesOfT1)
        {
            foreach (var side2 in sidesOfT2)
            {
                if (side1.GetValue() == side2.GetValue())
                    equals.Add((side1, side2));
            }
        }

        foreach ((TSegment s1, TSegment s2) in equals)
        {
            if (triangle1.GetAngleOf(s1.V1).GetValue() == triangle2.GetAngleOf(s2.V1).GetValue() && triangle1.GetAngleOf(s1.V2).GetValue() == triangle2.GetAngleOf(s2.V2).GetValue())
            {
                yield return triangle1.Congruent(triangle2, (triangle1.GetAngleOf(s1.V1), triangle2.GetAngleOf(s2.V1)), (s1, s2), (triangle1.GetAngleOf(s1.V2), triangle2.GetAngleOf(s2.V2))).AddReferences(
                    triangle1.GetAngleOf(s1.V1).GetEqualityDetail(triangle2.GetAngleOf(s2.V1)),
                    s1.GetEqualityDetail(s2),
                    triangle1.GetAngleOf(s1.V2).GetEqualityDetail(triangle2.GetAngleOf(s2.V2))

                );
            }
            else if (triangle1.GetAngleOf(s1.V1).GetValue() == triangle2.GetAngleOf(s2.V2).GetValue() && triangle1.GetAngleOf(s1.V2).GetValue() == triangle2.GetAngleOf(s2.V1).GetValue())
            {
                yield return triangle1.Congruent(triangle2, (triangle1.GetAngleOf(s1.V1), triangle2.GetAngleOf(s2.V2)), (s1, s2), (triangle1.GetAngleOf(s1.V2), triangle2.GetAngleOf(s2.V1))).AddReferences(
                    triangle1.GetAngleOf(s1.V1).GetEqualityDetail(triangle2.GetAngleOf(s2.V2)),
                    s1.GetEqualityDetail(s2),
                    triangle1.GetAngleOf(s1.V2).GetEqualityDetail(triangle2.GetAngleOf(s2.V1))
                );
            }
        }
    }

    [Reason(Reason.TRIANGLE_CONGRUENCY_S_S_S)]
    public static IEnumerable<Detail> TriangleCongruencyVia_SSS(TTriangle triangle1, TTriangle triangle2)
    {
        TokenHelpers.Validate(triangle1, triangle2);
        var equalPairs = new List<(TSegment, TSegment)>();
        foreach (var side1 in triangle1.Sides)
        {
            foreach (var side2 in triangle2.Sides)
            {
                if (side1.GetValue() != side2.GetValue()) return E();
                equalPairs.Add((side1, side2));
            }
        }

        var uniquePairs = new List<(TSegment, TSegment)>();
        if (equalPairs.Count > 3)
        {
            foreach (var pair in equalPairs)
            {
                bool isUnique = true;

                foreach (var uniquePair in uniquePairs)
                {
                    if (pair.Item1 == uniquePair.Item1 || pair.Item1 == uniquePair.Item2 ||
                        pair.Item2 == uniquePair.Item1 || pair.Item2 == uniquePair.Item2)
                    {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique) uniquePairs.Add(pair);
                if (uniquePairs.Count == 3) break;
            }
        }

        return new[]
        {
            triangle1.Congruent(triangle2, uniquePairs[0], uniquePairs[1], uniquePairs[2]).AddReferences(
                uniquePairs[0].Item1.GetEqualityDetail(uniquePairs[0].Item2),
                uniquePairs[1].Item1.GetEqualityDetail(uniquePairs[1].Item2),
                uniquePairs[2].Item1.GetEqualityDetail(uniquePairs[2].Item2)
            )
        };
    }

    // TODO: revisit this, > and < operators on values are a little complicated
    [Reason(Reason.TRIANGLE_CONGRUENCY_S_S_A)]
    public static IEnumerable<Detail> TriangleCongruencyVia_SSA(TTriangle triangle1, TTriangle triangle2)
    {
        TokenHelpers.Validate(triangle1, triangle2);
        var all = triangle1.ParentPool.AvailableDetails;
        TAngle big1 = null!, big2 = null!;

        foreach (var a in triangle1.GetAngles())
        {
            if (big1 == null) big1 = a;
            else if (a.GetValue() > big1.GetValue()) big1 = a;
        }
        foreach (var a in triangle2.GetAngles())
        {
            if (big2 == null) big2 = a;
            else if (a.GetValue() > big2.GetValue()) big2 = a;
        }

        TSegment opp1 = triangle1.GetOppositeSegment(big1), opp2 = triangle2.GetOppositeSegment(big2);

        if (big1.GetValue() != big2.GetValue()) return E();
        if (opp1.GetValue() != opp2.GetValue()) return E();

        var sides1 = triangle1.Sides.Except(opp1).Select(s => s.GetValue()).ToArray();
        var sides2 = triangle2.Sides.Except(opp2).Select(s => s.GetValue()).ToArray();
        var s1 = triangle1.Sides.Except(opp1).ToArray();
        var s2 = triangle2.Sides.Except(opp2).ToArray();
        (TSegment, TSegment) otherSides = (null!, null!);
        if (sides1[0] == sides2[0]) otherSides = (s1[0], s2[0]);
        else if (sides1[0] == sides2[1]) otherSides = (s1[0], s2[1]);
        else if (sides1[1] == sides2[0]) otherSides = (s1[1], s2[0]);
        else if (sides1[1] == sides2[1]) otherSides = (s1[1], s2[1]);
        else return E();

        return new[]
        {
            triangle1.Congruent(triangle2, otherSides, (opp1, opp2), (big1, big2)).AddReferences(
                otherSides.Item1.GetEqualityDetail(otherSides.Item2),
                opp1.GetEqualityDetail(opp2),
                big1.GetEqualityDetail(big2)
            )
        };
    }

    [Reason(Reason.TRIANGLE_BISECTORS_INTERSECT)]
    public static IEnumerable<Detail> TriangleProperties_A(TTriangle triangle)
    {
        var all = triangle.ParentPool.AvailableDetails;

        var bisectors = triangle.Sides
            .Select(s => (s.GetBisectors(), triangle.GetOppositeVertex(s), s))
            .Select(triple => (triple.Item1.Where(bisector => bisector.Parts.Contains(triple.Item2)), triple.Item3))
            .Select(pair => from bisector in pair.Item1 select (bisector, side: pair.Item2))
            .SelectMany(item => item);

        foreach (var pair in bisectors)
        {
            foreach (var pair2 in bisectors.Except(pair))
            {
                yield return pair.bisector.Intersects(pair2.bisector).AddReferences(
                    all.EnsuredGet(pair.bisector, Relation.BISECTS, pair.side),
                    all.EnsuredGet(pair2.bisector, Relation.BISECTS, pair2.side)
                );
            }
        }
    }
    /*                                                                            
                                                                            ████████
           █████████████                                                    █::::::█
         ██:::::::::::::██                                                  █::::::█
        █::::::█   █::::::█ ██████    ██████   ██████████████       █████████:::::█ 
        █:::::█     █:::::█ █::::█    █::::█   █████ ████:::::█  █::::::::::::::::█ 
        █:::::█     █:::::█ █::::█    █::::█          ███:::::█ █::::::█    █:::::█ 
        █:::::█  ████:::::█ █::::█    █::::█   █::::████::::::█ █:::::█     █:::::█ 
        █:::::::██::::::::█ █:::::::::::::::███::::█    █:::::█ █::::::█████::::::██
           ██:::::::::::█     ██::::::::██:::█ █::::::::::██:::█  █:::::::::███::::█
             █████████:::██     ████████  ████  ██████████  ████   █████████   █████
                     █::::█
                      ██████                                                        
    */

    [Reason(Reason.KITE_MAIN_DIAGONAL_BISECTS_ANGLE_BISECTS_DIAGONAL)]
    public static IEnumerable<Detail> KiteProperties_A(TQuad quad)
    {

        var details = new List<Detail>();

        if (!quad.ParentPool.AvailableDetails.Has(quad, Relation.QUAD_KITE)) return E();
        var kiteDetail = quad.ParentPool.AvailableDetails.EnsuredGet(quad, Relation.QUAD_KITE);

        TVertex o1 = ((TAngle)kiteDetail.SideProducts[0]).Origin, o2 = ((TAngle)kiteDetail.SideProducts[1]).Origin;
        var createAuxDetail = false;
        if (!o1.Relations.Contains(o2)) createAuxDetail = true;
        var d1 = o1.GetOrCreateSegment(o2);
        if (createAuxDetail) details.Add(o1.Connect(o2).MarkAuxiliary());

        var otherVertices = quad.Vertices.Except(o1, o2).Cast<TVertex>().ToArray();

        TVertex v1 = otherVertices[0], v2 = otherVertices[1];
        createAuxDetail = false;
        if (!v1.Relations.Contains(v2)) createAuxDetail = true;
        var d2 = v1.GetOrCreateSegment(v2);
        if (createAuxDetail) details.Add(v1.Connect(v2).MarkAuxiliary());

        return new[] {
            d1.Bisects(d2).AddReferences(kiteDetail),
            d1.Bisects((TAngle)kiteDetail.SideProducts[0]).AddReferences(kiteDetail),
            d1.Bisects((TAngle)kiteDetail.SideProducts[1]).AddReferences(kiteDetail)
        };
    }

    [Reason(Reason.PARALLELOGRAM_OPPOSITE_ANGLES_EQUAL)]
    public static IEnumerable<Detail> ParallelogramProperties_A(TQuad parallelogram)
    {
        if (!parallelogram.ParentPool.AvailableDetails.Has(parallelogram, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = parallelogram.ParentPool.AvailableDetails.EnsuredGet(parallelogram, Relation.QUAD_PARALLELOGRAM);
        return new[]
        {
            parallelogram.V1V2V3.EqualsVal(parallelogram.V1V4V3).AddReferences(parallelogramDetail),
            parallelogram.V2V1V4.EqualsVal(parallelogram.V2V3V4).AddReferences(parallelogramDetail),
        };
    }

    [Reason(Reason.PARALLELOGRAM_OPPOSITE_SIDES_EQUAL)]
    public static IEnumerable<Detail> ParallelogramProperties_B(TQuad parallelogram)
    {
        if (!parallelogram.ParentPool.AvailableDetails.Has(parallelogram, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = parallelogram.ParentPool.AvailableDetails.EnsuredGet(parallelogram, Relation.QUAD_PARALLELOGRAM);
        return new[]
        {
            parallelogram.V1V2.EqualsVal(parallelogram.V3V4).AddReferences(parallelogramDetail),
            parallelogram.V2V3.EqualsVal(parallelogram.V1V4).AddReferences(parallelogramDetail),
        };
    }

    [Reason(Reason.PARALLELOGRAM_DIAGONALS_BISECT_EACH_OTHER)]
    public static IEnumerable<Detail> ParallelogramProperties_C(TQuad parallelogram)
    {
        if (!parallelogram.ParentPool.AvailableDetails.Has(parallelogram, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = parallelogram.ParentPool.AvailableDetails.EnsuredGet(parallelogram, Relation.QUAD_PARALLELOGRAM);
        return new[]
        {
            parallelogram.V1.GetOrCreateSegment(parallelogram.V3).EqualsVal(parallelogram.V2.GetOrCreateSegment(parallelogram.V4)).AddReferences(parallelogramDetail)
        };
    }

    [Reason(Reason.QUAD_OPPOSITE_ANGLES_EQUAL_PARALLELOGRAM)]
    public static IEnumerable<Detail> IsQuadParallelogram_A(TQuad quad)
    {
        if (quad.V1V2V3.GetValue() == quad.V1V4V3.GetValue() && quad.V2V1V4.GetValue() == quad.V2V3V4.GetValue())
            return new[] {
                quad.MarkParallelogram().AddReferences(
                    quad.V1V2V3.GetEqualityDetail(quad.V1V4V3),
                    quad.V2V1V4.GetEqualityDetail(quad.V2V3V4)
                )
            };
        return E();
    }

    [Reason(Reason.QUAD_OPPOSITE_SIDES_EQUAL_PARALLELOGRAM)]
    public static IEnumerable<Detail> IsQuadParallelogram_B(TQuad quad)
    {
        if (quad.V1V2.GetValue() == quad.V3V4.GetValue() && quad.V2V3.GetValue() == quad.V1V4.GetValue())
            return new[] {
                quad.MarkParallelogram().AddReferences(
                    quad.V1V2.GetEqualityDetail(quad.V3V4),
                    quad.V2V3.GetEqualityDetail(quad.V1V4)
                )
            };
        return E();
    }

    [Reason(Reason.QUAD_OPPOSITE_PAIR_PARALLEL_EQUAL_PARALLELOGRAM)]
    public static IEnumerable<Detail> IsQuadParallelogram_C(TQuad quad)
    {
        if (quad.V1V2.GetValue() == quad.V3V4.GetValue() && quad.V1V2.IsParallel(quad.V3V4))
            return new[] {
                quad.MarkParallelogram().AddReferences(
                    quad.V1V2.GetEqualityDetail(quad.V3V4),
                    quad.ParentPool.AvailableDetails.EnsuredUnorderedGet(quad.V1V2, Relation.PARALLEL, quad.V3V4)
                )
            };
        if (quad.V2V3.GetValue() == quad.V1V4.GetValue() && quad.V2V3.IsParallel(quad.V1V4))
            return new[] {
                quad.MarkParallelogram().AddReferences(
                    quad.V2V3.GetEqualityDetail(quad.V1V4),
                    quad.ParentPool.AvailableDetails.EnsuredUnorderedGet(quad.V2V3, Relation.PARALLEL, quad.V1V4)
                )
            };
        return E();
    }

    [Reason(Reason.QUAD_DIAGONALS_BISECT_EACH_OTHER_PARALLELOGRAM)]
    public static IEnumerable<Detail> IsQuadParallelogram_D(TQuad quad)
    {
        var diagonal1 = quad.V1.GetOrCreateSegment(quad.V3);
        var diagonal2 = quad.V2.GetOrCreateSegment(quad.V4);
        if (diagonal1.IsBisecting(diagonal2) && diagonal2.IsBisecting(diagonal1))    
            return new[] {
                quad.MarkParallelogram().AddReferences(
                    quad.ParentPool.AvailableDetails.EnsuredGet(diagonal1, Relation.BISECTS, diagonal2),
                    quad.ParentPool.AvailableDetails.EnsuredGet(diagonal2, Relation.BISECTS, diagonal1)
                )
            };
        return E();
    }

    [Reason(Reason.RHOMBUS_DIAGONALS_BISECT_EACH_OTHER)]
    public static IEnumerable<Detail> RhombusProperties_A(TQuad rhombus)
    {
        if (!rhombus.ParentPool.AvailableDetails.Has(rhombus, Relation.QUAD_RHOMBUS)) return E();
        var rhombusDetail = rhombus.ParentPool.AvailableDetails.EnsuredGet(rhombus, Relation.QUAD_RHOMBUS);

        return new[]
        {
            rhombus.V1.GetOrCreateSegment(rhombus.V3).Bisects(rhombus.V2.GetOrCreateSegment(rhombus.V4)).AddReferences(rhombusDetail),
            rhombus.V2.GetOrCreateSegment(rhombus.V4).Bisects(rhombus.V1.GetOrCreateSegment(rhombus.V3)).AddReferences(rhombusDetail)
        };
    }

    [Reason(Reason.PARALLELOGRAM_DIAGONAL_ANGLE_BISECTOR_RHOMBUS)]
    public static IEnumerable<Detail> IsParallelogramRhombus_A(TQuad quad)
    {
        if (!quad.ParentPool.AvailableDetails.Has(quad, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = quad.ParentPool.AvailableDetails.EnsuredGet(quad, Relation.QUAD_PARALLELOGRAM);
        TSegment diagonal1 = quad.V1.GetOrCreateSegment(quad.V3), diagonal2 = quad.V2.GetOrCreateSegment(quad.V4);
        if (diagonal1.IsBisecting(quad.V2V1V4)) return new[] { quad.MarkRhombus().AddReferences(parallelogramDetail, quad.ParentPool.AvailableDetails.EnsuredGet(diagonal1, Relation.BISECTS, quad.V2V1V4)) };
        if (diagonal1.IsBisecting(quad.V2V3V4)) return new[] { quad.MarkRhombus().AddReferences(parallelogramDetail, quad.ParentPool.AvailableDetails.EnsuredGet(diagonal1, Relation.BISECTS, quad.V2V3V4)) };
        if (diagonal2.IsBisecting(quad.V1V2V3)) return new[] { quad.MarkRhombus().AddReferences(parallelogramDetail, quad.ParentPool.AvailableDetails.EnsuredGet(diagonal2, Relation.BISECTS, quad.V1V2V3)) };
        if (diagonal2.IsBisecting(quad.V1V4V3)) return new[] { quad.MarkRhombus().AddReferences(parallelogramDetail, quad.ParentPool.AvailableDetails.EnsuredGet(diagonal2, Relation.BISECTS, quad.V1V4V3)) };
        return E();
    }

    [Reason(Reason.RHOMBUS_DIAGONALS_PERPENDICULAR)]
    public static IEnumerable<Detail> RhombusProperties_B(TQuad rhombus)
    {
        if (!rhombus.ParentPool.AvailableDetails.Has(rhombus, Relation.QUAD_RHOMBUS)) return E();
        var rhombusDetail = rhombus.ParentPool.AvailableDetails.EnsuredGet(rhombus, Relation.QUAD_RHOMBUS);
        return new[] {
            rhombus.V1.GetOrCreateSegment(rhombus.V3).Perpendicular(rhombus.V2.GetOrCreateSegment(rhombus.V4)).AddReferences(rhombusDetail)
        };
    }

    [Reason(Reason.PARALLELOGRAM_DIAGONALS_PERPENDICULAR_RHOMBUS)]
    public static IEnumerable<Detail> IsParallelogramRhombus_B(TQuad quad)
    {
        if (!quad.ParentPool.AvailableDetails.Has(quad, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = quad.ParentPool.AvailableDetails.EnsuredGet(quad, Relation.QUAD_PARALLELOGRAM);
        TSegment diagonal1 = quad.V1.GetOrCreateSegment(quad.V3), diagonal2 = quad.V2.GetOrCreateSegment(quad.V4);
        if (diagonal1.IsPerpendicular(diagonal2)) return new[] { quad.MarkRhombus().AddReferences(parallelogramDetail, quad.ParentPool.AvailableDetails.EnsuredUnorderedGet(diagonal1, Relation.PERPENDICULAR, diagonal2)) };
        return E();
    }

    [Reason(Reason.RECTANGLE_DIAGONALS_EQUAL)]
    public static IEnumerable<Detail> RectangleProperties_A(TQuad rectangle)
    {
        if (!rectangle.ParentPool.AvailableDetails.Has(rectangle, Relation.QUAD_RECTANGLE)) return E();
        var rectangleDetail = rectangle.ParentPool.AvailableDetails.EnsuredGet(rectangle, Relation.QUAD_RECTANGLE);
        return new[] {
            rectangle.V1.GetOrCreateSegment(rectangle.V3).EqualsVal(rectangle.V2.GetOrCreateSegment(rectangle.V4)).AddReferences(rectangleDetail)
        };
    }

    [Reason(Reason.PARALLELOGRAM_DIAGONALS_EQUAL_RECTANGLE)]
    public static IEnumerable<Detail> IsParallelogramRectangle_A(TQuad quad)
    {
        if (!quad.ParentPool.AvailableDetails.Has(quad, Relation.QUAD_PARALLELOGRAM)) return E();
        var parallelogramDetail = quad.ParentPool.AvailableDetails.EnsuredGet(quad, Relation.QUAD_PARALLELOGRAM);
        TSegment diagonal1 = quad.V1.GetOrCreateSegment(quad.V3), diagonal2 = quad.V2.GetOrCreateSegment(quad.V4);
        if (diagonal1.GetValue() == diagonal2.GetValue()) return new[] { quad.MarkRectangle().AddReferences(parallelogramDetail, diagonal1.GetEqualityDetail(diagonal2)) };
        return E();
    }

    [Reason(Reason.ISOSTRAPEZOID_BASE_ANGLES_EQUAL)]
    public static IEnumerable<Detail> IsoscelesTrapezoidProperties_A(TQuad isoscelesTrapezoid)
    {
        if (!isoscelesTrapezoid.ParentPool.AvailableDetails.Has(isoscelesTrapezoid, Relation.QUAD_ISOSCELES_TRAPEZOID)) return E();
        var isoscelesTrapezoidDetail = isoscelesTrapezoid.ParentPool.AvailableDetails.EnsuredGet(isoscelesTrapezoid, Relation.QUAD_ISOSCELES_TRAPEZOID);
        
        var bases = isoscelesTrapezoid.GetTrapezoidBases();
        var pair1 = (isoscelesTrapezoid.GetAngle(bases.First().V1), isoscelesTrapezoid.GetAngle(bases.First().V2));
        var pair2 = (isoscelesTrapezoid.GetAngle(bases.Last().V1), isoscelesTrapezoid.GetAngle(bases.Last().V2));

        return new[] {
            pair1.Item1.EqualsVal(pair1.Item2).AddReferences(isoscelesTrapezoidDetail),
            pair2.Item1.EqualsVal(pair2.Item2).AddReferences(isoscelesTrapezoidDetail)
        };
    }

    [Reason(Reason.TRAPEZOID_BASE_ANGLES_EQUAL_ISOSTRAPEZOID)]
    public static IEnumerable<Detail> IsTrapezoidIsoscelesTrapezoid_A(TQuad trapezoid)
    {
        if (!trapezoid.ParentPool.AvailableDetails.Has(trapezoid, Relation.QUAD_TRAPEZOID)) return E();
        var trapezoidDetail = trapezoid.ParentPool.AvailableDetails.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID);

        IEnumerable<(TSegment segment, TAngle angle1, TAngle angle2)> baseAngles = trapezoid.GetTrapezoidBases().Select(x => (x, trapezoid.GetAngle(x.V1), trapezoid.GetAngle(x.V1)));

        foreach (var (segment, angle1, angle2) in baseAngles) {
            if (angle1.GetValue() == angle2.GetValue()) {
                return new [] {
                    trapezoid.MarkIsoscelesTrapezoid((trapezoid.GetTrapezoidBases().First(), trapezoid.GetTrapezoidBases().Last())).AddReferences(trapezoidDetail, angle1.GetEqualityDetail(angle2))
                };
            }
        }
        return E();
    }

    [Reason(Reason.ISOSTRAPEZOID_DIAGONALS_EQUAL)]
    public static IEnumerable<Detail> IsoscelesTrapezoidProperties_B(TQuad isoscelesTrapezoid)
    {
        if (!isoscelesTrapezoid.ParentPool.AvailableDetails.Has(isoscelesTrapezoid, Relation.QUAD_ISOSCELES_TRAPEZOID)) return E();
        var isoscelesTrapezoidDetail = isoscelesTrapezoid.ParentPool.AvailableDetails.EnsuredGet(isoscelesTrapezoid, Relation.QUAD_ISOSCELES_TRAPEZOID);
        return new[] {
            isoscelesTrapezoid.V1.GetOrCreateSegment(isoscelesTrapezoid.V3).EqualsVal(isoscelesTrapezoid.V2.GetOrCreateSegment(isoscelesTrapezoid.V4)).AddReferences(isoscelesTrapezoidDetail)
        };
    }

    [Reason(Reason.TRAPEZOID_DIAGONALS_EQUAL_ISOSTRAPEZOID)]
    public static IEnumerable<Detail> IsTrapezoidIsoscelesTrapezoid_B(TQuad trapezoid)
    {
        if (!trapezoid.ParentPool.AvailableDetails.Has(trapezoid, Relation.QUAD_TRAPEZOID)) return E();
        var trapezoidDetail = trapezoid.ParentPool.AvailableDetails.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID);
        var bases = trapezoid.GetTrapezoidBases();
        if (trapezoid.V1.GetOrCreateSegment(trapezoid.V3).GetValue() == trapezoid.V2.GetOrCreateSegment(trapezoid.V4).GetValue()) 
            return new[] { trapezoid.MarkIsoscelesTrapezoid((bases.First(), bases.Last())).AddReferences(trapezoidDetail, trapezoid.V1.GetOrCreateSegment(trapezoid.V3).GetEqualityDetail(trapezoid.V2.GetOrCreateSegment(trapezoid.V4))) };
        return E();
    }

    [Reason(Reason.TRAPEZOID_MIDSEGMENT_PARALLEL_BASES_EQUAL_BASE_AVERAGE)]
    public static IEnumerable<Detail> TrapezoidProperties_A(TQuad trapezoid)
    {
        if (!trapezoid.ParentPool.AvailableDetails.Has(trapezoid, Relation.QUAD_TRAPEZOID)) yield break;
        var trapezoidDetail = trapezoid.ParentPool.AvailableDetails.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID);

        var trapezoidBases = trapezoid.GetTrapezoidBases();
        var midsegmentsData = trapezoid.GetMidSegmentsWithOpposites();
        foreach (var (midsegment, opposites) in midsegmentsData)
        {
            var midsegmentDetail = trapezoid.ParentPool.AvailableDetails.EnsuredGet(midsegment, Relation.MIDSEGMENT, trapezoid);
            if (trapezoidBases.ContainsMany(trapezoidBases)) {
                yield return midsegment.EqualsVal((opposites.First().GetValue() + opposites.Last().GetValue()) / 2).AddReferences(
                    trapezoidDetail,
                    midsegmentDetail,
                    opposites.First().GetValueDetail(),
                    opposites.Last().GetValueDetail()
                );
                yield return midsegment.Parallel(opposites.First()).AddReferences(
                    trapezoidDetail,
                    midsegmentDetail
                );
                yield return midsegment.Parallel(opposites.Last()).AddReferences(
                    trapezoidDetail,
                    midsegmentDetail
                );
            }
        }
    }

    [Reason(Reason.TRAPEZOID_LINE_BISECTS_SIDE_PARALLEL_BASES_IS_MIDSEGMENT)]
    public static IEnumerable<Detail> TrapezoidProperties_B(TQuad trapezoid)
    {
        var all = trapezoid.ParentPool.AvailableDetails;
        if (!all.Has(trapezoid, Relation.QUAD_TRAPEZOID)) yield break;
        var trapezoidDetail = all.EnsuredGet(trapezoid, Relation.QUAD_TRAPEZOID);

        var trapezoidBases = trapezoid.GetTrapezoidBases();
        var trapezoidSides = trapezoid.GetTrapezoidSides();

        foreach (var side in trapezoidSides)
        {
            var bisectors = side.GetBisectors();
            foreach (var bisector in bisectors)
            {
                if (bisector.IsParallel(trapezoidBases.First()) && bisector.IsParallel(trapezoidBases.Last()))
                {
                    yield return bisector.MidSegment(trapezoid, trapezoidSides.First(), trapezoidSides.Last()).AddReferences(
                        trapezoidDetail,
                        all.EnsuredGet(bisector, Relation.BISECTS, side),
                        all.EnsuredGet(bisector, Relation.PARALLEL, trapezoidBases.First()),
                        all.EnsuredGet(bisector, Relation.PARALLEL, trapezoidBases.Last())

                    );
                }
            }
        }
    }

    /*                                                                                                 

               █████████  █████                                 ███████                     
           █:::::::::::█  █████                                 █:::::█                     
          █::::█████:::█                                        █:::::█                     
         █::::█    █████ ██████ ████   ███████      ██████████  ██::::█     ████████████    
        █::::█            █:::█ █::::::::::::::█  █:::::::::::█  █::::█  █::::::█████:::::██
        █::::█            █:::█ ██:::::████:::::██::::::███:::█  █::::█ █::::::█     █:::::█
        █::::█            █:::█  █::::█          █::::█          █::::█ █::::::███████████  
          █::::█████:::█ █:::::█ █::::█          █::::::███:::█ █::::::██::::::::█          
           █:::::::::::█ █:::::█ █::::█           █:::::::::::█ █::::::█ █::::::::████████  
               █████████ ███████ ██████             ██████████ █████████    ██████████████ 
    */
}
