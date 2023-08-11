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