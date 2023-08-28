using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;

namespace Dynamically.Backend.Helpers;


#pragma warning disable CS8602
#pragma warning disable CS8604
public partial class RoleMap
{
    private void Segment__AddToRole<T>(Role role, T item, Segment Subject)
    {
        switch (role)
        {

            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                Subject.joint1.OnMoved.Add((cx, cy, _, _) =>
                {
                    if (!Subject.joint1.CurrentlyDragging) return;
                    Subject.joint2.X = Subject.joint1.X - (Subject.joint1.X - c1.center.X) * 2;
                    Subject.joint2.Y = Subject.joint1.Y - (Subject.joint1.Y - c1.center.Y) * 2;
                });
                Subject.joint2.OnMoved.Add((cx, cy, _, _) =>
                {
                    if (!Subject.joint2.CurrentlyDragging) return;
                    Subject.joint1.X = Subject.joint2.X - (Subject.joint2.X - c1.center.X) * 2;
                    Subject.joint1.Y = Subject.joint2.Y - (Subject.joint2.Y - c1.center.Y) * 2;
                }); // TODO: fix this, OnMoved functions cant manipulate positions (testing, may be incorrect)
                break;
            // Triangle
            case Role.TRIANGLE_Side:
                Subject.OnRemoved.Add((_, _) => (item as Triangle).Dismantle());
                break;

            default:
                break;
        }
    }
    private void Segment__RemoveFromRole<T>(Role role, T item, Segment Subject)
    {
        switch (role)
        {
            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                Subject.joint1.OnMoved.Remove((cx, cy, _, _) =>
                {
                    if (!Subject.joint1.CurrentlyDragging) return;
                    Subject.joint2.X = Subject.joint1.X - (Subject.joint1.X - c1.center.X) * 2;
                    Subject.joint2.Y = Subject.joint1.Y - (Subject.joint1.Y - c1.center.Y) * 2;
                });
                Subject.joint2.OnMoved.Remove((cx, cy, _, _) =>
                {
                    if (!Subject.joint2.CurrentlyDragging) return;
                    Subject.joint1.X = Subject.joint2.X - (Subject.joint2.X - c1.center.X) * 2;
                    Subject.joint1.Y = Subject.joint2.Y - (Subject.joint2.Y - c1.center.Y) * 2;
                });
                break;
            case Role.TRIANGLE_Side:
                Subject.OnRemoved.Remove((_, _) => (item as Triangle).Dismantle());
                break;
            default:
                break;
        }
    }

    private void Segment__TransferRole<T>(Segment From, Role role, T item, Segment Subject)
    {

        switch (role)
        {
            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                From.joint1.OnMoved.Remove((cx, cy, _, _) =>
                {
                    if (!Subject.joint1.CurrentlyDragging) return;
                    Subject.joint2.X = Subject.joint1.X - (Subject.joint1.X - c1.center.X) * 2;
                    Subject.joint2.Y = Subject.joint1.Y - (Subject.joint1.Y - c1.center.Y) * 2;
                });
                From.joint2.OnMoved.Remove((cx, cy, _, _) =>
                {
                    if (!Subject.joint2.CurrentlyDragging) return;
                    Subject.joint1.X = Subject.joint2.X - (Subject.joint2.X - c1.center.X) * 2;
                    Subject.joint1.Y = Subject.joint2.Y - (Subject.joint2.Y - c1.center.Y) * 2;
                });
                Subject.joint1.OnMoved.Add((cx, cy, _, _) =>
                {
                    if (!Subject.joint1.CurrentlyDragging) return;
                    Subject.joint2.X = Subject.joint1.X - (Subject.joint1.X - c1.center.X) * 2;
                    Subject.joint2.Y = Subject.joint1.Y - (Subject.joint1.Y - c1.center.Y) * 2;
                });
                Subject.joint2.OnMoved.Add((cx, cy, _, _) =>
                {
                    if (!Subject.joint2.CurrentlyDragging) return;
                    Subject.joint1.X = Subject.joint2.X - (Subject.joint2.X - c1.center.X) * 2;
                    Subject.joint1.Y = Subject.joint2.Y - (Subject.joint2.Y - c1.center.Y) * 2;
                });
                break;
            case Role.TRIANGLE_Side:
                From.OnRemoved.Remove((_, _) => (item as Triangle).Dismantle());
                Subject.OnRemoved.Add((_, _) => (item as Triangle).Dismantle());
                break;
            default:
                break;
        }
    }

}

#pragma warning restore CS8602
#pragma warning restore CS8604