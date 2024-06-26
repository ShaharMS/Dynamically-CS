using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;
using Dynamically.Geometry;
using Dynamically.Geometry.Basics;


namespace Dynamically.Backend.Roles;


#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable CA1822
public partial class RoleMap
{
    private void Vertex__AddToRole<T>(Role role, T item, Vertex Subject)
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
                Subject.OnDragged.Add(s1.__reposition);
                Subject.OnMoved.Add(s1.__repositionLabel);
                s1.Vertex1.Relations.Remove(s1.Vertex2);
                s1.Vertex2.Relations.Remove(s1.Vertex1);
                s1.Vertex1.EffectedFormulas.AddRange(new Formula[] { s1.Formula, s1.MiddleFormula, s1.RayFormula });
                s1.Vertex2.EffectedFormulas.AddRange(new Formula[] { s1.Formula, s1.MiddleFormula, s1.RayFormula });
                break;
            case Role.SEGMENT_On:
                (item as Segment).Formula.AddFollower(Subject);
                break;
            case Role.SEGMENT_Center:
                (item as Segment).MiddleFormula.AddFollower(Subject);
                break;
            // Arc
            case Role.ARC_On:
                (item as Arc).Formula.AddFollower(Subject);
                break;

            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.AddFollower(Subject);
                foreach (Vertex vertex in Subject.Relations) {
                    if (vertex.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(vertex, Subject.GetSegmentTo(vertex));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                Subject.OnRemoved.Add((item as Circle).__circle_Remove);
                Subject.EffectedFormulas.Add((item as Circle).Formula);
                foreach (Vertex vertex in Subject.Relations) {
                    if (vertex.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(vertex, Subject.GetSegmentTo(vertex));
                    }
                }
                break;
            case Role.TRIANGLE_CircumCircleCenter:
                var t1 = item as Triangle;
                t1.Circumcircle.OnRemoved.Add(() => {
                    t1.Circumcircle = null;
                    t1.Provider.Regenerate();
                });
                break;
            case Role.TRIANGLE_InCircleCenter:
                var t2 = item as Triangle;
                t2.Incircle.OnRemoved.Add(() => {
                    t2.Incircle.Center.OnDragStart.Remove(t2.__triangleMoveThroughIncircleCenter);
                    t2.Incircle = null;
                    t2.Provider.Regenerate();
                    t2.Formula.RemoveFollower(Subject);
                });
                t2.Formula.AddFollower(Subject);
                t2.Incircle.Center.OnDragStart.Add(t2.__triangleMoveThroughIncircleCenter);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Add((item as Triangle).__Disment);
                Subject.OnDragged.Add((item as Triangle).__Regen);
                break;
            // Quadrilateral
            case Role.QUAD_Corner:
                Subject.OnRemoved.Add((item as Quadrilateral).__Disment);
                Subject.OnDragged.Add((item as Quadrilateral).__Regen);
                break;
            default: break;
        }
    }

    private void Vertex__RemoveFromRole<T>(Role role, T item, Vertex Subject)
    {
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).RemoveFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnDragged.Remove(s1.__reposition);
                s1.Vertex1.Relations.Remove(s1.Vertex2);
                s1.Vertex2.Relations.Remove(s1.Vertex1);
                foreach (var formula in new Formula[] { s1.Formula, s1.MiddleFormula, s1.RayFormula })
                {
                    s1.Vertex1.EffectedFormulas.Remove(formula);
                    s1.Vertex2.EffectedFormulas.Remove(formula);
                }
                break;
            case Role.SEGMENT_On:
                (item as Segment).Formula.RemoveFollower(Subject);
                break;
            case Role.SEGMENT_Center:
                (item as Segment).MiddleFormula.RemoveFollower(Subject);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.RemoveFollower(Subject);
                foreach (Vertex vertex in Subject.Relations) {
                    if (vertex.Roles.Has(Role.CIRCLE_On, item)) {
                        // try for both diameter & chord
                        Subject.GetSegmentTo(vertex).Roles.RemoveFromRole(Role.CIRCLE_Chord, item);
                        Subject.GetSegmentTo(vertex).Roles.RemoveFromRole(Role.CIRCLE_Diameter, item);
                    }
                    if (vertex.Roles.Has(Role.CIRCLE_Center, item)) Subject.GetSegmentTo(vertex).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                }
                break;
            case Role.CIRCLE_Center:
                (item as Circle).Center.OnMoved.Remove((item as Circle).__circle_OnChange);
                (item as Circle).Center.OnRemoved.Remove((item as Circle).__circle_Remove);
                (item as Circle).Center.EffectedFormulas.Remove((item as Circle).Formula);
                foreach (Vertex vertex in Subject.Relations) {
                    if (vertex.Roles.Has(Role.CIRCLE_On, item)) {
                        vertex.Roles.RemoveFromRole(Role.CIRCLE_On, item);
                        Subject.GetSegmentTo(vertex).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                    }
                }

