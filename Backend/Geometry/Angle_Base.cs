using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Containers;
using Dynamically.Design;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Angle : DraggableGraphic
{

    public static readonly List<Angle> All = new();

    public double DefaultDistance
    {
        get => Math.Min(50, Math.Min(Center.DistanceTo(Vertex1), Center.DistanceTo(Vertex2)) * 0.8);
    }

    public new double Opacity {
        get => base.Opacity;
        set {
            base.Opacity = value;
            Label.Opacity = value;
        }
    }
    

    Vertex _c;
    public Vertex Center
    {
        get => _c;
        set
        {
            _c.OnMoved.Remove(__updateAngle);
            _c = value;
            _c.OnMoved.Add(__updateAngle);
        }
    }
    Vertex _v1;
    public Vertex Vertex1
    {
        get => _v1;
        set
        {
            _v1.OnMoved.Remove(__updateAngle);
            _v1 = value;
            _v1.OnMoved.Add(__updateAngle);
        }
    }
    Vertex _v2;
    public Vertex Vertex2
    {
        get => _v2;
        set
        {
            _v2.OnMoved.Remove(__updateAngle);
            _v2 = value;
            _v2.OnMoved.Add(__updateAngle);
        }
    }
    public double Degrees { get; private set; }
    public double Radians { get; private set; }

    public RayFormula BisectorRay {get; private set;}
    bool _large;
    public bool Large
    {
        get => _large;
        set
        {
            _large = value;
            InvalidateVisual();

        }
    }

    public Label Label = new();

    AngleTextDisplay _disp = AngleTextDisplay.NONE;
    public AngleTextDisplay TextDisplayMode
    {
        get => _disp;
        set
        {
            _disp = value;
            InvalidateVisual();
        }
    }

    public Angle(Vertex v1, Vertex c, Vertex v2, bool large = false) : base(c.ParentBoard)
    {
        _c = c;
        _v1 = v1;
        _v2 = v2;
        Center = c;
        Vertex1 = v1;
        Vertex2 = v2;
        _large = large;

        Draggable = false;

        Label = new Label
        {
            FontSize = 16,
            FontWeight = FontWeight.SemiLight,
            Background = new SolidColorBrush(Colors.Black),
            BorderThickness = new Thickness(0, 0, 0, 0),
            Width = double.NaN,
            Height = 24,
        };
        Label.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == nameof(Label.Content))
            {
                Label.IsVisible = Label.Content?.ToString()?.Length != 0;
            }
        };
        Label.Content = "";

        Children.Add(Label);

        foreach (Vertex v in new[] { v1, v2, c }) v.OnRemoved.Add((_, _) => { RemoveFromBoard(); });



        MainWindow.Instance.MainBoard.Children.Insert(0, this);

        BisectorRay = new RayFormula(Center, Math.Tan(GetBisectorRadians()));

        ContextMenu = new ContextMenu();
        Provider = new AngleContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        this.SetPosition(0, 0);

        All.Add(this);
    }

    public override void Render(DrawingContext context)
    {
        Degrees = Tools.GetDegreesBetween3Points(Vertex1, Center, Vertex2);
        if ((Degrees > 180 && !Large) || (Degrees < 180 && Large)) Degrees = 360 - Degrees;
        Radians = Degrees.ToRadians();

        var order = Tools.OrderRadiansBySmallArc(Center.RadiansTo(Vertex1), Center.RadiansTo(Vertex2));
        if (Large) order = order.Reverse().ToArray();
        var start = order[0];
        var end = order[1];
        Point? previous = null;
        if (!Degrees.RoughlyEquals(90))
        {
            // Draw a triangle between the two angle edges, and the center, for easier clicking.
            var geom = new PathGeometry();
            var figure = new PathFigure()
            {
                StartPoint = Center,
                IsFilled = true,
                IsClosed = true
            };
            figure.Segments?.Add(new LineSegment { Point = new Point(Center.X + DefaultDistance * Math.Cos(end), Center.Y + DefaultDistance * Math.Sin(end)) });
            figure.Segments?.Add(new LineSegment { Point = new Point(Center.X + DefaultDistance * Math.Cos(start), Center.Y + DefaultDistance * Math.Sin(start)) });
            geom.Figures.Add(figure);
            context.DrawGeometry(new SolidColorBrush(Colors.Black, 0.01), null, geom);

            for (double i = start; i <= start + Radians; i += Math.PI / 36)
            {
                if (previous == null)
                {
                    previous = new Point(Center.X + DefaultDistance * Math.Cos(i), Center.Y + DefaultDistance * Math.Sin(i));
                }
                else
                {
                    var point = new Point(Center.X + DefaultDistance * Math.Cos(i), Center.Y + DefaultDistance * Math.Sin(i));
                    context.DrawLine(new Pen(UIColors.ConnectionColor, 2), previous.Value, point);
                    // padding for easier clicking
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 2), previous.Value, point);
                    previous = point;
                }
            }
            if (previous != null)
            {
                var p = new Point(Center.X + DefaultDistance * Math.Cos(end), Center.Y + DefaultDistance * Math.Sin(end));
                context.DrawLine(new Pen(UIColors.ConnectionColor, 2), previous.Value, p);
                context.DrawLine(new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 1.5), previous.Value, p);
            }

        }
        else
        {
            var p1 = new Point(Center.X + DefaultDistance / 2 * Math.Cos(start), Center.Y + DefaultDistance / 2 * Math.Sin(start));
            var p2 = new Point(Center.X + Math.Sqrt(2) * DefaultDistance / 2 * Math.Cos(start + Math.PI / 4), Center.Y + Math.Sqrt(2) * DefaultDistance / 2 * Math.Sin(start + Math.PI / 4));
            var p3 = new Point(Center.X + DefaultDistance / 2 * Math.Cos(end), Center.Y + DefaultDistance / 2 * Math.Sin(end));
            context.DrawLine(new Pen(UIColors.ConnectionColor, 2), p1, p2);
            context.DrawLine(new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 1.5), p1, p2);
            context.DrawLine(new Pen(UIColors.ConnectionColor, 2), p3, p2);
            context.DrawLine(new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 1.5), p3, p2);
            context.DrawRectangle(new SolidColorBrush(Colors.Black, 0.01), null, new Rect(Center, p2));
        }





        switch (TextDisplayMode)
        {
            case AngleTextDisplay.DEGREES_EXACT:
                Label.Content = Math.Round(Degrees, 3);
                break;
            case AngleTextDisplay.DEGREES_ROUND:
                Label.Content = Math.Round(Degrees);
                break;
            case AngleTextDisplay.RADIANS_EXACT:
                Label.Content = Math.Round(Radians, 3);
                break;
            case AngleTextDisplay.RADIANS_ROUND:
                Label.Content = Math.Round(Radians);
                break;
            case AngleTextDisplay.DEGREES_GIVEN:
            case AngleTextDisplay.RADIANS_GIVEN:
            case AngleTextDisplay.PARAM:
            case AngleTextDisplay.NONE:
                break;
        }
        Canvas.SetLeft(Label, Center.X + (DefaultDistance * 0.95 * Math.Cos(start + Radians / 2) - Label.GuessTextWidth() / 2) * (Large ? -1 : 1));
        Canvas.SetTop(Label, Center.Y + (DefaultDistance * 0.95 * Math.Sin(start + Radians / 2) - Label.Height / 2) * (Large ? -1 : 1));
    }

    public Segment GenerateBisector() {
        double middleAngle = GetBisectorRadians();
        var len = (Center.DistanceTo(Vertex1) + Center.DistanceTo(Vertex2)) / 2;
        var x = Center.X + len * Math.Cos(middleAngle);
        var y = Center.Y + len * Math.Sin(middleAngle);
        
        var nj = new Vertex(ParentBoard, x, y);
        nj.Roles.AddToRole(Role.RAY_On, BisectorRay);
        var segment = Center.Connect(nj);
        segment.Roles.AddToRole(Role.ANGLE_Bisector, this);

        return segment;
    }

    public double GetBisectorRadians() =>((Center.RadiansTo(Vertex1) + Center.RadiansTo(Vertex2) + Math.PI) % (2 * Math.PI) - Math.PI) / 2;

    public static bool Exists(Vertex center, Vertex j1, Vertex j2)
    {
        if (center == j1 || j1 == j2 || center == j2) return false;
        foreach (Angle a in All)
        {
            if (a.Center == center && ((a.Vertex1 == j1 && a.Vertex2 == j2) || (a.Vertex1 == j2 && a.Vertex2 == j1))) return true;
        }
        return false;
    }

    public static bool Exists(char cid, char id1, char id2)
    {
        var c = Vertex.GetJointById(cid);
        if (c == null) return false;
        var j1 = Vertex.GetJointById(id1);
        if (j1 == null) return false;
        var j2 = Vertex.GetJointById(id2);
        if (j2 == null) return false;
        return Exists(c, j1, j2);
    }

    public static bool Exists(string angle)
    {
        if (angle.StartsWith("∠")) angle = angle.Remove(0, 1);
        if (angle.Length != 3) return false;
        var arr = angle.ToCharArray();
        return Exists(arr[1], arr[0], arr[2]);
    }

    public void RemoveFromBoard()
    {
        All.Remove(this);
        MainWindow.Instance.MainBoard.Children.Remove(this);
    }
}


public enum AngleTextDisplay
{
    DEGREES_EXACT,
    DEGREES_ROUND,
    DEGREES_GIVEN,
    RADIANS_EXACT,
    RADIANS_ROUND,
    RADIANS_GIVEN,
    PARAM,
    NONE,

}
