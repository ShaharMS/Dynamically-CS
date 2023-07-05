using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using StaticExtensions;
using GeometryBackend;
using GraphicsBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically;
using Dynamically.Formulas;

namespace GeometryBackend;

public class Joint : DraggableGraphic, IDrawable
{
    public static List<Joint> all = new List<Joint>();

    public char id = ' ';
    public List<Connection> connections = new List<Connection>();

    public Color outlineColor;
    public Color fillColor;

    List<Formula> _geometricPosition = new List<Formula>();
    public List<Formula> geometricPosition
    {
        get => _geometricPosition;
        set
        {
            _geometricPosition = value;
            foreach (var position in value)
            {
                ((ChangeListener)position).onChange.Add(() =>
                {
                    var newPos = position.GetClosestOnFormula(x, y);
                    if (newPos.HasValue)
                    {
                        x = newPos.Value.X;
                        y = newPos.Value.Y;
                        foreach (var l in onMoved) l(x, y, x, y);
                    }

                    foreach (Connection c in Connection.all) c.InvalidateVisual();
                });

                ((ChangeListener)position).onMove.Add((curX, curY, preX, preY) =>
                {
                    x = x - preX + curX;
                    y = y - preY + curY;
                    foreach (var l in onMoved) l(x, y, x, y);
                    foreach (Connection c in Connection.all) c.InvalidateVisual();
                });
            }
        }
    }
    public Joint(double x, double y, char id = 'A')
    {
        all.Add(this);

        this.id = id;

        this.x = x;
        this.y = y;

        onMoved.Add((double px, double py, double mx, double my) =>
        {
            if (geometricPosition.Count != 0)
            {
                foreach (var position in geometricPosition)
                {
                    var newPos = position.GetClosestOnFormula(this.x, this.y);
                    if (newPos.HasValue)
                    {
                        this.x = newPos.Value.X;
                        this.y = newPos.Value.Y;
                    }
                }
            }
            foreach (Connection c in Connection.all) c.InvalidateVisual();
        });
        onDragged.Add((double cx, double cy, double prx, double pry) =>
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

    public Joint Connect(Joint to, string connectionText = "")
    {
        // Dont connect something twice
        foreach (Connection c in connections.Concat(to.connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return this;
        }

        var connection = new Connection(this, to, connectionText);
        connections.Add(connection);
        to.connections.Add(connection);
        return this;
    }

    public Joint Connect(params Joint[] joints)
    {
        foreach (Joint joint in joints)
        {
            var doNothing = false;
            // Dont connect something twice
            foreach (Connection c in connections.Concat(joint.connections))
            {
                if ((c.joint1 == this && c.joint2 == joint) || (c.joint2 == this && c.joint1 == joint))
                    doNothing = true;
            }

            if (!doNothing)
            {
                var connection = new Connection(this, joint);
                connections.Add(connection);
                joint.connections.Add(connection);
            }
        }
        return this;
    }

    public Joint Disconnect(Joint joint)
    {
        foreach (Connection c in this.connections)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                connections.Remove(c);
        }

        foreach (Connection c in joint.connections)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                joint.connections.Remove(c);
        }
        return this;
    }



    public static implicit operator Point(Joint joint) { return new Point(joint.x, joint.y); }
    public static implicit operator Joint(Point point) { return new Joint(point.X, point.Y); }
}

public class Connection : DraggableGraphic, IDrawable
{
    public static List<Connection> all = new List<Connection>();

    public Joint joint1;
    public Joint joint2;

    public Color outlineColor;
    public string text = "";
    public string dataText = "";

    double org1X;
    double org1Y;
    double org2X;
    double org2Y;

    public Connection(Joint f, Joint t, string dataText = "")
    {
        joint1 = f;
        joint2 = t;
        org1X = f.x;
        org1Y = f.y;
        org2X = t.x;
        org2Y = t.y;
        this.dataText = dataText;
        text = "" + f.id + t.id;

        onMoved.Add((double px, double py, double mx, double my) =>
        {
            joint1.x = org1X + x;
            joint2.x = org2X + x;
            joint1.y = org1Y + y;
            joint2.y = org2Y + y;
            this.x = 0;
            this.y = 0;
            foreach (var l in joint1.onMoved) l(joint1.x, joint1.y, mx, my);
            foreach (var l in joint2.onMoved) l(joint2.x, joint2.y, mx, my);
            InvalidateVisual();
        });

        onDragged.Add((double cx, double cy, double prx, double pry) =>
        {
            foreach (Connection c in Connection.all) c.reposition();
        });

        all.Add(this);

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var pen = new Pen(new SolidColorBrush(Colors.Black), 4);
        context.DrawLine(pen, new Point(joint1.x, joint1.y), new Point(joint2.x, joint2.y));
        // padding for easier dragging
        var pen2 = new Pen(new SolidColorBrush(Colors.Black, 0.01));
        context.DrawLine(pen2, new Point(joint1.x, joint1.y), new Point(joint2.x, joint2.y));

    }

    public void reposition()
    {
        org1X = joint1.x;
        org1Y = joint1.y;
        org2X = joint2.x;
        org2Y = joint2.y;
    }
}
public class EllipseBase : DraggableGraphic, IDrawable
{

    public static List<EllipseBase> all = new List<EllipseBase>();

    public Joint focal1;
    public Joint focal2;

    public double distanceSum;

    public Action onDistanceSumChange = () => { };

    public EllipseBase(Joint f1, Joint f2, double dSum)
    {
        focal1 = f1;
        focal2 = f2;
        distanceSum = dSum;

        all.Add(this);

        focal1.onMoved.Add((double _, double _, double _, double _) => { InvalidateVisual(); });
        focal2.onMoved.Add((double _, double _, double _, double _) => { InvalidateVisual(); });
        focal1.onDragged.Add((double _, double _, double _, double _) => { reposition(); });
        focal2.onDragged.Add((double _, double _, double _, double _) => { reposition(); });


        onMoved.Add((double _, double _, double mx, double my) =>
        {
            Margin = new Thickness(0, 0, 0, 0);
            distanceSum = new Point(focal1.x, focal1.y).DistanceTo(new Point(mx, my)) + new Point(focal2.x, focal2.y).DistanceTo(new Point(mx, my));
            onDistanceSumChange();
            InvalidateVisual();
        });

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var info = ConvertFocalsToEllipse(focal1.x, focal1.y, focal2.x, focal2.y);

        var pen = new Pen(new SolidColorBrush(Colors.Black), 4);
        context.DrawEllipse(null, pen, new Point(info.X + info.Width / 2, info.Y + info.Height / 2), info.Width / 2, info.Height / 2);
    }
    public void reposition() { }

    EllipseData ConvertFocalsToEllipse(double focus1X, double focus1Y, double focus2X, double focus2Y)
    {
        var distance = Math.Sqrt(Math.Pow(focus2X - focus1X, 2) + Math.Pow(focus2Y - focus1Y, 2));
        var semiMajorAxis = distanceSum / 2;
        var semiMinorAxis = Math.Sqrt(Math.Pow(semiMajorAxis, 2) - Math.Pow(distance / 2, 2));

        var width = 2 * semiMajorAxis;
        var height = 2 * semiMinorAxis;
        var x = (focus1X + focus2X) / 2 - width / 2;
        var y = (focus1Y + focus2Y) / 2 - height / 2;
        return new EllipseData
        {
            Width = width,
            Height = height,
            X = x,
            Y = y
        };
    }
}

struct EllipseData
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}
