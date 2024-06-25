using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

#pragma warning disable CS8602
#pragma warning disable CS8604
public partial class RoleMap
{
    public void EllipseBase__AddToRole<T>(Role role, T Item, EllipseBase Subject)
    {
        switch (role)
        {
            case Role.VERTEX_On:
                var item = Item as Vertex;
                item.Formula.AddFollower(Subject);
                item.PositioningByFormula.Add(Subject.GetCenterPosition);
                break;
        }
    }

    public void EllipseBase__RemoveFromRole<T>(Role role, T Item, EllipseBase Subject)
    {
        switch (role)
        {
            case Role.VERTEX_On:
                var item = Item as Vertex;
                item.Formula.RemoveFollower(Subject);
                item.PositioningByFormula.Remove(Subject.GetCenterPosition);
                break;
        }
    }

    public void EllipseBase__TransferRole<T>(EllipseBase From, Role role, T Item, EllipseBase Subject)
    {
        switch (role)
        {
            case Role.VERTEX_On:
                var item = Item as Vertex;
                item.Formula.RemoveFollower(From);
                item.PositioningByFormula.Remove(From.GetCenterPosition);
                item.Formula.AddFollower(Subject);
                item.PositioningByFormula.Add(Subject.GetCenterPosition);
                break;
        }
    }
}

#pragma warning restore CS8602
#pragma warning restore CS8604