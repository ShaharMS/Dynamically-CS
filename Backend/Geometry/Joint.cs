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
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;

namespace Dynamically.Backend.Geometry;

public class Joint : DraggableGraphic, IDrawable, IContextMenuSupporter
{
    public static readonly List<Joint> all = new();

    public static readonly double GraphicalRadius = 10;

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
    /// This is used to associate joints with the shapes & formulas they're on. <br/>
    /// for example, given a circle, and a triangle formed with one joint being the center, 
    /// the joint's <c>Roles</c> map would contain the circle and the triangle. <br />
    /// </summary>
    public RoleMap Roles { get; set; }

    public JointContextMenuProvider Provider;

    public Label IdDisplay;

    public bool GotRemoved
    {
        get => !MainWindow.BigScreen.Children.Contains(this);
    }

    public bool Hidden {
        get => !IsVisible;
        set {
            IdDisplay.IsVisible = IsVisible = !value;
        }
    }

    public Joint(Point p) : this(p.X, p.Y) { }
    public Joint(double x, double y, char id = '_')
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
        
        if (id == '_') IDGenerator.GenerateFor(this);
        else this.Id = id;

        Roles = new RoleMap(this);



        MainWindow.BigScreen.Children.Add(IdDisplay);

        X = x;
        Y = y;
        ContextMenu = new ContextMenu();
        Provider = new JointContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;


        OnMoved.Add((double _, double _, double _, double _) => reposition());

        InvalidateVisual();
        RepositionText();

        MainWindow.BigScreen.Children.Add(this);
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
        Width = Height = GraphicalRadius * 2;
    }
    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var brush = new SolidColorBrush(Colors.White);
        var pen = new Pen(new SolidColorBrush(Colors.Black), GraphicalRadius / 2.5);
        context.DrawEllipse(brush, pen, new Point(0, 0), GraphicalRadius, GraphicalRadius);
    }

    public void reposition()
    {
        // Position is validated, now redraw connections & place text
        // text is placed in the middle of the biggest angle at the distance of fontSize + 4
        foreach (Connection c in Connections)
        {
            c.InvalidateVisual();
            if (c.joint1 == this) c.joint2.RepositionText();
            else c.joint1.RepositionText();
        }
        RepositionText();
        Provider.Regenerate();
    }

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

        reposition();
        to.reposition();

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

    public void RemoveFromBoard()
    {
        double sx = X, sy = Y;
        all.Remove(this);
        DisconnectAll();
        IDGenerator.Remove(this);

        MainWindow.BigScreen.Children.Remove(this);
        MainWindow.BigScreen.Children.Remove(IdDisplay);

        Roles.Clear();
        OnMoved.Clear();
        OnDragged.Clear();

        foreach (var r in OnRemoved)
        {
            r(sx, sy);
        }

        OnRemoved.Clear();
    }

    public override string ToString()
    {
        return Id + "";
    }

    public override bool Overlaps(Point point)
    {
        return X - Width / 2 < point.X && Y - Width / 2 + MainWindow.BigScreen.GetPosition().Y < point.Y && X + Width / 2 > point.X && Y + MainWindow.BigScreen.GetPosition().Y + Height / 2 > point.Y;
    }

    public override double Area()
    {
        return 0;
    }

    public static implicit operator Point(Joint joint) { return new Point(joint.X, joint.Y); }
    public static implicit operator Joint(Point point) { return new Joint(point.X, point.Y); }
}
