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
    public static Detail[] EvaluateAdjacentAngles(TAngle angle)
    {
        if (!angle.HasAdjacentAngles()) return E();

        var adjacentAngles = angle.GetAdjacentAngles();
        return adjacentAngles.Select(a => a.EqualsVal(new TValue($"180\\deg - {a}", TValueKind.Equation) { ParentPool = a.ParentPool })).ToArray();
    }

    [Reason(Reason.VERTEX_ANGLES_EQUAL)]
    public static Detail[] EvaluateVertexAngle(TAngle angle)
    {
        if (!angle.HasVertexAngle()) return E();
        var vertexAngle = angle.GetVertexAngle();
        if (vertexAngle == null) return E();
        return new[] { vertexAngle.EqualsVal(angle) };
    }

    // public static Detail[] BiggerSideLargerAngleAt(TTriangle triangle)

    [Reason(Reason.TRIANGLE_ANGLE_SUM_180)]
    public static Detail[] EvaluateAngleSumWithinTriangle(TTriangle triangle)
    {
        var detail1 = triangle.V2V1V3.EqualsVal(new TValue($"180\\deg - {triangle.V1V2V3} - {triangle.V1V3V2}", TValueKind.Equation) { ParentPool = triangle.ParentPool });
        var detail2 = triangle.V1V2V3.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3} - {triangle.V1V3V2}", TValueKind.Equation) { ParentPool = triangle.ParentPool });
        var detail3 = triangle.V1V3V2.EqualsVal(new TValue($"180\\deg - {triangle.V2V1V3} - {triangle.V1V2V3}", TValueKind.Equation) { ParentPool = triangle.ParentPool });

        return new[] { detail1, detail2, detail3 };
    }

    [Reason(Reason.OUTSIDE_ANGLE_EQUALS_TWO_OTHER_TRIANGLE_ANGLES)]
    public static Detail[] EvaluateOuterAnglesOfTriangle(TTriangle triangle)
    {
        var details = new Detail[6];

        foreach (TVertex vertex in triangle.GetVertices())
        {
            var oppositeSegment = triangle.GetOppositeSegment(vertex);
            var angle1 = triangle.GetAngleOf(oppositeSegment.First);
            var angle2 = triangle.GetAngleOf(oppositeSegment.Last);

            var segments = triangle.GetSegments().Except(new[] { oppositeSegment });
            foreach (var segment in segments)
            {
                details[i] = vertex.GetAngle()
            }
        }

        return details;
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

        // public static Detail[] LargerAngleBiggerSideAt(TTriangle triangle)

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

        [Reason(Reason.TRIANGLE_EQUAL_SIDES_EQUAL_ANGLES)]
    public static Detail[] EvaluateTriangleEqualSidesEqualAngles(TTriangle triangle)
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
    public static Detail[] EquateIsoscelesBaseAngles(TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Relation.TRIANGLE_ISOSCELES)) return E();
        
        var equalSides = triangle.ParentPool.AvailableDetails.EnsuredGet(triangle, Relation.TRIANGLE_ISOSCELES);
        if (equalSides.SideProducts.Count != 2) throw new Exception("Invalid Isosceles triangle - 2 equal sides must be provided");

        TAngle opposite1 = triangle.GetOppositeAngle((TSegment)equalSides.SideProducts[0]);
        TAngle opposite2 = triangle.GetOppositeAngle((TSegment)equalSides.SideProducts[1]);

        var detail = opposite1.EqualsVal(opposite2).AddReferences(equalSides);
        return new[] { detail };
    }

    [Reason(Reason.TRIANGLE_SUM_TWO_SIDES_LARGER_THIRD)]
    public static Detail[] TriangleSumTwoSidesLargerThird(TTriangle triangle)
    {
        return new[]
        {
            triangle.V1V2.Smaller(new TValue($"{triangle.V1V3} + {triangle.V2V3}", TValueKind.Equation)),
            triangle.V1V3.Smaller(new TValue($"{triangle.V1V2} + {triangle.V2V3}", TValueKind.Equation)),
            triangle.V2V3.Smaller(new TValue($"{triangle.V1V2} + {triangle.V1V3}", TValueKind.Equation))
        };
    }

    [Reason(Reason.ISOSCELES_PERPENDICUAL_ANGLEBISECTOR_BISECTOR)]
    public static Detail[] MergedAngleBisectorPerpendicularAndBisector(TTriangle triangle)
    {
        if (!triangle.ParentPool.AvailableDetails.Has(triangle, Relation.TRIANGLE_ISOSCELES)) return E();

        var headAngle = triangle.GetIsoscelesHeadAngle();
        var baseSide = triangle.GetIsoscelesBaseSide();

        var potentials = triangle.ParentPool.AvailableDetails.GetMany((Relation.BISECTS, Relation.PERPENDICULAR), baseSide).Where(x => x.Right is TSegment);
        var potentials2 = triangle.ParentPool.AvailableDetails.GetMany(baseSide, Relation.PERPENDICULAR);
        var potentials3 = triangle.ParentPool.AvailableDetails.GetMany(Relation.BISECTS, headAngle);

        var details = new List<Detail>();

        foreach(var potential in potentials)
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
    public static Detail[] IsTriangleIsosceles_A(TTriangle triangle)
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
    public static Detail[] IsTriangleIsosceles_B(TTriangle triangle)
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
    public static Detail[] IsTriangleIsosceles_C(TTriangle triangle)
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
