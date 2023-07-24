using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public class Connection : DraggableGraphic, IDrawable
{
    public static readonly List<Connection> all = new();

    public Joint joint1;
    public Joint joint2;

    public Color outlineColor;
    public string text = "";
    public string dataText = "";

    double org1X;
    double org1Y;
    double org2X;
    double org2Y;

    public SegmentFormula Formula { get; set; }

    public Connection(Joint f, Joint t, string dataText = "")
    {
        joint1 = f;
        joint2 = t;
        joint1.Roles.AddToRole(Role.SEGMENT_Corner, this);
        joint2.Roles.AddToRole(Role.SEGMENT_Corner, this);
        org1X = f.X;
        org1Y = f.Y;
        org2X = t.X;
        org2Y = t.Y;
        this.dataText = dataText;
        text = "" + f.Id + t.Id;
        Formula = new SegmentFormula(f, t);

        OnMoved.Add((double px, double py, double mx, double my) =>
        {
            joint1.X = org1X + X;
            joint2.X = org2X + X;
            joint1.Y = org1Y + Y;
            joint2.Y = org2Y + Y;
            this.X = 0;
            this.Y = 0;
            foreach (var l in joint1.OnMoved) l(joint1.X, joint1.Y, mx, my);
            foreach (var l in joint2.OnMoved) l(joint2.X, joint2.Y, mx, my);
            InvalidateVisual();
        });

        OnDragged.Add((double cx, double cy, double prx, double pry) =>
        {
            joint1.Provider.EvaluateRecommendations();
            joint2.Provider.EvaluateRecommendations();
            foreach (var c in joint1.Connections) c.reposition();
            foreach (var c in joint2.Connections) c.reposition();
        });

        all.Add(this);

        MainWindow.BigScreen.Children.Insert(0, this);
        InvalidateVisual();
    }

    public void UpdateFormula()
    {
        if (Formula == null) return;
        Formula.x1 = joint1.X;
        Formula.y1 = joint1.Y;
        Formula.x2 = joint2.X;
        Formula.y2 = joint2.Y;
    }

    public double Length
    { 
        get => Math.Sqrt(Math.Pow(joint2.X  - joint1.X, 2) + Math.Pow(joint2.Y - joint1.Y, 2));
        set
        {
            var ray = new RayFormula(joint1, joint2);
            var p2Arr = ray.GetPointsByDistanceFrom(joint1, value);
            if (p2Arr[0].DistanceTo(joint2) < p2Arr[1].DistanceTo(joint2))
            {
                joint2.X = p2Arr[0].X;
                joint2.Y = p2Arr[0].Y;
            }
            else
            {
                joint2.X = p2Arr[1].X;
                joint2.Y = p2Arr[1].Y;
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var pen = new Pen(new SolidColorBrush(Colors.Black), 4);
        context.DrawLine(pen, new Point(joint1.X, joint1.Y), new Point(joint2.X, joint2.Y));
        // padding for easier dragging
        var pen2 = new Pen(new SolidColorBrush(Colors.Black, 0.01));
        context.DrawLine(pen2, new Point(joint1.X, joint1.Y), new Point(joint2.X, joint2.Y));

    }

    public void reposition()
    {
        org1X = joint1.X;
        org1Y = joint1.Y;
        org2X = joint2.X;
        org2Y = joint2.Y;
    }

    public override string ToString()
    {
        return ((char)Math.Min(joint1.Id, joint2.Id)).ToString() + ((char)Math.Max(joint1.Id, joint2.Id)).ToString();
    }

    public override double Area()
    {
        return 1;
    }

#pragma warning disable IDE1006
    public void __updateFormula(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v; // Supress unused params warning
        UpdateFormula();
    }

    public void __reposition(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v; // Supress unused params warning
        reposition();
    }
#pragma warning restore IDE1006
}
