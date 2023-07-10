using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

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
        org1X = f.X;
        org1Y = f.Y;
        org2X = t.X;
        org2Y = t.Y;
        this.dataText = dataText;
        text = "" + f.Id + t.Id;

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
            joint1.Provider.EvaluateSuggestions();
            joint2.Provider.EvaluateSuggestions();
            reposition();
        });

        all.Add(this);

        InvalidateVisual();
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
}
