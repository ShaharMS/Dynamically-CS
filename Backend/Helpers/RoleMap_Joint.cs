using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;
using Dynamically.Shapes;

namespace Dynamically.Backend.Helpers;


#pragma warning disable CS8602
#pragma warning disable CS8604

public partial class RoleMap
{
    private void Joint__AddToRole<T>(Role role, T item, Joint joint)
    {
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).AddFollower(joint);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                joint.OnMoved.Add(s1.__updateFormula);
                joint.OnDragged.Add(s1.__reposition);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.AddFollower(joint);
                break;
            case Role.CIRCLE_Center:
                joint.OnMoved.Add((item as Circle).__circle_OnChange);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                joint.OnRemoved.Add((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }

    }
}



#pragma warning restore CS8602
#pragma warning restore CS8604