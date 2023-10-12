﻿namespace SolverSubProject.Information;

public enum Reason
{
    // Allowed reasons at bagrut, in order.
    GIVEN,
    /**/ ADJACENT_ANGLES_180, // 1
    /**/ VERTEX_ANGLES_EQUAL, // 2
    /**/ TRIANGLE_EQUAL_SIDES_EQUAL_ANGLES, // 3
    /**/ ISOSCELES_BASE_ANGLES_EQUAL, // 4
    /**/ TRIANGLE_SUM_TWO_SIDES_LARGER_THIRD, // 5
    /**/ ISOSCELES_PERPENDICUAL_ANGLEBISECTOR_BISECTOR, // 6
    /**/ TRIANGLE_ANGLEBISECTOR_PERPENDICULAR_IS_ISOSCELES, // 7
    /**/ TRIANGLE_ANGLEBISECTOR_BISECTOR_IS_ISOSCELES, // 8
    /**/ TRIANGLE_PERPENDICULAR_BISECTOR_IS_ISOSCELES, // 9
    TRIANGLE_BIGGER_SIDE_LARGER_ANGLE, // 10
    TRIANGLE_LARGER_ANGLE_BIGGER_SIDE, // 11
    /**/ TRIANGLE_ANGLE_SUM_180, // 12
    /**/ OUTSIDE_ANGLE_EQUALS_TWO_OTHER_TRIANGLE_ANGLES, //13
    /**/ MIDSEGMENT_PARALLEL_OTHER_TRIANGLE_SIDE, // 14
    /**/ LINE_BISECTS_SIDE_PARALLEL_OTHER_BISECTS_THIRD, // 15
    /**/ LINE_INTERSECTS_SIDES_PARALLEL_THIRD_HALF_THIRD_LENGTH_IS_MIDSEGMENT, // 16
    TRIANGLE_CONGRUENCY_S_A_S, // 17
    TRIANGLE_CONGRUENCY_A_S_A, // 18
    TRIANGLE_CONGRUENCY_S_S_S, // 19
    TRIANGLE_CONGRUENCY_S_S_A, // 20
    KITE_MAIN_DIAGONAL_BISECTS_ANGLE_BISECTS_DIAGONAL, // 21
    CORRESPONDING_ANGLES_EQUAL_LINES_PARALLEL, // 22
    ALTERNATE_ANGLES_EQUAL_LINES_PARALLEL, // 23
    COINTERIOR_ANGLES_180_LINES_PARALLEL, // 24
    CORRESPONDING_ANGLES_EQUAL, // 25 A
    ALTERNATE_ANGLES_EQUAL, // 25 B
    COINTERIOR_ANGLES_180, // 25 C
    PARALLELOGRAM_OPPOSITE_ANGLES_EQUAL, // 26
    PARALLELOGRAM_OPPOSITE_SIDES_EQUAL, // 27
    PARALLELOGRAM_DIAGONALS_BISECT_EACH_OTHER, // 28
    QUAD_OPPOSIDE_ANGLES_EQUAL_PARALLELOGRAM, // 29
    QUAD_OPPOSIDE_SIDES_EQUAL_PARALLELOGRAM, // 30
    QUAD_OPPOSITE_PAIR_PARALLEL_EQUAL_PARALLELOGRAM, // 31
    QUAD_DIAGONALS_BISECT_EACH_OTHER_PARALLELOGRAM, // 32
    RHOMBUS_DIAGONALS_BISECT_EACH_OTHER, // 33
    PARALLELOGRAM_DIAGONAL_ANGLE_BISECTOR_RHOMBUS, // 34
    RHOMBUS_DIAGONALS_PERPENDICULAR, // 35
    PARALLELOGRAM_DIAGONALS_PERPENDICULAR_RHOMBUS, // 36
    RECTANGLE_DIAGONALS_EQUAL, // 37
    PARALLELOGRAM_DIAGONALS_EQUAL_RECTANGLE, // 38
    ISOSTRAPEZOID_BASE_ANGLES_EQUAL, // 39
    TRAPEZOID_BASE_ANGLES_EQUAL_ISOSTRAPEZOID, // 40
    ISOSTRAPEZOID_DIAGONALS_EQUAL, // 41
    TRAPEZOID_DIAGONALS_EQUAL_ISOSTRAPEZOID, // 42
    TRAPEZOID_MIDSEGMENT_PARALLEL_BASES_EQUAL_BASE_AVERAGE, // 43
    TRAPEZOID_LINE_BISECTS_SIDE_PARALLEL_BASES_IS_MIDSEGMENT, // 44
    TRIANGLE_BISECTORS_INTERSECT, // 45
    TRIANGLE_BISECTORS_INTERSECT_RATIO_1_FAR_2_CLOSE, // 46
    POINT_ON_ANGLEBISECTOR_EQUAL_DISTANCE_TO_SIDES, // 47
    POINT_IN_EQUAL_DISTANCE_TO_SIDES_IS_ON_ANGLEBISECTOR, // 48
    TRIANGLES_ANGLEBISECTORS_INTERSECT_INCIRCLE_CENTER, // 49
    EVERY_TRIANGLE_HAS_INCIRCLE, // 50
    POINT_ON_PERPENDICULARBISECTOR_EQUAL_DISTANCE_TO_SIDE_VERTICES, // 51
    POINT_AT_EQUAL_DISTANCE_FROM_SIDE_VERTICES_IS_ON_PERPENDICULARBISECTOR, // 52
    EVERY_TRIANGLE_HAS_CIRCUMCIRCLE, // 53
    TRIANGLE_PERPENDICULARBISECTORS_INTERSECT_CIRCUMCIRCLE_CENTER, // 54
    TRIANGLE_PERPENDICULARS_INTERSECT_ONE_POINT, // 55
    QUAD_HAS_CIRCUMCIRCLE_IF_OPPOSITE_ANGLES_180, // 56
    CONVEX_QUAD_HAS_INCIRCLE_IF_OPPOSITE_SIDES_SUM_EQUAL, // 57
    POLYGON_HAS_CIRCUMCIRCLE, // 58
    POLYGON_HAS_INCIRCLE, // 59
    THREE_POINTS_ARE_ON_SINGLE_CIRCLE, // 60
    EQUAL_ARCS_EQUAL_CENTERALANGLES, // 61
    EQUAL_CHORDS_EQUAL_CENTERALANGLES, // 62
    EQUAL_ARCS_EQUAL_CHORDS, // 63
    EQUAL_CHORDS_EQUAL_DISTANCE_TO_CENTER, // 64
    EQUAL_DISTANCE_TO_CENTER_EQUAL_CHORDS, // 65
    CHORD_SIZE_DEPENDS_ON_CHORD_DISTANCE_TO_CENTER, // 66
    PERPENDICULAR_FROM_CENTER_TO_CHORD_BISECTS_CHORD_BISECTS_CENTRALANGLE_BISECTS_ARC, // 67
    LINE_FROM_CIRCLE_CENTER_INTERSECTS_CHORD_PERPENDICULAR_CHORD, // 68
    PERIPHERALANGLE_EQUAL_HALF_CENTERALANGLE, // 69
    EQUAL_PERIPHERALANGLES_EQUAL_ARCS_EQUAL_CHORDS, // 70
    EQUAL_ARCS_EQUAL_PERIPHERALANGLES, // 71
    PERIPHERALANGLE_ON_SAME_CHORD_FROM_SAME_SIDE_EQUAL, // 72
    PERIPHERALANGLE_ON_DIAMETER_RIGHT, // 73
    RIGHT_PERIPHERALANGLE_IS_ON_DIAMETER, // 74
    INTIRIOR_ANGLE_IS_AVERAGE_OF_TRAPPED_ARCS, // 75
    EXTERIOR_ANGLE_IS_DIFF_OVER_2_OF_TRAPPED_ARCS, // 76
    TANGENT_PERPENDICULAR_RADIUS, // 77
    LINE_PERPENDICULAR_RADIUS_AT_VERTEX, // 78
    ANGLE_BETWEEN_TANGENT_CHORD_EQUAL_PERIPHERALANGLE_ON_CHORD_SAME_DIRECTION, // 79
    TWO_TANGENTS_FROM_SAME_VERTEX_EQUAL, // 80
    LINE_FROM_CIRCLE_CENTER_TO_VERTEX_OF_TWO_TANGENTS_BISECTS_ANGLE, // 81
    CENTERS_SEGMENT_INTERSECTING_CIRCLES_PERPENDICULAR_BISECTS_RADICAL_LINE, // 82
    CONTACT_POINT_OF_CIRCLES_ON_CENTERS_SEGMENT_RAY, // 83
    PYTHAGOREAN_THEOREM, // 84
    PYTHAGOREAN_THEOREM_OPPOSITE, // 85
    RIGHT_TRIANGLE_HYPOTENUSE_BISECTOR_EQUAL_HALF_HYPOTENUSE, // 86
    TRIANGLE_HYPOTENUSE_BISECTOR_EQUAL_HALF_HYPOTENUSE_IS_RIGHT, // 87
    RIGHT_TRIANGLE_30_ANGLE_OPPOSITE_SIDE_HALF_HYPOTENUSE, // 88
    RIGHT_TRIANGLE_SIDE_HALF_HYPOTENUSE_OPPOSITE_ANGLE_30, // 89
    THALESSEN_THEOREM, // 90
    THALESSEN_THEOREM_EXTENDED, // 91
    THALESSEN_THEOREM_OPPOSITE, // 92
    ANGLEBISECTOR_THEOREM, // 93
    ANGLEBISECTOR_THEOREM_OPPOSITE, // 94
    ANGLEBISECTOR_THEOREM_OUTER_ANGLE, // 95
    ANGLEBISECTOR_THEOREM_OUTER_ANGLE_OPPOSITE, // 96
    TRIANGLE_SIMILARITY_S_A_S, // 97
    TRIANGLE_SIMILARITY_A_A, // 98
    TRIANGLE_SIMILARITY_S_S_S, // 99
    SIMILAR_TRIANGLES_PERPENDICULAR_RATIO_EQUAL_SIMILARITY_RATIO, // 100 A
    SIMILAR_TRIANGLES_ANGLEBISECTOR_RATIO_EQUAL_SIMILARITY_RATIO, // 100 B
    SIMILAR_TRIANGLES_BISECTOR_RATIO_EQUAL_SIMILARITY_RATIO, // 100 C
    SIMILAR_TRIANGLES_PERIMETER_EQUAL_SIMILARITY_RATIO, // 100 D
    SIMILAR_TRIANGLES_RATIO_EQUAL_CIRCUMCIRCLE_RADII_RATIO, // 100 E
    SIMILAR_TRIANGLES_RATIO_EQUAL_INCIRCLE_RADII_RATIO, // 100 F
    SIMILAR_TRIANGLES_RATIO_SQUARED_EQUAL_AREA_RATIO, // 100 G
    CIRCLE_TWO_CHORDS_INTERSECT_ONE_CHORD_PART_PRODUCT_EQUAL_OTHER_CHORD_PART_PRODUCT, // 101
    VERTEX_HAS_TWO_LINES_INTERSECT_CIRCLE_ONE_INTERESCT_MULTIPLY_OUTER_PART_EQUAL_OTHER_INTERSECT_MULTIPLY_OUTER_PART, // 102
    VERTEX_HAS_INTERSECT_AND_TANGENT_CIRCLE_INTERSECT_MULTIPLY_OUTER_PART_EQUAL_TANGENT_SQUARED, // 103
    EUCLIDEAN_THEOREM, // 104
    EUCLIDEAN_THEOREM_OPPOSITE, // 105
    ANGLE_SUM_OF_CONVEX_SHAPE_EQUAL_180_TIMES_SIDE_MINUS_2, // 106

