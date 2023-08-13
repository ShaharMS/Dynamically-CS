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
using Dynamically.Design;

namespace Dynamically.Backend.Geometry;

public partial class Joint : DraggableGraphic, IDrawable, IContextMenuSupporter, IStringifyable
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
            Width = 25
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
            foreach (Segment c in Connections)
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
        var pen = new Pen(new SolidColorBrush(Colors.Black), UIDesign.JointGraphicCircleRadius / 2.5);
        context.DrawEllipse(brush, pen, new Point(0, 0), UIDesign.JointGraphicCircleRadius, UIDesign.JointGraphicCircleRadius);
        context.FillRectangle(new SolidColorBrush(Colors.Red), new Rect(-1, -1, 2, 2));
    }

    public void reposition()
    {
        // Position is validated, now redraw connections & place text
        // text is placed in the middle of the biggest angle at the distance of fontSize + 4
        foreach (Segment c in Connections)
        {
            c.InvalidateVisual();
            if (c.joint1 == this) c.joint2.RepositionText();
            else c.joint1.RepositionText();
        }
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
    public override bool Overlaps(Point point)
    {
        return X - Width / 2 < point.X && Y - Width / 2 + MainWindow.BigScreen.GetPosition().Y < point.Y && X + Width / 2 > point.X && Y + MainWindow.BigScreen.GetPosition().Y + Height / 2 > point.Y;
    }

    public override double Area()
    {
        return 0;
    }

    public override string ToString()
    {
        return Id + "";
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return "Joint " + Id;
    }

    public static implicit operator Point(Joint joint) { return new Point(joint.X, joint.Y); }
    public static implicit operator Joint(Point point) { return new Joint(point.X, point.Y); }
}
