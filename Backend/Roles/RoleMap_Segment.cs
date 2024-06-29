using Dynamically.Backend.Geometry;
using Dynamically.Geometry;
using Avalonia;
using System;
using Dynamically.Geometry.Basics;


namespace Dynamically.Backend.Roles;


#pragma warning disable CS8602
#pragma warning disable CS8604
public partial class RoleMap
{
    private void Segment__AddToRole<T>(Role role, T item, Segment Subject)
    {
        switch (role)
        {
            // Angle
            case Role.ANGLE_Bisector:
                var a1 = item as Angle;
                Subject.OnRemoved.Add((V1, V2) =>
                {
                    if (a1.Center == V1) V2.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                    else V1.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                });
                break;
            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                Subject.Vertex1.OnMoved.Add((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex1, Subject.Vertex2));
                Subject.Vertex2.OnMoved.Add((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex2, Subject.Vertex1));
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
            // Angle
            case Role.ANGLE_Bisector:
                var a1 = item as Angle;
                Subject.OnRemoved.Remove((V1, V2) =>
                {
                    if (a1.Center == V1) V2.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                    else V1.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                });
                break;
            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                Subject.Vertex1.OnMoved.Remove((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex1, Subject.Vertex2));
                Subject.Vertex2.OnMoved.Remove((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex2, Subject.Vertex1));
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
            // Angle
            case Role.ANGLE_Bisector:
                var a1 = item as Angle;
                From.OnRemoved.Remove((V1, V2) =>
                {
                    if (a1.Center == V1) V2.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                    else V1.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                });

                Subject.OnRemoved.Add((V1, V2) =>
                {
                    if (a1.Center == V1) V2.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                    else V1.Roles.RemoveFromRole(Role.RAY_On, a1.BisectorRay);
                });
                break;

            // Circle
            case Role.CIRCLE_Diameter:
                var c1 = item as Circle;

                From.Vertex1.OnMoved.Remove((_, _, _, _) => c1.__circle_handleDiameter(From.Vertex1, From.Vertex2));
                From.Vertex2.OnMoved.Remove((_, _, _, _) => c1.__circle_handleDiameter(From.Vertex2, From.Vertex1));

                Subject.Vertex1.OnMoved.Add((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex1, Subject.Vertex2));
                Subject.Vertex2.OnMoved.Add((_, _, _, _) => c1.__circle_handleDiameter(Subject.Vertex2, Subject.Vertex1));
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