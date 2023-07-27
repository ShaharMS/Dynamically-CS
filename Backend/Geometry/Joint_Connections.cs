using Dynamically.Formulas;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Joint
{
    public Segment Connect(Joint to, bool updateRelations = true)
    {
        // Don't connect something twice
        foreach (Segment c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }

        var connection = new Segment(this, to);
        Connections.Add(connection);
        to.Connections.Add(connection);

        reposition();
        to.reposition();

        if (updateRelations) CreateBoardRelationsWith(to, connection);
        return connection;
    }

    public List<Segment> Connect(params Joint[] joints)
    {
        var cons = new List<Segment>();
        foreach (Joint joint in joints)
        {
            var doNothing = false;
            // Don't connect something twice
            foreach (Segment c in Connections.Concat(joint.Connections))
            {
                if ((c.joint1 == this && c.joint2 == joint) || (c.joint2 == this && c.joint1 == joint))
                    doNothing = true;
            }

            if (!doNothing)
            {
                var connection = new Segment(this, joint);
                cons.Add(connection);
                Connections.Add(connection);
                joint.Connections.Add(connection);
                joint.reposition();
                CreateBoardRelationsWith(joint, connection);
            }
        }
        reposition();
        return cons;
    }

    public bool IsConnectedTo(Joint joint)
    {
        if (this == joint) return false;
        Log.Write(Connections);
        foreach (Segment c in Connections)
        {
            Joint[] js = new[] { c.joint1, c.joint2 };
            if (js.Contains(joint) && js.Contains(this)) return true;
        }
        return false;
    }

    public Segment? GetConnectionTo(Joint to)
    {
        foreach (Segment c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }
        return null;
    }

    public void Disconnect(Joint joint)
    {
        var past = Connections.ToList();
        foreach (Segment c in past)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                Connections.Remove(c);
                MainWindow.BigScreen.Children.Remove(c);
            }
        }
        var past2 = joint.Connections.ToList();
        foreach (Segment c in past2)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                joint.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                joint.Connections.Remove(c);
                MainWindow.BigScreen.Children.Remove(c);
            }
        }
    }

    public void Disconnect(params Joint[] joints)
    {
        foreach (Joint joint in joints)
        {
            var past = Connections.ToList();
            foreach (Segment c in past)
            {
                if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                {
                    this.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    joint.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    Connections.Remove(c);
                    MainWindow.BigScreen.Children.Remove(c);
                }
            }

            var past2 = joint.Connections.ToList();
            foreach (Segment c in past2)
            {
                if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                {
                    this.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    joint.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    joint.Connections.Remove(c);
                    MainWindow.BigScreen.Children.Remove(c);
                }
            }
        }

    }

    public void DisconnectAll()
    {

        foreach (Segment c in Connections)
        {
            MainWindow.BigScreen.Children.Remove(c);
            if (c.joint1 != this) c.joint1.Connections.Remove(c);
            else c.joint2.Connections.Remove(c);
            if (c.joint1 != this) c.joint1.RepositionText();
            else c.joint2.RepositionText();

            c.joint1.Roles.RemoveFromRole(Role.SEGMENT_Corner, c); // One of them is `this`
            c.joint2.Roles.RemoveFromRole(Role.SEGMENT_Corner, c); // The other is the second joint
        }
        Connections.Clear();
        RepositionText();
    }

    public void CreateBoardRelationsWith(Joint joint, Segment segment)
    {
        // Basic connection info

        //First check - radius
        if ((joint.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_Center)) || joint.Roles.Has(Role.CIRCLE_Center) && Roles.Has(Role.CIRCLE_On)) {
            foreach (var circle in joint.Roles.Access<Circle>(Role.CIRCLE_On)) {
                if (Roles.Has(Role.CIRCLE_Center, circle)) {
                    segment.Roles.AddToRole(Role.CIRCLE_Radius, circle);
                }
            }
        }
        // Second check - diameter & chord
        if (joint.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_On)) {
            foreach (var circle in joint.Roles.Access<Circle>(Role.CIRCLE_On)) {
                if (Roles.Has(Role.CIRCLE_On, circle)) {
                    if (joint.DistanceTo(this) == circle.radius * 2 && joint.RadiansTo(circle.center) == circle.center.RadiansTo(this)) {
                        segment.Roles.AddToRole(Role.CIRCLE_Diameter, circle);
                        circle.center.Roles.AddToRole(Role.SEGMENT_Center, new RatioOnSegmentFormula(segment.Formula, 0.5))
                    }
                    segment.Roles.AddToRole(Role.CIRCLE_Chord, circle);
                }
            }
        }

        foreach (Segment c in Connections)
        {
            var other = c.joint1 == this ? c.joint2 : c.joint1;

            // First case - connecting a line and forming a triangle

            if (other.IsConnectedTo(joint) /* this.IsConnectedTo(joint) is `true` */) // this joint is connected to other, and the just connected joint is also connected to it -> potentially new triangle
            {
                var hasTriangle = false;
                var currentTriangles = Roles.Access<Triangle>(Role.TRIANGLE_Corner);
                foreach (var t in currentTriangles)
                {
                    if (t.IsDefinedBy(this, other, joint))
                    {
                        hasTriangle = true;
                        break;
                    }
                }
                if (!hasTriangle)
                {
                    var triangle = new Triangle(this, other, joint);
                    foreach (var j in new[] { other, joint, this })
                    {
                        j.Roles.AddToRole(Role.TRIANGLE_Corner, triangle);
                    }
                }
            }
        }
    }
}
