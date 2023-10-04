using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Details;

public enum Relation
{
    CONNECTED,
    ON,
    MIDDLE,
    INTERSECTS,
    BISECTS,
    TANGENT,
    PARALLEL,
    PERPENDICULAR,
    AUXILIARY,
    CIRCLE,
    ASSIGN,
    EQUALS,
    NOTEQUALS,
    LARGER,
    SMALLER,
    EQLARGER,
    EQSMALLER,
    VALUEOF,

    TRIANGLE_EQUILATERAL,
    TRIANGLE_ISOSCELES,
    TRIANGLE_RIGHT,
    TRIANGLE_ISOSCELES_RIGHT,
    TRIANGLE_SCALENE,

    QUAD_SQUARE,
    QUAD_RECTANGLE,
    QUAD_PARALLELOGRAM,
    QUAD_RHOMBUS,
    QUAD_TRAPEZOID,
    QUAD_ISOSCELES_TRAPEZOID,
    QUAD_RIGHT_TRAPEZOID,
    QUAD_KITE,
    QUAD_IRREGULAR
}
