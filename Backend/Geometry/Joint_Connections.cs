using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Joint
{
    public Connection Connect(Joint to, bool updateRelations = true)
    {
        // Don't connect something twice
        foreach (Connection c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }

        var connection = new Connection(this, to);
        Connections.Add(connection);
        to.Connections.Add(connection);

        reposition();
        to.reposition();

        if (updateRelations) CreateBoardRelationsWith(to);
        return connection;
    }

    public List<Connection> Connect(params Joint[] joints)
    {
        var cons = new List<Connection>();
        foreach (Joint joint in joints)
        {
            var doNothing = false;
            // Don't connect something twice
            foreach (Connection c in Connections.Concat(joint.Connections))
            {
                if ((c.joint1 == this && c.joint2 == joint) || (c.joint2 == this && c.joint1 == joint))
                    doNothing = true;
            }

            if (!doNothing)
            {
                var connection = new Connection(this, joint);
                cons.Add(connection);
                Connections.Add(connection);
                joint.Connections.Add(connection);
                joint.reposition();
                CreateBoardRelationsWith(joint);
            }
        }
        reposition();
        return cons;
    }

    public bool IsConnectedTo(Joint joint)
    {
        if (this == joint) return false;
        Log.Write(Connections);
        foreach (Connection c in Connections)
        {
            Joint[] js = new[] { c.joint1, c.joint2 };
            if (js.Contains(joint) && js.Contains(this)) return true;
        }
        return false;
    }

    public Connection? GetConnectionTo(Joint to)
    {
        foreach (Connection c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }
        return null;
    }

    public void Disconnect(Joint joint)
    {
        var past = Connections.ToList();
        foreach (Connection c in past)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                Connections.Remove(c);
                MainWindow.BigScreen.Children.Remove(c);
            }
        }
        var past2 = joint.Connections.ToList();
        foreach (Connection c in past2)
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
            foreach (Connection c in past)
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
            foreach (Connection c in past2)
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

        foreach (Connection c in Connections)
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

    public void CreateBoardRelationsWith(Joint joint)
    {
        // First case - connnecting a line and forming a triangle
        foreach (Connection c in Connections)
        {
            var other = c.joint1 == this ? c.joint2 : c.joint1;

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
