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
    private void Joint__AddToRole<T>(Role role, T item, Joint Subject)
    {
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).AddFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnMoved.Add(s1.__updateFormula);
                Subject.OnDragged.Add(s1.__reposition);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.AddFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Add((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }
    }

    private void Joint__RemoveFromRole<T>(Role role, T item, Joint Subject)
    {
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).AddFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnMoved.Remove(s1.__updateFormula);
                Subject.OnDragged.Remove(s1.__reposition);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.RemoveFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                var circ = item as Circle;
                circ.center.OnMoved.Remove(circ.__circle_OnChange);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Remove((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }
    }

}



#pragma warning restore CS8602
#pragma warning restore CS8604