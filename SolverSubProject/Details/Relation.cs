using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Details;

public enum Relation
{
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    CONNECTED,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Vertex, Segment | Vertex, Circle</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    ON,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Vertex, Segment</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    MIDDLE,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Vertex, Segment</item>
    ///     <item>On <c>Segment, Segment</c>, one side product - point of intersection</item>
    /// </list>
    /// </summary>
    INTERSECTS,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Angle</item>
    ///     <item>On <c>Segment, Segment</c>, one side product - point of bisection</item>
    /// </list>
    /// </summary>
    BISECTS,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Circle</item>
    ///     <item>One side product - point of intersection</item>
    /// </list>
    /// </summary>
    TANGENT,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    PARALLEL,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Triangle, Segment, Quad</item>
    ///     <item> Two scenarios:
    ///         <list type="bullet">
    ///             <item>On <c>Segment, Triangle</c>, one side product - the other side</item>
    ///             <item>On <c>Segment, Quad</c>, two side products - the intersecting sides</item>
    ///         </list>
    ///     </item>
    /// </list>
    /// </summary>
    MIDSEGMENT,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment</item>
    ///     <item>One side product - point of intersection</item>
    /// </list>
    /// </summary>
    PERPENDICULAR,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be equal to one or more elements</item>
    /// </list>
    /// </summary>
    EQUALS,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be unequal to one or more elements</item>
    /// </list>
    /// </summary>
    NOTEQUALS,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be smaller than one or more elements</item>
    /// </list>
    /// </summary>
    LARGER,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be larger than one or more elements</item>
    /// </list>
    /// </summary>
    SMALLER,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be equal/larger than one or more elements</item>
    /// </list>
    /// </summary>
    EQLARGER,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Segment, Segment | Segment, Value | Angle, Angle | Angle, Value | Angle, Arc | Arc, Arc | Arc, Value | Value, Value </item>
    ///     <item>0 or more side products - left operand can be equal/smaller than one or more elements</item>
    /// </list>
    /// </summary>
    EQSMALLER,

    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle, Triangle</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    CONGRUENT,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle, Triangle</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    SIMILAR,

    /// <summary>
    /// <list type="bullet">
    ///     <item>Vertex</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    CIRCLE,


    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    TRIANGLE_EQUILATERAL,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle</item>
    ///     <item>Two side product - the similar sides</item>
    /// </list>
    /// </summary>
    TRIANGLE_ISOSCELES,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle</item>
    ///     <item>One side product - the right angle</item>
    /// </list>
    /// </summary>
    TRIANGLE_RIGHT,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Triangle</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    TRIANGLE_SCALENE,

    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    QUAD_SQUARE,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>4 side products - the first pair of similar sides, then the second</item>
    /// </list>
    /// </summary>
    QUAD_RECTANGLE,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>4 side products - the first pair of parallel sides, then the second</item>
    /// </list>
    /// </summary>
    QUAD_PARALLELOGRAM,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    QUAD_RHOMBUS,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>2 side products - the two parallel segments</item>
    /// </list>
    /// </summary>
    QUAD_TRAPEZOID,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>4 side products - the two parallel segments, then the 2 similar sides</item>
    /// </list>
    /// </summary>
    QUAD_ISOSCELES_TRAPEZOID,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>3 side products - the two parallel segments, then the right angle</item>
    /// </list>
    /// </summary>
    QUAD_RIGHT_TRAPEZOID,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>2 side products - the two head angles</item>
    /// </list>
    /// </summary>
    QUAD_KITE,
    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    QUAD_IRREGULAR,

    /// <summary>
    /// <list type="bullet">
    ///     <item>Quad</item>
    ///     <item>No side products</item>
    /// </list>
    /// </summary>
    QUAD_CONVEX
}
