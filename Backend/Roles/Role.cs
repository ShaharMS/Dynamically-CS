﻿using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public enum Role
{
    // Vertex

    VERTEX_On,

    RAY_On,

    SEGMENT_Corner,
    SEGMENT_On,
    SEGMENT_Center,

    ARC_Center,
    ARC_Corner,
    ARC_On,

    CIRCLE_Center,
    CIRCLE_On,

    TRIANGLE_Corner,
    TRIANGLE_InCircleCenter,
    TRIANGLE_CircumCircleCenter,

    QUAD_Corner,
    QUAD_CircumCircleCenter,
    QUAD_InCircleCenter,
    // Segment

    ANGLE_Bisector,
    
    CIRCLE_Radius,
    CIRCLE_Diameter,
    CIRCLE_Chord,
    CIRCLE_Tangent,

    ARC_Radius,
    ARC_Diameter,
    ARC_Chord,
    ARC_Tangent,

    TRIANGLE_Side,
    TRIANGLE_AngleBisector,
    TRIANGLE_Perpendicular,

    QUAD_Side,
    QUAD_Hypotenuse,

    // EllipseBase


    /// <summary>
    /// Used only in internal RoleMap class.
    /// </summary>
    Null
}