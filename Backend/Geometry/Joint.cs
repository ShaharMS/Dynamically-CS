using Avalonia.Media;
using Dynamically.Formulas;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Menus.ContextMenus;

namespace Dynamically.Backend.Geometry;

public class Joint : DraggableGraphic, IDrawable
{
    public static List<Joint> all = new();

    public char id = ' ';
    public List<Connection> connections = new();

    /// <summary>
    /// This is used to associate joints with the shapes they're inside. <br/>
    /// for example, given a circle, and a triangle formed with one joint being the center, 
    /// the joint's <c>partOf</c> array would contain the circle and the triangle. <br />
    /// When working together with geometric position, can provide a nice suggestions UI for what to do next.
    /// </summary>
    public List<DraggableGraphic> PartOf = new();

    public Color outlineColor;
    public Color fillColor;

    List<Formula> _geometricPosition = new();
    public List<Formula> geometricPosition
    {
        get => _geometricPosition;
        set
        {
            _geometricPosition = value;
            foreach (var position in value)
            {
                ((ChangeListener)position).OnChange.Add(() =>
                {
                    var newPos = position.GetClosestOnFormula(X, Y);
                    if (newPos.HasValue)
                    {
                        X = newPos.Value.X;
                        Y = newPos.Value.Y;
                        foreach (var l in OnMoved) l(X, Y, X, Y);
                    }

                    foreach (Connection c in Connection.all) c.InvalidateVisual();
                });

                ((ChangeListener)position).OnMove.Add((curX, curY, preX, preY) =>
                {
                    X = X - preX + curX;
                    Y = Y - preY + curY;
                    foreach (var l in OnMoved) l(X, Y, X, Y);
                    foreach (Connection c in Connection.all) c.InvalidateVisual();
                });
            }
        }
    }
    public Joint(double x, double y, char id = 'A')
    {
        all.Add(this);

        this.id = id;

        X = x;
        Y = y;

        ContextMenu = new JointContextMenu(this);

        OnMoved.Add((double _, double _, double _, double _) =>
        {
            if (geometricPosition.Count != 0)
            {
                foreach (var position in geometricPosition)
                {
                    var newPos = position.GetClosestOnFormula(X, Y);
                    if (newPos.HasValue)
                    {
                        X = newPos.Value.X;
                        Y = newPos.Value.Y;
                    }
                }
            }
            foreach (Connection c in Connection.all) c.InvalidateVisual();
        });
        OnDragged.Add((double _, double _, double _, double _) =>
        {
            foreach (Connection c in Connection.all) c.reposition();
        });

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var brush = new SolidColorBrush(Colors.White);
        var pen = new Pen(new SolidColorBrush(Colors.Black), 4);
        context.DrawEllipse(brush, pen, new Point(0, 0), 10, 10);
    }

    public void reposition() { }

    public Connection Connect(Joint to, string connectionText = "")
    {
        // Don't connect something twice
        foreach (Connection c in connections.Concat(to.connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }

        var connection = new Connection(this, to, connectionText);
        connections.Add(connection);
        to.connections.Add(connection);
        return connection;
    }

    public List<Connection> Connect(params Joint[] joints)
    {
        var cons = new List<Connection>();
        foreach (Joint joint in joints)
        {
            var doNothing = false;
            // Don't connect something twice
            foreach (Connection c in connections.Concat(joint.connections))
            {
                if ((c.joint1 == this && c.joint2 == joint) || (c.joint2 == this && c.joint1 == joint))
                    doNothing = true;
            }

            if (!doNothing)
            {
                var connection = new Connection(this, joint);
                connections.Add(connection);
                cons.Add(connection);
                joint.connections.Add(connection);
            }
        }
        return cons;
    }

    public void Disconnect(Joint joint)
    {
        foreach (Connection c in connections)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                connections.Remove(c);
        }

        foreach (Connection c in joint.connections)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                joint.connections.Remove(c);
        }
    }



    public static implicit operator Point(Joint joint) { return new Point(joint.X, joint.Y); }
    public static implicit operator Joint(Point point) { return new Joint(point.X, point.Y); }
}
