using Dynamically.Solver.Details;
using Dynamically.Solver.Helpers;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverSubProject.Information;

/// <summary>
/// The heart of the auto-solver - here reside the methods used to infer further information.
/// <br/>
/// Categories are sorted by the potential detail: for example, a check for similar peripheral angles on arc will be placed under "Angle", and not "Circle".
/// <br/>
/// The reason used to infer each detail is marked in every method as an attribute.
/// </summary>
public class Extractor
{
    static Detail[] E() => Array.Empty<Detail>();

    /*                                                                                                   
                                                                                        
                AAA                                         lllllll                     
               A:::A                                        l:::::l                     
              A:::::A      nnnn nnnnnnnn      gggggggg   ggg l::::l     eeeeeeeeeeee    
             A::A A::A     n::::::::::::nn  g::::::::::::::g l::::l  e::::::eeeee:::::ee
            A::A   A::A      n::::nnnn::::ng::::g     g:::g  l::::l e:::::::eeeee::::::e
           A:::::::::::A     n:::n    n:::ng::::g     g:::g  l::::l e::::::eeeeeeeeeee  
          A::A       A::A    n:::n    n:::ng::::::ggggg:::g l::::::le::::::::e          
         A::A         A::A   n:::n    n:::n g:::::::::::::g l::::::l e::::::::eeeeeeee  
        AAAA           AAAA  nnnnn    nnnnn   gggggggg::::g llllllll    eeeeeeeeeeeeee  
                                           ggggg      g:::g                             
                                            g:::::ggg:::::g                             
                                              ggg::::::ggg                              
                                                 gggggg                                 
    */

    [Reason(Reason.ADJACENT_ANGLES_180)]
    public static Detail[] EvaluateAdjacentAngles(TAngle angle)
    {
        if (!angle.HasAdjacentAngles()) return E();

        var adjacentAngles = angle.GetAdjacentAngles();
        return adjacentAngles.Select(a => a.EqualsVal(new TValue($"180\\deg - {a.GetValue().Value}", TValueKind.Equation) { ParentPool = a.ParentPool })).ToArray();
    }

    [Reason(Reason.VERTEX_ANGLES_EQUAL)]
    public static Detail[] EvaluateVertexAngle(TAngle angle)
    {
        if (!angle.HasVertexAngle()) return E();
        var vertexAngle = angle.GetVertexAngle();
        if (vertexAngle == null) return E();
        return new[] { vertexAngle.EqualsVal(angle) };
    }

    /*                                                                                                                             

        LLLLLLLLL                                                             ttt      hhhhh          
        L:::::::L                                                          t::::t      h:::h          
          L:::L             eeeeeeeeee    nnnn nnnnnnn      gggggg   gggtttt::::tttt    h::h hhhh     
          L:::L          e::::eeeee:::::een:::::::::::nn  g::::::::::::gt:::::::::::    h:::::::::hh  
          L:::L         e:::::eeeee::::::e  n::::nnn::::ng::::g   g:::g    t::::t       h::::h  h::::h
          L:::L         e::::eeeeeeeeeee    n:::n   n:::ng::::g   g:::g    t::::t       h:::h    h:::h
        LL:::::LLLLL:::Le::::::e            n:::n   n:::ng::::::ggg:::g    t:::::tt:::t h:::h    h:::h
        L::::::::::::::L  ee:::::::::::e    n:::n   n:::n  g::::::::::g      tt::::::tt h:::h    h:::h
        LLLLLLLLLLLLLLLL    eeeeeeeeeeee    nnnnn   nnnnn   gggggg::::g        tttttt   hhhhh    hhhhh
                                                         ggggg    g:::g                               
                                                          g:::::g:::::g                               
                                                            ggg::::ggg                                
    */

    /*                                                                                                                                       

        TTTTTTTTTTTTTT                iii                                             llll             
        T::TT::::TT::T                                                                l::l             
        TTT  T::T  TTTrrr   rrrrr   iiiiii   aaaaaaaaa   nnnn nnnnnnn     ggggggg  ggg l:l   eeeeeeee  
             T::T     r:::::::::::r  i:::i   aaaaaaa:::a n:::::::::::nn  g:::::::::::g l:l  e::eeee::ee
             T::T      r:::r   r:::r i:::i     aaaaa:::a   n::::nnn::::ng:::g    g::g  l:l e:::eeee:::e
             T::T      r:::r   rrrrr i:::i   aa::::::::a   n:::n   n:::ng:::g    g::g  l:l e:::::::::e 
             T::T      r:::r         i:::i a::::   a:::a   n:::n   n:::ng::::g   g::g  l:l e:::e       
           T::::::T    r:::r        i:::::ia::::aaa::::a   n:::n   n:::n g::::::::::g l:::l e::::eeeee  
           TTTTTTTT    rrrrr        iiiiiii  aaaaaaa  aaa  nnnnn   nnnnn  ggggggg:::g lllll  eeeeeeeee  
                                                                        gggg     g::g                  
                                                                         g::::gg::::g                  
                                                                          ggg:::::g                    
                                                                             ggggg                     
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
        return E();
    }

    /*                                                                            
                                                                            dddddddd
             QQQQQQQQQ                                                      d::::::d
         QQ:::::::::::::QQ                                                  d::::::d
        Q::::::O   Q::::::Q uuuuuu    uuuuuu    aaaaaaaaaaaaa       ddddddddd:::::d 
        Q:::::O     Q:::::Q u::::u    u::::u    aaaaaaaaa:::::a  d::::::::::::::::d 
        Q:::::O     Q:::::Q u::::u    u::::u      aaaaaaa:::::a d::::::d    d:::::d 
        Q:::::O  QQQQ:::::Q u::::u    u::::u   a::::aaaa::::::a d:::::d     d:::::d 
        Q:::::::QQ::::::::Q u:::::::::::::::uua::::a    a:::::a d::::::ddddd::::::dd
           QQ:::::::::::Q     uu::::::::uu:::u a::::::::::aa:::a  d:::::::::ddd::::d
             QQQQQQQQ::::QQ     uuuuuuuu  uuuu  aaaaaaaaaa  aaaa   ddddddddd   ddddd
                      QQQQQQ                                                        
    */

    /*                                                                                                 

               CCCCCCCCC  iiiii                               lllllll                     
           C:::::::::::C  iiiii                               l:::::l                     
          C::::CCCCC:::C                                      l:::::l                     
         C::::C    CCCCC iiiiii rrrr   rrrrrrr      ccccccccccc l::::l     eeeeeeeeeeee    
        C::::C            i:::i r::::::::::::::r  c:::::::::::c l::::l  e::::::eeeee:::::ee
        C::::C            i:::i rr:::::rrrr:::::rc::::::ccc:::c l::::l e::::::e     e:::::e
        C::::C            i:::i  r::::r          c::::c         l::::l e::::::eeeeeeeeeee  
          C::::CCCCC:::C i:::::i r::::r          c::::::ccc:::cl::::::le::::::::e          
           C:::::::::::C i:::::i r::::r           c:::::::::::cl::::::l e::::::::eeeeeeee  
               CCCCCCCCC iiiiiii rrrrrr             cccccccccccllllllll    eeeeeeeeeeeeee  
    */
}
