using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public enum Role
{
    GENERAL_OnConnection,

    CIRCLE_Center,
    CIRCLE_Contact,
    CIRCLE_Tangent,

    TRIANGLE_Joint
}