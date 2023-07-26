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
    SEGMENT_Split,

    CIRCLE_Center,
    CIRCLE_On,

    TRIANGLE_Corner,
    
    // Connection
    CIRCLE_Radius,
    CIRCLE_DIAMETER,
    CIRCLE_CHORD,
    CIRCLE_TANGENT,

    TRIANGLE_SIDE,
    TRIANGLE_ANGLE_BISECTOR,
    TRIANGLE_PERPENDICULAR
}