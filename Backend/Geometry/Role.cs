using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public enum Role
{
    // Joint
    RAY_On,

    SEGMENT_Corner,
    SEGMENT_On,
    SEGMENT_Center,

    CIRCLE_Center,
    CIRCLE_On,

    TRIANGLE_Corner,
    TRIANGLE_InCircleCenter,
    TRIANGLE_CircumCircleCenter,

    QUAD_Corner,
    QUAD_CircumCircleCenter,
    QUAD_InCircleCenter,
    // Segment
    CIRCLE_Radius,
    CIRCLE_Diameter,
    CIRCLE_Chord,
    CIRCLE_Tangent,

    TRIANGLE_Side,
    TRIANGLE_AngleBisector,
    TRIANGLE_Perpendicular,

    QUAD_Side,
    QUAD_Hypotenuse,

    /// <summary>
    /// Used only in internal RoleMap class.
    /// </summary>
    Null
}