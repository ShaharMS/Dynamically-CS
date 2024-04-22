using Avalonia;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Menus.ContextMenus;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Vertex : IDrawable, IContextMenuSupporter<VertexContextMenuProvider>, IStringifyable, ISupportsAdjacency, ISelectable, IMovementFreezable
{
    /// <summary>
    /// This is used to associate joints with the shapes & formulas they're on. <br/>
    /// for example, given a circle, and a Triangle formed with one joint being the Center, 
    /// the joint's <c>Roles</c> map would contain the circle and the Triangle. <br />
    /// </summary>
    public RoleMap Roles { get; set; }

    public VertexContextMenuProvider Provider { get; }

    public void Reposition()
    {
        // Position is validated, now redraw connections & place text
        // text is placed in the middle of the biggest angle at the distance of fontSize + 4
        foreach (Segment c in Connections)
        {
            c.InvalidateVisual();
            if (c.Vertex1 == this) c.Vertex2.RepositionText();
            else c.Vertex1.RepositionText();
        }

        RepositionText();
    }

    public override bool Overlaps(Point point)
    {
        return X - Width / 2 < point.X && Y - Width / 2 + MainWindow.Instance.MainBoard.GetPosition().Y < point.Y && X + Width / 2 > point.X && Y + MainWindow.Instance.MainBoard.GetPosition().Y + Height / 2 > point.Y;
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
}
