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
                s1.Vertex1.EffectedFormulas.Add(s1.Formula);
                s1.Vertex2.EffectedFormulas.Add(s1.Formula);
                break;
            case Role.SEGMENT_On:
                (item as Segment).Formula.AddFollower(Subject);
                break;
            case Role.SEGMENT_Center:
                (item as Segment).MiddleFormula.AddFollower(Subject);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.AddFollower(Subject);
                foreach (Vertex joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                Subject.OnDragStart.Add((item as Circle).__circle_Moving);
                Subject.OnDragged.Add((item as Circle).__circle_StopMoving);
                Subject.OnRemoved.Add((item as Circle).__circle_Remove);
                Subject.EffectedFormulas.Add((item as Circle).Formula);
                foreach (Vertex joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            case Role.TRIANGLE_CircumCircleCenter:
                (item as Triangle).Circumcircle.OnRemoved.Add(() => (item as Triangle).Circumcircle = null);
                break;
            case Role.TRIANGLE_InCircleCenter:
                (item as Triangle).Incircle.OnRemoved.Add(() => (item as Triangle).Incircle = null);
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
                s1.Vertex1.EffectedFormulas.Remove(s1.Formula);
                s1.Vertex2.EffectedFormulas.Remove(s1.Formula);
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
                foreach (Vertex joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        // try for both diameter & chord
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Chord, item);
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Diameter, item);
                    }
                    if (joint.Roles.Has(Role.CIRCLE_Center, item)) Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                }
                break;
            case Role.CIRCLE_Center:
                (item as Circle).Center.OnMoved.Remove((item as Circle).__circle_OnChange);
                (item as Circle).Center.OnDragStart.Remove((item as Circle).__circle_Moving);
                (item as Circle).Center.OnDragged.Remove((item as Circle).__circle_StopMoving);
                (item as Circle).Center.OnRemoved.Remove((item as Circle).__circle_Remove);
                (item as Circle).Center.EffectedFormulas.Remove((item as Circle).Formula);
                foreach (Vertex joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        joint.Roles.RemoveFromRole(Role.CIRCLE_On, item);
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                    }
                }

                break;
            case Role.TRIANGLE_CircumCircleCenter:
                (item as Triangle).Circumcircle.OnRemoved.Remove(() => (item as Triangle).Circumcircle = null);
                break;
            case Role.TRIANGLE_InCircleCenter:
                (item as Triangle).Incircle.OnRemoved.Remove(() => (item as Triangle).Incircle = null);
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
                s1.ReplaceJoint(From, Subject);
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
                foreach (Vertex joint in Subject.Relations)
                {
                    if (joint.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        From.CreateBoardRelationsWith(joint, From.GetConnectionTo(joint));
                    }
                } 
                (item as Circle).Formula.AddFollower(Subject);
                foreach (Vertex joint in Subject.Relations)
                {
                    if (joint.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                From.EffectedFormulas.Remove((item as Circle).Formula);
                Subject.EffectedFormulas.Remove((item as Circle).Formula);
                (item as Circle).Set(Subject, (item as Circle).Radius);
                foreach (Vertex joint in Subject.Relations)
                {
                    if (joint.Roles.Has(Role.CIRCLE_On, item))
                    {
                        From.CreateBoardRelationsWith(joint, From.GetConnectionTo(joint));
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                var t1 = item as Triangle;

                char? id = null;
                if (t1.Incircle != null)
                {
                    t1.Incircle.Dismantle();
                    id = t1.Incircle.Center.Id;
                    t1.Incircle.Center.RemoveFromBoard();
                }

                if (t1.Vertex1 == From) t1.Vertex1 = Subject;
                else if (t1.Vertex2 == From) t1.Vertex2 = Subject;
                else if (t1.Vertex3 == From) t1.Vertex3 = Subject;

                t1.Segment12.ReplaceJoint(From, Subject);
                t1.Segment23.ReplaceJoint(From, Subject);
                t1.Segment13.ReplaceJoint(From, Subject);

                if (From.OnMoved.Contains(t1.__RecalculateInCircle))
                {
                    From.OnMoved.Remove(t1.__RecalculateInCircle);
                    Subject.OnMoved.Add(t1.__RecalculateInCircle);
                }

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