    // Also allowed
    PARALLELOGRAM_AREA_SIDE_PERPENDICULAR,
    TRIANGLE_AREA_SIDE_PERPENDICULAR,
    RHOMBUS_AREA_DIAGONALS,
    TRAPEZOID_AREA_SIDE_BASES,
    CIRCLE_AREA_RADIUS_SQUARED,
    CIRCLE_PERIMETER_2RADIUS,
}

public static class Reasons
{
    public static Reason FromIndex(int Index, char Section)
    {
        if (Index > 25) Index += 2; //  Some reasons have multiple subs, 
        if (Index > 100) Index += 6;  // We need to jump over them

        Index += Convert.ToInt32(Section) - 41; // 41 is the index of A in ascii

        return (Reason)Index;
    }

    public static (int, char) ToIndex(this Reason reason)
    {
        var initial = (int)reason;

        var i = initial;
        if (i > 27) i -= 2;
        if (i > 108) i -= 6;

        if (i > 24 && i < 28)
        {
            return (25, Convert.ToChar(41 + i - 25));
        } else if (i > 101 && 1 < 109)
        {
            return (100, Convert.ToChar(41 + i - 102));
        }

        return (i, Convert.ToChar(0));
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class ReasonAttribute : Attribute
{
    // See the attribute guidelines at 
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    readonly int index;
    readonly int section;

    // This is a positional argument
    public ReasonAttribute(Reason reason)
    {
        (index, section) = reason.ToIndex();
    }

    public int ReasonIndex
    {
        get => index;
    }
    public char Section => (char)section;
}