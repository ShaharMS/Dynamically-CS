using Avalonia.Media;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Geometry;
using Dynamically.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Screens;
using Dynamically.Backend.Interfaces;

namespace Dynamically.Backend.Geometry;

public class EllipseBase : DraggableGraphic, IDrawable
{

    public static readonly List<EllipseBase> all = new();

    public Joint focal1;
    public Joint focal2;

    public override bool Draggable
    {
        set
        {
            base.Draggable = value;
            if (ring != null) ring.Draggable = value;
        }
    }

    public double distanceSum;

    internal Ring ring;
    public EllipseBase(Joint f1, Joint f2, double dSum)
    {
        focal1 = f1;
        focal2 = f2;
        distanceSum = dSum;
        ring = new Ring(this);
        ring.Draggable = Draggable;
        all.Add(this);

        focal1.OnMoved.Add((double _, double _, double _, double _) => { ring.InvalidateVisual(); });
        focal2.OnMoved.Add((double _, double _, double _, double _) => { ring.InvalidateVisual(); });
        focal1.OnDragged.Add((double _, double _, double _, double _) => { reposition(); });
        focal2.OnDragged.Add((double _, double _, double _, double _) => { reposition(); });

        MainWindow.BigScreen.Children.Insert(0, this);
        MainWindow.BigScreen.Children.Insert(0, ring);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
    }

    public void reposition()
    {
    }
}

internal class Ring : DraggableGraphic
{
    public static readonly List<Ring> all = new();
    private EllipseBase ellipse;

    public Ring(EllipseBase el)
    {
        all.Add(this);
        ellipse = el;

        OnMoved.Add((double _, double _, double _, double _) =>
        {
            double mx = BigScreen.MouseX, my = BigScreen.MouseY;
            var parentOffset = this.GetPosition();
            mx -= parentOffset.X;
            my -= parentOffset.Y;
            this.SetPosition(0, 0);
            ellipse.distanceSum = new Point(ellipse.focal1.X, ellipse.focal1.Y).DistanceTo(new Point(mx, my)) + new Point(ellipse.focal2.X, ellipse.focal2.Y).DistanceTo(new Point(mx, my));
            InvalidateVisual();

            foreach (var l in ellipse.OnMoved) l(X, Y, X, Y);
        });
        OnDragged.Add(MainWindow.regenAll);

    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var info = ConvertFociToEllipse(ellipse.focal1.X, ellipse.focal1.Y, ellipse.focal2.X, ellipse.focal2.Y);

        var pen = new Pen(new SolidColorBrush(Colors.Black), 4);
        context.DrawEllipse(null, pen, new Point(info.X + info.Width / 2, info.Y + info.Height / 2), info.Width / 2, info.Height / 2);
    }

    EllipseData ConvertFociToEllipse(double focus1X, double focus1Y, double focus2X, double focus2Y)
    {
        var distance = Math.Sqrt(Math.Pow(focus2X - focus1X, 2) + Math.Pow(focus2Y - focus1Y, 2));
        var semiMajorAxis = ellipse.distanceSum / 2;
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

    public override double Area()
    {
        return 1;
    }
}

struct EllipseData
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
}

