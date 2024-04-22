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

                Subject.Vertex1.GetConnectionTo(c1.Center)!.RayFormula.AddFollower(Subject.Vertex2);
                Subject.Vertex2.GetConnectionTo(c1.Center)!.RayFormula.AddFollower(Subject.Vertex1);
                break;
            // Triangle
            case Role.TRIANGLE_Side:
                Subject.OnRemoved.Add((item as Triangle).__Disment);
                Subject.OnDragged.Add((item as Triangle).__Regen);
                break;
            // Quadrilateral
            case Role.QUAD_Side:
                Subject.OnRemoved.Add((item as Quadrilateral).__Disment);
                Subject.OnDragged.Add((item as Quadrilateral).__Regen);
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

                Subject.Vertex1.GetConnectionTo(c1.Center)!.RayFormula.RemoveFollower(Subject.Vertex2);
                Subject.Vertex2.GetConnectionTo(c1.Center)!.RayFormula.RemoveFollower(Subject.Vertex1);
                break;
            case Role.TRIANGLE_Side:
                Subject.OnRemoved.Remove((item as Triangle).__Disment);
                Subject.OnDragged.Remove((item as Triangle).__Regen);
                break;
            // Quadrilateral
            case Role.QUAD_Side:
                Subject.OnRemoved.Remove((item as Quadrilateral).__Disment);
                Subject.OnDragged.Remove((item as Quadrilateral).__Regen);
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
                From.Vertex1.GetConnectionTo(c1.Center)!.RayFormula.RemoveFollower(From.Vertex2);
                From.Vertex2.GetConnectionTo(c1.Center)!.RayFormula.RemoveFollower(From.Vertex1);

                Subject.Vertex1.GetConnectionTo(c1.Center)!.RayFormula.AddFollower(Subject.Vertex2);
                Subject.Vertex2.GetConnectionTo(c1.Center)!.RayFormula.AddFollower(Subject.Vertex1);
                break;
            // Triangle
            case Role.TRIANGLE_Side:
                From.OnRemoved.Remove((item as Triangle).__Disment);
                From.OnDragged.Remove((item as Triangle).__Regen);
                Subject.OnRemoved.Add((item as Triangle).__Disment);
                Subject.OnDragged.Add((item as Triangle).__Regen);
                break;
            // Quadrilateral
            case Role.QUAD_Side:
                From.OnRemoved.Remove((item as Quadrilateral).__Disment);
                From.OnDragged.Remove((item as Quadrilateral).__Regen);
                Subject.OnRemoved.Add((item as Quadrilateral).__Disment);
                Subject.OnDragged.Add((item as Quadrilateral).__Regen);
                break;
            default:
                break;
        }
    }

}

#pragma warning restore CS8602
#pragma warning restore CS8604