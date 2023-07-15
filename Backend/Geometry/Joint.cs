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
using Avalonia.Controls;
using Dynamically.Shapes;
using System.Runtime.CompilerServices;
using Avalonia.Styling;
using Dynamically.Screens;

namespace Dynamically.Backend.Geometry;

public class Joint : DraggableWithContextInfo, IDrawable, IContextMenuSupporter
{
    public static List<Joint> all = new();

    public char _id = 'A';
    public char Id
    {
        get => _id;
        set
        {
            IdDisplay.Content = value.ToString();
            _id = value;
        }
    }

    public List<Connection> Connections = new();

    public List<Action<double, double>> OnRemoved = new();

    /// <summary>
    /// This is used to associate joints with the shapes they're inside. <br/>
    /// for example, given a circle, and a triangle formed with one joint being the center, 
    /// the joint's <c>partOf</c> array would contain the circle and the triangle. <br />
    /// When working together with geometric position, can provide a nice suggestions UI for what to do next.
    /// </summary>
    public Dictionary<Role, List<DraggableGraphic>> PartOf
    {
        get => _partOf;
        set => _partOf = value;
    }
    Dictionary<Role, List<DraggableGraphic>> _partOf = new();


    public JointContextMenuProvider Provider;

    public Label IdDisplay;

    public bool GotRemoved
    {
        get => !MainWindow.BigScreen.Children.Contains(this);
    }

    List<FormulaBase> _geometricPosition = new();
    public List<FormulaBase> GeometricPosition
    {
        get => _geometricPosition;
        set
        {
            _geometricPosition = value;
            foreach (var position in value)
            {
                position.OnChange.Add(() =>
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

                position.OnMove.Add((curX, curY, preX, preY) =>
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

        IdDisplay = new Label()
        {
            FontSize = 20,
            FontWeight = FontWeight.DemiBold,
            Background = null,
            BorderThickness = new Thickness(0, 0, 0, 0),
            Width = 20
        };

        this.Id = id;




        MainWindow.BigScreen.Children.Add(IdDisplay);

        X = x;
        Y = y;
        ContextMenu = new ContextMenu();
        Provider = new JointContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;


        OnMoved.Add((double _, double _, double _, double _) =>
        {
            // Validate position
            if (GeometricPosition.Count != 0)
            {
                var removeQueue = new List<FormulaBase>();
                foreach (var position in GeometricPosition)
                {
                    if (position.queueRemoval)
                    {
                        removeQueue.Add(position);
                        continue;
                    }
                    var newPos = position.GetClosestOnFormula(X, Y);
                    if (newPos.HasValue)
                    {
                        X = newPos.Value.X;
                        Y = newPos.Value.Y;
                    }
                }
                foreach (var item in removeQueue) GeometricPosition.Remove(item);
            }
            // Position is validated, now redraw connections & place text
            // text is placed in the middle of the biggest angle at the distance of fontSize + 4
            foreach (Connection c in Connections)
            {
                c.InvalidateVisual();
                if (c.joint1 == this) c.joint2.RepositionText();
                else c.joint1.RepositionText();
            }
            RepositionText();
        });
        OnDragged.Add((double _, double _, double _, double _) =>
        {
            foreach (Connection c in Connections) c.reposition();
        });

        InvalidateVisual();
        RepositionText();
    }

    public void RepositionText()
    {
        var d = IdDisplay.FontSize + 4;
        var degs = new double[Connections.Count + 1];
        var i = 0;
        if (Connections.Count >= 1)
        {
            foreach (Connection c in Connections)
            {
                if (c.joint1 == this)
                {
                    degs[i] = new Point(c.joint1.X, c.joint1.Y).DegreesTo(c.joint2);
                }
                else if (c.joint2 == this)
                {
                    degs[i] = new Point(c.joint2.X, c.joint2.Y).DegreesTo(c.joint1);
                }
                i++;
            }
        }

        degs[Connections.Count] = 360 + degs[0];
        List<double> ds = degs.ToList();
        ds.Sort();
        degs = ds.ToArray();

        var biggestGap = double.MinValue;
        var previous = degs[0];
        var degStart = degs[0];
        for (int j = 1; j < degs.Length; j++)
        {
            double deg = degs[j];
            if (deg - previous > biggestGap)
            {
                biggestGap = deg - previous;
                degStart = previous;
            }
            previous = deg;
        }

        var finalAngle = degStart + biggestGap / 2;
        Canvas.SetLeft(IdDisplay, X + d * Math.Cos(finalAngle * (Math.PI / 180.0)) - 20 / 2);
        Canvas.SetTop(IdDisplay, Y + d * Math.Sin(finalAngle * (Math.PI / 180.0)) - 30 / 2);
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
        foreach (Connection c in Connections.Concat(to.Connections))
        {
            if ((c.joint1 == this && c.joint2 == to) || (c.joint2 == this && c.joint1 == to)) return c;
        }

        var connection = new Connection(this, to, connectionText);
        Connections.Add(connection);
        to.Connections.Add(connection);

        RepositionText();
        to.RepositionText();

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
                Connections.Add(connection);
                cons.Add(connection);
                joint.Connections.Add(connection);
                joint.RepositionText();
            }
        }
        RepositionText();
        return cons;
    }

    public void Disconnect(Joint joint)
    {
        var past = Connections.ToList();
        foreach (Connection c in past)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
                Connections.Remove(c);
                MainWindow.BigScreen.Children.Remove(c);
            }
        }
        var past2 = joint.Connections.ToList();
        foreach (Connection c in past2)
        {
            if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
            {
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
                    Connections.Remove(c);
                    MainWindow.BigScreen.Children.Remove(c);
                }
            }

            var past2 = joint.Connections.ToList();
            foreach (Connection c in past2)
            {
                if (c.joint1 == this && c.joint2 == joint || c.joint1 == joint && c.joint2 == this)
                {
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
        }
        Connections.Clear();
        RepositionText();
    }

    public void RemoveFromBoard()
    {
        double sx = X, sy = Y;
        DisconnectAll();

        MainWindow.BigScreen.Children.Remove(this);
        MainWindow.BigScreen.Children.Remove(IdDisplay);

        foreach (var pair in PartOf)
        {
            foreach (var draggable in pair.Value)
            {
                switch (pair.Key)
                {
                    case Role.TRIANGLE_Joint:
                        if (draggable is Triangle)
                        {
                            ((Triangle)draggable).Dismantle();
                        }
                        break;
                    case Role.CIRCLE_Center:
                        if (draggable is Circle)
                        {
                            ((Circle)draggable).Dismantle();
                        }
                        break;
                    default:
                        Log.Write($"{pair.Key.ToString()} Unimplemented");
                        break;
                } 
            }
        }

        foreach (var r in OnRemoved)
        {
            r(sx, sy);
        }
    }

    public static implicit operator Point(Joint joint) { return new Point(joint.X, joint.Y); }
    public static implicit operator Joint(Point point) { return new Joint(point.X, point.Y); }
}
