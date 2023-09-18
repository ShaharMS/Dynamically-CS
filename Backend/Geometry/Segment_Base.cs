using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Rendering.SceneGraph;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Segment : DraggableGraphic
{
    public static readonly List<Segment> all = new();

    public bool Anchored
    {
        get => joint1.Anchored && joint2.Anchored;
        set => joint1.Anchored = joint2.Anchored = value;
    }

    bool _aux;

    /// <summary>
    /// Determines whether or not this construction doesnt come with the question.
    /// </summary>
    public bool IsAuxiliary
    {
        get => _aux;
        set
        {
            _aux = value;
            InvalidateVisual();
        }
    }

    public Joint joint1 { get; private set; }
    public Joint joint2 { get; private set;}

    public List<Action<Joint, Joint>> OnRemoved = new();
    public RoleMap Roles { get; set; }

    double org1X;
    double org1Y;
    double org2X;
    double org2Y;


    public Label Label = new();

    SegmentTextDisplay _disp = SegmentTextDisplay.NONE;
    public SegmentTextDisplay TextDisplayMode
    {
        get => _disp;
        set
        {
            _disp = value;
            switch (value)
            {
                case SegmentTextDisplay.LENGTH_EXACT:
                    if (joint1.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (joint2.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => Label.Content = "" + Math.Round(Length, 3);
                    if (!joint1.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint1.OnMoved.Add((_, _, _, _) => labelUpdater());
                    if (!joint2.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint2.OnMoved.Add((_, _, _, _) => labelUpdater());
                    labelUpdater();
                    break;
                case SegmentTextDisplay.LENGTH_ROUND:
                    if (joint1.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (joint2.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => Label.Content = "" + Math.Round(Length);
                    if (!joint1.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint1.OnMoved.Add((_, _, _, _) => labelUpdater());
                    if (!joint2.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint2.OnMoved.Add((_, _, _, _) => labelUpdater());
                    labelUpdater();
                    break;
                case SegmentTextDisplay.PARAM:
                case SegmentTextDisplay.CUSTOM:
                case SegmentTextDisplay.LENGTH_GIVEN:
                case SegmentTextDisplay.NONE:
                    if (joint1.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (joint2.OnMoved.Contains((_, _, _, _) => labelUpdater())) joint2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => { };
                    break;
            }
            if (_disp != SegmentTextDisplay.NONE) InvalidateVisual();
        }
    }

    Action labelUpdater = () => { };
    public Segment(Joint f, Joint t)
    {
        joint1 = f;
        joint2 = t;
        joint1.Roles.AddToRole(Role.SEGMENT_Corner, this);
        joint2.Roles.AddToRole(Role.SEGMENT_Corner, this);
        org1X = f.X;
        org1Y = f.Y;
        org2X = t.X;
        org2Y = t.Y;
        Formula = new SegmentFormula(this);
        MiddleFormula = new RatioOnSegmentFormula(Formula, 0.5);

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

        Roles = new RoleMap(this);

        ContextMenu = new ContextMenu();
        Provider = new SegmentContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;
        PointerReleased += (sender, args) => {
            if (args.InitialPressMouseButton == Avalonia.Input.MouseButton.Right) Provider.Regenerate();
        };


        MainWindow.BigScreen.Children.Add(Label);

        OnMoved.Add((_, _, _, _) =>
        {
            joint1.CurrentlyDragging = joint2.CurrentlyDragging = true;
            if (joint1.Anchored || joint2.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            var pj1X = joint1.X; var pj2X = joint2.X;
            var pj1Y = joint1.Y; var pj2Y = joint2.Y;
            joint1.X = org1X + X;
            joint2.X = org2X + X;
            joint1.Y = org1Y + Y;
            joint2.Y = org2Y + Y;
            X = 0; Y = 0;
            joint1.DispatchOnMovedEvents(joint1.X, joint1.Y, pj1X, pj1Y);
            joint2.DispatchOnMovedEvents(joint2.X, joint2.Y, pj2X, pj2Y);
            Canvas.SetLeft(Label, MiddleFormula.pointOnRatio.X - Label.GuessTextWidth() / 2);
            Canvas.SetTop(Label, MiddleFormula.pointOnRatio.Y - Label.Height / 2);
            InvalidateVisual();
        });

        OnDragged.Add((double cx, double cy, double prx, double pry) =>
        {
            joint1.CurrentlyDragging = joint2.CurrentlyDragging = false;
            joint1.DispatchOnDraggedEvents();
            joint2.DispatchOnDraggedEvents();
            joint1.InvalidateVisual();
            joint2.InvalidateVisual();
            reposition();
        });
        OnDragged.Add(MainWindow.regenAll);


        all.Add(this);

        MainWindow.BigScreen.Children.Insert(0, this);

        InvalidateVisual();
    }

    public Segment ReplaceJoint(Joint joint, Joint by)
    {
        if (joint1 == joint)
        {
            joint1.Connections.Remove(this);
            joint1.Relations.Remove(joint2);
            joint1 = by;
            joint1.Connections.Add(this);
            joint1.Relations.Add(joint2);

            joint1.CreateBoardRelationsWith(joint2, this);
        }
        else if (joint2 == joint)
        {
            joint2.Connections.Remove(this);
            joint2.Relations.Remove(joint1);
            joint2 = by;
            joint2.Connections.Add(this);
            joint2.Relations.Add(joint1);

            joint1.CreateBoardRelationsWith(joint2, this);
        }
        InvalidateVisual();
        return this;
    }

    public override void Render(DrawingContext context)
    {
        // Label
        Label.RenderTransform = new RotateTransform(Math.Atan(Formula.slope) * 180 / Math.PI);
        

        // Graphic is cleared
        var pen = new Pen
        {
            Brush = UIColors.ConnectionColor,
            Thickness = UIDesign.ConnectionGraphicWidth,
        };
        if (IsAuxiliary) pen.DashStyle = DashStyle.Dash;
        context.DrawLine(pen, new Point(joint1.X, joint1.Y), new Point(joint2.X, joint2.Y));
        // padding for easier dragging
        var pen2 = new Pen(new SolidColorBrush(Colors.Black, 0.01), UIDesign.ConnectionGraphicWidth * 1.5);
        context.DrawLine(pen2, new Point(joint1.X, joint1.Y), new Point(joint2.X, joint2.Y));

    }

}

public enum SegmentTextDisplay
{
    LENGTH_EXACT,
    LENGTH_ROUND,
    PARAM,
    NONE,
    CUSTOM,
    LENGTH_GIVEN
}
