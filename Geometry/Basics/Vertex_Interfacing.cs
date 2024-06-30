using Avalonia;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using Dynamically.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Backend.Roles;
using Dynamically.Backend.Geometry;

namespace Dynamically.Geometry.Basics;

public partial class Vertex : IDrawable, IContextMenuSupporter<VertexContextMenuProvider>, IStringifyable, ISupportsAdjacency, ISelectable, IMovementFreezable, IHasFormula<PointFormula>, ICanFollowFormula
{
    public override double X { get => base.X; set { if (DispatchingOnMovedEvents) throw new Exception($"Cannot edit Vertex {this}'s X position in an OnMoved function."); else if (!Anchored) base.X = value; } }
    public override double Y { get => base.Y; set { if (DispatchingOnMovedEvents) throw new Exception($"Cannot edit Vertex {this}'s Y position in an OnMoved function."); else if (!Anchored) base.Y = value; } }

    List<Func<double, double, (double X, double Y)>> _positioningByFormula = new();
    public List<Func<double, double, (double X, double Y)>> PositioningByFormula => _positioningByFormula;

    /// <summary>
    /// This is used to associate vertices with the shapes & formulas they're on. <br/>
    /// for example, given a circle, and a Triangle formed with one vertex being the Center, 
    /// the vertice's <c>Roles</c> map would contain the circle and the Triangle. <br />
    /// </summary>
    public RoleMap Roles { get; set; }

    public PointFormula Formula { get; set; }

    public VertexContextMenuProvider Provider { get; }

    public void Reposition()
    {
        // Position is validated, now redraw connections & place text
        // text is placed in the middle of the biggest angle at the distance of fontSize + 4
        foreach (var vertex in Relations)
        {
            vertex.GetSegmentTo(this)!.Reposition();
            vertex.RepositionText();
        }

        RepositionText();
    }

    public override bool Overlaps(Point point)
    {
        return X - Width / 2 < point.X && Y - Width / 2 < point.Y && X + Width / 2 > point.X && Y + Height / 2 > point.Y;
    }

    public override double Area()
    {
        return 1;
    }

    public override string ToString()
    {
        return Id + "";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return "Vertex " + Id;
    }

    public bool EncapsulatedWithin(Rect rect)
    {
        return rect.Contains(this);
    }

    public bool IsMovable()
    {
        if (Roles.Has(Role.CIRCLE_Center))
        {
            var circs = Roles.Access<Circle>(Role.CIRCLE_Center);
            foreach (var c in circs)
            {
                foreach (var t in Triangle.All)
                {
                    if (t.Circumcircle == c && t.Incircle != null && t.Incircle.Center.Anchored) return false; // Case 1.
                }
            }
        }
        if (Roles.Has(Role.TRIANGLE_Corner))
        {
            var tris = Roles.Access<Triangle>(Role.TRIANGLE_Corner);
            foreach (var t in tris)
            {
                if (t.Incircle != null && t.Incircle.Center.Anchored) return false; // Case 2.
            }
        }

        return true;
    }

    public bool Contains(Vertex vertex)
    {
        return this == vertex;
    }

    public bool Contains(Segment segment)
    {
        return false;
    }

    public bool HasMounted(Vertex vertex)
    {
        return Formula.Followers.Contains(vertex);
    }

    public bool HasMounted(Segment segment)
    {
        return Formula.Followers.Contains(segment.Vertex1) && Formula.Followers.Contains(segment.Vertex2);
    }


}