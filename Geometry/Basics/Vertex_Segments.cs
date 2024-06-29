using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Geometry.Basics;

public partial class Vertex
{
    public List<Vertex> Relations = new();

    public Segment Connect(Vertex to, bool updateRelations = true)
    {
        // Don'q connect something twice
        if (Relations.Contains(to)) return to.GetSegmentTo(this)!;

        var segment = new Segment(this, to);

        Relations.Add(to);
        to.Relations.Add(this);

        Reposition();
        to.Reposition();

        if (updateRelations) CreateBoardRelationsWith(to, segment);
        return segment;
    }

    public List<Segment> Connect(params Vertex[] vertices) => vertices.Select(x => Connect(x)).ToList();

    public bool HasSegmentWith(Vertex vertex) => Relations.Contains(vertex);

    public Segment? GetSegmentTo(Vertex to) => Segment.All.FirstOrDefault(x => x.Vertex1 == this && x.Vertex2 == to || x.Vertex1 == to && x.Vertex2 == this);

    public void Disconnect(Vertex vertex)
    {
        var c = GetSegmentTo(vertex)!;
        if (c == null) return;
        foreach (var v in new Vertex[] { c.Vertex1, c.Vertex2 })
        {
            var other = v == c.Vertex1 ? c.Vertex2 : c.Vertex1;

            v.Roles.RemoveFromRole(Role.SEGMENT_Corner, c);
            v.Relations.Remove(other);
            c.Formula.RemoveAllFollowers();
            c.MiddleFormula.RemoveAllFollowers();
        }
        Segment.All.Remove(c);
        ParentBoard.RemoveChild(c);
        foreach (var l in c.OnRemoved) l(this, vertex);
    }

    public void Disconnect(params Vertex[] vertices)
    {
        foreach (var v in vertices)
        {
            Disconnect(v);
        }
    }

    public void DisconnectAll() => Disconnect(Relations.ToArray());

    public void CreateBoardRelationsWith(Vertex vertex, Segment segment)
    {
        // Basic seg info

        //First check - Radius

        if ((vertex.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_Center)) || vertex.Roles.Has(Role.CIRCLE_Center) && Roles.Has(Role.CIRCLE_On))
        {
            foreach (var circle in vertex.Roles.Access<Circle>(Role.CIRCLE_On))
            {
                if (Roles.Has(Role.CIRCLE_Center, circle))
                {
                    segment.Roles.AddToRole(Role.CIRCLE_Radius, circle);
                }
            }
        }
        // Second check - diameter & chord
        if (vertex.Roles.Has(Role.CIRCLE_On) && Roles.Has(Role.CIRCLE_On))
        {
            foreach (var circle in vertex.Roles.Access<Circle>(Role.CIRCLE_On))
            {
                if (Roles.Has(Role.CIRCLE_On, circle))
                {
                    if (vertex.DistanceTo(this) == circle.Radius * 2 && vertex.RadiansTo(circle.Center) == circle.Center.RadiansTo(this))
                    {
                        segment.Roles.AddToRole(Role.CIRCLE_Diameter, circle);
                        /* 
                            This causes a crash, since making a diameter already connects its
                            edges to the circle, which dispatches onMoved on the Center, and adding SEGMENT_Center
                            also does this, during onMoved is dispatched, which is disallowed.
                        */
                        //circle.Center.Roles.AddToRole(Role.SEGMENT_Center, seg);
                    } 
                    else segment.Roles.AddToRole(Role.CIRCLE_Chord, circle);
                }
            }
        }

        // Third case - connecting a line and forming a Triangle
        foreach (var v in Relations.ToArray())
        {
            var seg = GetSegmentTo(v)!;
            var other = seg.Vertex1 == this ? seg.Vertex2 : seg.Vertex1;
            if (other.HasSegmentWith(vertex) /* this.HasSegmentWith(vertex) is `true` */) // this vertex is connected to other, and the just connected vertex is also connected to it -> potentially new Triangle
            {
                if (Triangle.Exists(this, other, vertex)) continue;
                _ = new Triangle(this, other, vertex);
            }
        }
        // Fourth case - connecting a line and forming a quadrilateral
        foreach (Vertex other1 in Relations.Where(x => x != vertex))
        {
            foreach (Vertex other2 in vertex.Relations.Where(x => x != this))
            {
                if (!other1.HasSegmentWith(other2)) continue;
                if (Quadrilateral.All.Any(x => x.IsDefinedBy(this, vertex, other1, other2))) continue;
                if (other1.GetSegmentTo(other2)!.Formula.Intersects(segment.Formula)) continue;
                _ = new Quadrilateral(this, vertex, other1, other2);
            }
        }
    }

    public void UpdateBoardRelations()
    {
        foreach (var v in Relations)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            CreateBoardRelationsWith(v, v.GetSegmentTo(this)); // Must be non--null anyways
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
