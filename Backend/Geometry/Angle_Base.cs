using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
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

    public static readonly List<Angle> all = new();

    public double DefaultDistance
    {
        get => Math.Min(50, Math.Min(Center.DistanceTo(joint1), Center.DistanceTo(joint2)) * 0.8);
    }

    public new double Opacity {
        get => base.Opacity;
        set {
            base.Opacity = value;
            Label.Opacity = value;
        }
    }
    

    Joint _c;
    public Joint Center
    {
        get => _c;
        set
        {
            _c.OnMoved.Remove(__updateAngle);
            _c = value;
            _c.OnMoved.Add(__updateAngle);
        }
    }
    Joint _j1;
    public Joint joint1
    {
        get => _j1;
        set
        {
            _j1.OnMoved.Remove(__updateAngle);
            _j1 = value;
            _j1.OnMoved.Add(__updateAngle);
        }
    }
    Joint _j2;
    public Joint joint2
    {
        get => _j2;
        set
        {
            _j2.OnMoved.Remove(__updateAngle);
            _j2 = value;
            _j2.OnMoved.Add(__updateAngle);
        }
    }
    public double Degrees { get; private set; }
    public double Radians { get; private set; }

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

    Action labelUpdater = () => { };

    public Angle(Joint v1, Joint c, Joint v2, bool large = false)
    {
        _c = c;
        _j1 = v1;
        _j2 = v2;
        Center = c;
        joint1 = v1;
        joint2 = v2;
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

        foreach (Joint v in new[] { v1, v2, c }) v.OnRemoved.Add((_, _) => { RemoveFromBoard(); });



        MainWindow.BigScreen.Children.Insert(0, this);

        ContextMenu = new ContextMenu();
        Provider = new AngleContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        this.SetPosition(0, 0);

        all.Add(this);
    }

    public override void Render(DrawingContext context)
    {
        Degrees = Tools.GetDegreesBetween3Points(joint1, Center, joint2);
        if ((Degrees > 180 && !Large) || (Degrees < 180 && Large)) Degrees = 360 - Degrees;
        Radians = Degrees.ToRadians();

        var order = Tools.OrderRadiansBySmallArc(Center.RadiansTo(joint1), Center.RadiansTo(joint2));
        if (Large) order.Reverse();
        var start = order[0];
        var end = order[1];
        Point? previous = null;
        if (Degrees != 90)
        {
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
                    context.DrawLine(new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 1.5), previous.Value, point);
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
        Canvas.SetLeft(Label, Center.X + DefaultDistance * 0.95 * Math.Cos(start + Radians / 2) - Label.GuessTextWidth() / 2);
        Canvas.SetTop(Label, Center.Y + DefaultDistance * 0.95 * Math.Sin(start + Radians / 2) - Label.Height / 2);
    }

    public static bool Exists(Joint center, Joint j1, Joint j2)
    {
        if (center == j1 || j1 == j2 || center == j2) return false;
        foreach (Angle a in all)
        {
            if (a.Center == center && ((a.joint1 == j1 && a.joint2 == j2) || (a.joint1 == j2 && a.joint2 == j1))) return true;
        }
        return false;
    }

    public static bool Exists(char cid, char id1, char id2)
    {
        var c = Joint.GetJointById(cid);
        if (c == null) return false;
        var j1 = Joint.GetJointById(id1);
        if (j1 == null) return false;
        var j2 = Joint.GetJointById(id2);
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
        all.Remove(this);
        MainWindow.BigScreen.Children.Remove(this);
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
