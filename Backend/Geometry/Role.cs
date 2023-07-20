using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public enum Role
{
    RAY_On,
    SEGMENT_On,

    CIRCLE_Center,
    CIRCLE_Contact,

    TRIANGLE_Corner
}