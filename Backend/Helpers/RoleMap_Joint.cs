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
                Subject.OnDragged.Add(s1.__reposition);
                s1.joint1.Relations.Remove(s1.joint2);
                s1.joint2.Relations.Remove(s1.joint1);
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
                foreach (Joint joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                Subject.OnRemoved.Add((item as Circle).__circle_Remove);
                foreach (Joint joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
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
                (item as RayFormula).RemoveFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnDragged.Remove(s1.__reposition);
                s1.joint1.Relations.Remove(s1.joint2);
                s1.joint2.Relations.Remove(s1.joint1);
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
                foreach (Joint joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        // try for both diameter & chord
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Chord, item);
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Diameter, item);
                    }
                    if (joint.Roles.Has(Role.CIRCLE_Center, item)) Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                }
                break;
            case Role.CIRCLE_Center:
                (item as Circle).center.OnMoved.Remove((item as Circle).__circle_OnChange);
                (item as Circle).center.OnRemoved.Add((item as Circle).__circle_Remove);
                foreach (Joint joint in Subject.Relations) {
                    if (joint.Roles.Has(Role.CIRCLE_On, item)) {
                        joint.Roles.RemoveFromRole(Role.CIRCLE_On, item);
                        Subject.GetConnectionTo(joint).Roles.RemoveFromRole(Role.CIRCLE_Radius, item);
                    }
                }
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Remove((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }
    }

    private void Joint__TransferRole<T>(Joint From, Role role, T item, Joint Subject)
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
                foreach (Joint joint in Subject.Relations)
                {
                    if (joint.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        From.CreateBoardRelationsWith(joint, From.GetConnectionTo(joint));
                    }
                } 
                (item as Circle).Formula.AddFollower(Subject);
                foreach (Joint joint in Subject.Relations)
                {
                    if (joint.Roles.Has((Role.CIRCLE_On, Role.CIRCLE_Center), item))
                    {
                        Subject.CreateBoardRelationsWith(joint, Subject.GetConnectionTo(joint));
                    }
                }
                break;
            case Role.CIRCLE_Center:
                (item as Circle).Set(Subject, (item as Circle).radius);
                foreach (Joint joint in Subject.Relations)
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
                if (t1.incircle != null)
                {
                    t1.incircle.Dismantle();
                    id = t1.incircle.center.Id;
                    t1.incircle.center.RemoveFromBoard();
                }

                if (t1.joint1 == From) t1.joint1 = Subject;
                else if (t1.joint2 == From) t1.joint2 = Subject;
                else if (t1.joint3 == From) t1.joint3 = Subject;

                t1.con12.ReplaceJoint(From, Subject);
                t1.con23.ReplaceJoint(From, Subject);
                t1.con13.ReplaceJoint(From, Subject);

                if (From.OnMoved.Contains(t1.__RecalculateInCircle))
                {
                    From.OnMoved.Remove(t1.__RecalculateInCircle);
                    Subject.OnMoved.Add(t1.__RecalculateInCircle);
                }

                From.OnRemoved.Remove((_, _) => t1.Dismantle());
                Subject.OnRemoved.Add((_, _) => t1.Dismantle());

                if (id != null)
                {
                    t1.GenerateInCircle();
                    t1.incircle.center.Id = id.Value;
                }
                break;
            default: break;
        }
    }
}



#pragma warning restore CS8602
#pragma warning restore CS8604
#pragma warning restore CA1822