                break;
            case Role.TRIANGLE_CircumCircleCenter:
                var t1 = item as Triangle;
                // Circumcircle callback is already deallocated
                t1.Provider.Regenerate();
                break;
            case Role.TRIANGLE_InCircleCenter:
                var t2 = item as Triangle;
                // Incircle callback is already deallocated
                t2.Formula.RemoveFollower(Subject);
                t2.Incircle.Center.OnDragStart.Remove(t2.__triangleMoveThroughIncircleCenter);
                t2.Provider.Regenerate();
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Remove((item as Triangle).__Disment);
                Subject.OnDragged.Remove((item as Triangle).__Regen);
                break;
            // Quadrilateral
            case Role.QUAD_Corner:
                Subject.OnRemoved.Remove((item as Quadrilateral).__Disment);
                Subject.OnDragged.Remove((item as Quadrilateral).__Regen);
                break;
            default: break;
        }
    }

    private void Vertex__TransferRole<T>(Vertex From, Role role, T item, Vertex Subject)
    {
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).RemoveFollower(From);
                (item as RayFormula).AddFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                s1.ReplaceVertex(From, Subject);
                From.OnMoved.Remove(s1.__reposition);
                Subject.OnDragged.Add(s1.__reposition);
                break;
            case Role.SEGMENT_On:
                (item as Segment).Formula.RemoveFollower(From);
                (item as Segment).Formula.AddFollower(Subject);
                break;
            case Role.SEGMENT_Center:
                (item as Segment).MiddleFormula.RemoveFollower(From);
                (item as Segment).MiddleFormula.AddFollower(Subject);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.RemoveFollower(From);
                foreach (Vertex vertex in Subject.Relations)
                {
                    if (vertex.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        From.CreateBoardRelationsWith(vertex, From.GetSegmentTo(vertex));
                    }
                } 
                (item as Circle).Formula.AddFollower(Subject);
                foreach (Vertex vertex in Subject.Relations)
                {
                    if (vertex.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        Subject.CreateBoardRelationsWith(vertex, Subject.GetSegmentTo(vertex));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                From.EffectedFormulas.Remove((item as Circle).Formula);
                Subject.EffectedFormulas.Add((item as Circle).Formula);
                (item as Circle).Set(Subject, (item as Circle).Radius);
                foreach (Vertex vertex in Subject.Relations)
                {
                    if (vertex.Roles.Has(Role.CIRCLE_On, item))
                    {
                        From.CreateBoardRelationsWith(vertex, From.GetSegmentTo(vertex));
                        Subject.CreateBoardRelationsWith(vertex, Subject.GetSegmentTo(vertex));
                    }
                }
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                var t1 = item as Triangle;

                char? id = null;
                if (t1.Incircle != null)
                {
                    id = t1.Incircle.Center.Id;
                    t1.Incircle.Dismantle();
                    t1.Incircle.Center.RemoveFromBoard();
                }

                if (t1.Vertex1 == From) t1.Vertex1 = Subject;
                else if (t1.Vertex2 == From) t1.Vertex2 = Subject;
                else if (t1.Vertex3 == From) t1.Vertex3 = Subject;

                t1.Segment12.ReplaceVertex(From, Subject);
                t1.Segment23.ReplaceVertex(From, Subject);
                t1.Segment13.ReplaceVertex(From, Subject);


                From.OnRemoved.Remove(t1.__Disment);
                From.OnDragged.Remove(t1.__Regen);
                Subject.OnRemoved.Add(t1.__Disment);
                Subject.OnDragged.Add(t1.__Regen);


                if (id != null)
                {
                    t1.GenerateInCircle();
                    t1.Incircle.Center.Id = id.Value;
                }
                break;
            default: break;
        }
    }
}



#pragma warning restore CS8602
#pragma warning restore CS8604
#pragma warning restore CA1822
