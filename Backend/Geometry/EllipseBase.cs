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
using Dynamically.Design;

namespace Dynamically.Backend.Geometry;

public class EllipseBase : DraggableGraphic, IDrawable
{

    public static readonly List<EllipseBase> All = new();

    private Vertex _f1;
    public Vertex Focal1
    {
        get => _f1;
        set
        {
            _f1.OnMoved.Remove(__focal_redraw);
            _f1.OnDragged.Remove(__reposition);
            _f1 = value;
            _f1.OnDragged.Add(__reposition);
            _f1.OnMoved.Add(__focal_redraw);
        }
    }

    private Vertex _f2;
    public Vertex Focal2
    {
        get => _f2;
        set
        {
            _f2.OnMoved.Remove(__focal_redraw);
            _f2.OnDragged.Remove(__reposition);
            _f2 = value;
            _f2.OnDragged.Add(__reposition);
            _f2.OnMoved.Add(__focal_redraw);
        }
    }

    public override bool Draggable
    {
        set
        {
            base.Draggable = value;
            if (Ring != null) Ring.Draggable = value;
        }
    }

    public double DistanceSum { get; set; }

    internal Ring Ring;
    public EllipseBase(Vertex f1, Vertex f2, double dSum)
    {
        _f1 = f1;
        _f2 = f2;
        DistanceSum = dSum;
        Ring = new Ring(this)
        {
            Draggable = Draggable
        };
        All.Add(this);

        Focal1.OnMoved.Add(__focal_redraw);
        Focal2.OnMoved.Add(__focal_redraw);
        Focal1.OnDragged.Add(__reposition);
        Focal2.OnDragged.Add(__reposition);

        Focal1.OnRemoved.Add(__remove);
        Focal1.OnRemoved.Add(__remove);

        MainWindow.BigScreen.Children.Insert(0, this);
        MainWindow.BigScreen.Children.Insert(0, Ring);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
    }

    public void Reposition()
    {
    }

    private void __focal_redraw(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v;
        Ring.InvalidateVisual();
    }

    private void __reposition(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v;
        Reposition();
    }

    private void __remove(double z, double x)
    {
        _ = z; _ = x;
        MainWindow.BigScreen.Children.Remove(this);
        MainWindow.BigScreen.Children.Remove(Ring);
    }
}
    internal class Ring : DraggableGraphic
    {
        public static readonly List<Ring> All = new();
        public EllipseBase Ellipse;

        public Ring(EllipseBase el)
        {
            All.Add(this);
            Ellipse = el;

            OnMoved.Add((double _, double _, double _, double _) =>
            {
                double mx = BigScreen.MouseX, my = BigScreen.MouseY;
                var parentOffset = this.GetPosition();
                mx -= parentOffset.X;
                my -= parentOffset.Y;
                this.SetPosition(0, 0);
                Ellipse.DistanceSum = new Point(Ellipse.Focal1.X, Ellipse.Focal1.Y).DistanceTo(new Point(mx, my)) + new Point(Ellipse.Focal2.X, Ellipse.Focal2.Y).DistanceTo(new Point(mx, my));
                InvalidateVisual();

                foreach (var l in Ellipse.OnMoved) l(X, Y, X, Y);
            });
            OnDragged.Add(MainWindow.RegenAll);

        }

        public override void Render(DrawingContext context)
        {
            // Graphic is cleared
            var info = ConvertFociToEllipse(Ellipse.Focal1.X, Ellipse.Focal1.Y, Ellipse.Focal2.X, Ellipse.Focal2.Y);

            var pen = new Pen(UIColors.ConnectionColor, 4);
            context.DrawEllipse(null, pen, new Point(info.X + info.Width / 2, info.Y + info.Height / 2), info.Width / 2, info.Height / 2);
        }

        EllipseData ConvertFociToEllipse(double focus1X, double focus1Y, double focus2X, double focus2Y)
        {
            var distance = Math.Sqrt(Math.Pow(focus2X - focus1X, 2) + Math.Pow(focus2Y - focus1Y, 2));
            var semiMajorAxis = Ellipse.DistanceSum / 2;
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

