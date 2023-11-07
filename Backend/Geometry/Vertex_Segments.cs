﻿using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Vertex
{
    public List<Segment> Connections = new();

    public List<Vertex> Relations = new();

    public Segment Connect(Vertex to, bool updateRelations = true)
    {
        // Don'q connect something twice
        foreach (Segment c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }

        var connection = new Segment(this, to);
        Connections.Add(connection);
        to.Connections.Add(connection);

        Relations.Add(to);
        to.Relations.Add(this);

        reposition();
        to.reposition();

        if (updateRelations) CreateBoardRelationsWith(to, connection);
        return connection;
    }

    public List<Segment> Connect(params Vertex[] joints)
    {
        var cons = new List<Segment>();
        foreach (Vertex joint in joints)
        {
            var doNothing = false;
            // Don'q connect something twice
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

                Relations.Add(joint);
                joint.Relations.Add(this);

                joint.reposition();
                CreateBoardRelationsWith(joint, connection);
            }
        }
        reposition();
        return cons;
    }

    public bool IsConnectedTo(Vertex joint)
    {
        if (this == joint) return false;
        foreach (Segment c in Connections)
        {
            Vertex[] js = new[] { c.joint1, c.joint2 };
            if (js.Contains(joint) && js.Contains(this)) return true;
        }
        return false;
    }

    public Segment? GetConnectionTo(Vertex to)
    {
        foreach (Segment c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }
        return null;
    }

    public void Disconnect(Vertex joint)
    {
        var past = Connections.ToList();
        foreach (Segment c in past)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                Connections.Remove(c);
                Relations.Remove(joint);
            }
        }
        var past2 = joint.Connections.ToList();
        foreach (Segment c in past2)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                joint.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                joint.Connections.Remove(c);
                joint.Relations.Remove(this);
                c.Formula.RemoveAllFollowers();
                c.MiddleFormula.RemoveAllFollowers();
                Segment.all.Remove(c);
                MainWindow.BigScreen.Children.Remove(c);
                foreach (var l in c.OnRemoved) l(this, joint);
            }
        }
    }

    public void Disconnect(params Vertex[] joints)
    {
        foreach (Vertex joint in joints)
        {
            var past = Connections.ToList();
            foreach (Segment c in past)
            {
                if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                {
                    Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    Connections.Remove(c);
                    Relations.Remove(joint);
                }
            }

            var past2 = joint.Connections.ToList();
            foreach (Segment c in past2)
            {
                if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                {
                    joint.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
                    joint.Connections.Remove(c);
                    joint.Relations.Remove(this);
                    c.Formula.RemoveAllFollowers();
                    c.MiddleFormula.RemoveAllFollowers();
                    Segment.all.Remove(c);
                    MainWindow.BigScreen.Children.Remove(c);
                    foreach (var l in c.OnRemoved) l(this, joint);

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
            if (c.joint1 != this) c.joint1.Relations.Remove(c.joint2);
            else c.joint2.Relations.Remove(c.joint1);
            if (c.joint1 != this) c.joint1.RepositionText();
            else c.joint2.RepositionText();

            c.joint1.Roles.RemoveFromRole(Role.SEGMENT_Corner, c); // One of them is `this`
            c.joint2.Roles.RemoveFromRole(Role.SEGMENT_Corner, c); // The other is the second joint

            c.Formula.RemoveAllFollowers();
            c.MiddleFormula.RemoveAllFollowers();
            Segment.all.Remove(c);

            foreach (var l in c.OnRemoved) l(c.joint1, c.joint2);

        }
        Connections.Clear();
        Relations.Clear();
        RepositionText();
    }

    public void CreateBoardRelationsWith(Vertex joint, Segment segment)
    {
        // Basic connection info

        //First check - radius

        if ((joint.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_Center)) || joint.Roles.Has(Role.CIRCLE_Center) && Roles.Has(Role.CIRCLE_On))
        {
            foreach (var circle in joint.Roles.Access<Circle>(Role.CIRCLE_On))
            {
                if (Roles.Has(Role.CIRCLE_Center, circle))
                {
                    segment.Roles.AddToRole(Role.CIRCLE_Radius, circle);
                }
            }
        }
        // Second check - diameter & chord
        if (joint.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_On))
        {
            foreach (var circle in joint.Roles.Access<Circle>(Role.CIRCLE_On))
            {
                if (Roles.Has(Role.CIRCLE_On, circle))
                {
                    if (joint.DistanceTo(this) == circle.radius * 2 && joint.RadiansTo(circle.center) == circle.center.RadiansTo(this))
                    {
                        segment.Roles.AddToRole(Role.CIRCLE_Diameter, circle);
                        /* 
                            This causes a crash, since making a diameter already connects its
                            edges to the circle, which dispatches onMoved on the center, and adding SEGMENT_Center
                            also does this, during onMoved is dispatched, which is disallowed.
                        */
                        //circle.center.Roles.AddToRole(Role.SEGMENT_Center, segment);
                    } 
                    else segment.Roles.AddToRole(Role.CIRCLE_Chord, circle);
                }
            }
        }

        // Third case - connecting a line and forming a Triangle
        foreach (Segment c in Connections.ToArray())
        {
            var other = c.joint1 == this ? c.joint2 : c.joint1;

            

            if (other.IsConnectedTo(joint) /* this.IsConnectedTo(joint) is `true` */) // this joint is connected to other, and the just connected joint is also connected to it -> potentially new Triangle
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
                    _ = new Triangle(this, other, joint);
                }
            }
        }
        // Fourth case - connection a line and forming a quadrilateral
        foreach (Vertex other1 in this.Relations.ToArray())
        {
            if (other1 == joint) continue;
            foreach (Vertex other2 in joint.Relations.ToArray())
            {
                if (other2 == this) continue;
                if (other1 == other2) continue;
                if (other1.IsConnectedTo(other2))
                {
                    var hasQuad = false;
                    var currentQuads = Roles.Access<Quadrilateral>(Role.QUAD_Corner);
                    foreach (var q in currentQuads)
                    {
                        if (q.IsDefinedBy(this, joint, other1, other2))
                        {
                            hasQuad = true;
                            break;
                        }
                    }
                    if (!hasQuad)
                    {
                        var con2 = other1.GetConnectionTo(this);
                        var con3 = other2.GetConnectionTo(joint);
                        if (con2 == null || con3 == null) continue;
                        if (con2.Formula.Intersect(con3.Formula) != null) continue;
                        _ = new Quadrilateral(this, joint, other2, other1);
                    }
                }
            }
        }
    }

    public void UpdateBoardRelations()
    {
        foreach (var v in Relations)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            CreateBoardRelationsWith(v, v.GetConnectionTo(this)); // Must be non--null anyways
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}