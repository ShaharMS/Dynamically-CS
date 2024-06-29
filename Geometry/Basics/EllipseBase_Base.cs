using Avalonia.Media;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Containers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;

namespace Dynamically.Geometry.Basics;

public partial class EllipseBase : DraggableGraphic, IDrawable
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

    public double CenterX
    {
        get => (Focal1.X + Focal2.X) / 2;
        set
        {
            if (Anchored) return;
            else if (Focal1.Anchored || Focal2.Anchored)
            {
                var anc = Focal1.Anchored ? Focal1 : Focal2;
                var other = Focal1.Anchored ? Focal2 : Focal1;

                var dist = anc.DistanceTo(value, CenterY);
                var angle = anc.RadiansTo(value, CenterY);

                other.X = anc.X + 2 * dist * Math.Cos(angle);
                other.Y = anc.Y + 2 * dist * Math.Sin(angle);
                other.DispatchOnMovedEvents();
            }
            else
            {
                Focal1.X = value - (Focal1.X - Focal2.X) / 2;
                Focal2.X = value + (Focal1.X - Focal2.X) / 2;
                Focal1.DispatchOnMovedEvents();
                Focal2.DispatchOnMovedEvents();
            }
        }
    }
    public double CenterY
    {
        get => (Focal1.Y + Focal2.Y) / 2;
        set
        {
            if (Anchored) return;
            else if (Focal1.Anchored || Focal2.Anchored)
            {
                var anc = Focal1.Anchored ? Focal1 : Focal2;
                var other = Focal1.Anchored ? Focal2 : Focal1;

                var dist = anc.DistanceTo(CenterX, value);
                var angle = anc.RadiansTo(CenterX, value);

                other.X = anc.X + 2 * dist * Math.Cos(angle);
                other.Y = anc.Y + 2 * dist * Math.Sin(angle);

                other.DispatchOnMovedEvents();
            }
            else
            {
                Focal1.Y = value - (Focal1.Y - Focal2.Y) / 2;
                Focal2.Y = value + (Focal1.Y - Focal2.Y) / 2;
                Focal1.DispatchOnMovedEvents();
                Focal2.DispatchOnMovedEvents();
            }
        }
    }

    public Point Center => new(CenterX, CenterY);
    /// <summary>
    /// Ellipse's semi-major axis
    /// </summary>
    public double A => DistanceSum / 2;

    public double B => Math.Sqrt(DistanceSum.Pow(2) / 4 - C.Pow(2));

    public double C => Focal1.DistanceTo(Focal2) / 2;

    public override bool Draggable
    {
        set
        {
            base.Draggable = value;
            if (Ring != null) Ring.Draggable = value;
        }
    }

    public bool Anchored => Focal1.Anchored && Focal2.Anchored;

    public double DistanceSum { get; set; }

    internal Ring Ring;
    public EllipseBase(Vertex f1, Vertex f2, double dSum) : base(f1.ParentBoard)
    {
        _f1 = f1;
        _f2 = f2;
        DistanceSum = dSum;
        Ring = new Ring(this)
        {
            Draggable = Draggable
        };

        Focal1.OnMoved.Add(__focal_redraw);
        Focal2.OnMoved.Add(__focal_redraw);
        Focal1.OnDragged.Add(__reposition);
        Focal2.OnDragged.Add(__reposition);

        Focal1.OnRemoved.Add(__remove);
        Focal1.OnRemoved.Add(__remove);

        ParentBoard.Children.Insert(0, this);
        ParentBoard.Children.Insert(0, Ring);

        All.Add(this);

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
        ParentBoard.Children.Remove(this);
        ParentBoard.Children.Remove(Ring);
    }
}
internal class Ring : DraggableGraphic
{
    public static readonly List<Ring> All = new();
    public EllipseBase Ellipse;

    public Ring(EllipseBase el) : base(el.ParentBoard)
    {

        Ellipse = el;

        OnMoved.Add((_, _, _, _) =>
            {
                double mx = ParentBoard.MouseX, my = ParentBoard.MouseY;
                mx -= ((Board)Parent!).X;
                my -= ((Board)Parent!).Y;
                this.SetPosition(0, 0);
                Ellipse.DistanceSum = new Point(Ellipse.Focal1.X, Ellipse.Focal1.Y).DistanceTo(new Point(mx, my)) + new Point(Ellipse.Focal2.X, Ellipse.Focal2.Y).DistanceTo(new Point(mx, my));
                InvalidateVisual();

                foreach (var l in Ellipse.OnMoved) l(X, Y, X, Y);
            });
        OnDragged.Add(MainWindow.RegenAll);

        All.Add(this);

    }

    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var info = ConvertFociToEllipse(Ellipse.Focal1.X, Ellipse.Focal1.Y, Ellipse.Focal2.X, Ellipse.Focal2.Y);

        var pen = new Pen(UIColors.SegmentColor, 4);
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

