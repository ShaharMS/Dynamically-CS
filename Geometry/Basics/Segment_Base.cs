﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Rendering.SceneGraph;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Backend.Roles;
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

namespace Dynamically.Geometry.Basics;

public partial class Segment : DraggableGraphic
{
    public static readonly List<Segment> All = new();

    public bool Anchored
    {
        get => Vertex1.Anchored && Vertex2.Anchored;
        set => Vertex1.Anchored = Vertex2.Anchored = value;
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

    public new double Opacity {
        get => base.Opacity;
        set {
            base.Opacity = value;
            Label.Opacity = value;
        }
    }

    public Vertex Vertex1 { get; private set; }
    public Vertex Vertex2 { get; private set;}

    public List<Action<Vertex, Vertex>> OnRemoved = new();
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
                    if (Vertex1.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (Vertex2.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => Label.Content = "" + Math.Round(Length, 3);
                    if (!Vertex1.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex1.OnMoved.Add((_, _, _, _) => labelUpdater());
                    if (!Vertex2.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex2.OnMoved.Add((_, _, _, _) => labelUpdater());
                    labelUpdater();
                    break;
                case SegmentTextDisplay.LENGTH_ROUND:
                    if (Vertex1.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (Vertex2.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => Label.Content = "" + Math.Round(Length);
                    if (!Vertex1.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex1.OnMoved.Add((_, _, _, _) => labelUpdater());
                    if (!Vertex2.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex2.OnMoved.Add((_, _, _, _) => labelUpdater());
                    labelUpdater();
                    break;
                case SegmentTextDisplay.PARAM:
                case SegmentTextDisplay.CUSTOM:
                case SegmentTextDisplay.LENGTH_GIVEN:
                case SegmentTextDisplay.NONE:
                    if (Vertex1.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex1.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    if (Vertex2.OnMoved.Contains((_, _, _, _) => labelUpdater())) Vertex2.OnMoved.Remove((_, _, _, _) => labelUpdater());
                    labelUpdater = () => { };
                    break;
            }
            if (_disp != SegmentTextDisplay.NONE) InvalidateVisual();
        }
    }

    Action labelUpdater = () => { };
    public Segment(Vertex f, Vertex t) : base(f.ParentBoard)
    {
        Vertex1 = f;
        Vertex2 = t;
        org1X = f.X;
        org1Y = f.Y;
        org2X = t.X;
        org2Y = t.Y;
        Formula = new SegmentFormula(this);
        MiddleFormula = new RatioOnSegmentFormula(Formula, 0.5);
        RayFormula = new RayFormula(this);

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


        ParentBoard.AddChild(Label);

        OnDragStart.Add(() => { if (!IsMovable()) CurrentlyDragging = false; });
        OnMoved.Add((_, _, _, _) =>
        {
            Vertex1.CurrentlyDragging = Vertex2.CurrentlyDragging = true;
            if (Vertex1.Anchored || Vertex2.Anchored)
            {
                this.SetPosition(0, 0);
                return;
            }
            var pj1X = Vertex1.X; var pj2X = Vertex2.X;
            var pj1Y = Vertex1.Y; var pj2Y = Vertex2.Y;

                Vertex1.X = org1X + X;
                Vertex1.Y = org1Y + Y;
                Vertex1.DispatchOnMovedEvents(pj1X, pj1Y);
                Vertex2.X = org2X + X;
                Vertex2.Y = org2Y + Y;
                Vertex2.DispatchOnMovedEvents(pj2X, pj2Y);

            X = 0; Y = 0;

            InvalidateVisual();
        });

        OnDragged.Add((double cx, double cy, double prx, double pry) =>
        {
            Vertex1.CurrentlyDragging = Vertex2.CurrentlyDragging = false;
            Vertex1.DispatchOnDraggedEvents();
            Vertex2.DispatchOnDraggedEvents();
            Vertex1.InvalidateVisual();
            Vertex2.InvalidateVisual();
            Reposition();
        });
        OnDragged.Add(AppWindow.RegenAll);

        All.Add(this);

        ParentBoard.AddChildAt(this, 0);

        Vertex1.Roles.AddToRole(Role.SEGMENT_Corner, this);
        Vertex2.Roles.AddToRole(Role.SEGMENT_Corner, this);

        InvalidateVisual();
    }

    public Segment ReplaceVertex(Vertex vertex, Vertex by)
    {
        if (Vertex1 == vertex)
        {
            Vertex1.Relations.Remove(Vertex2);
            foreach (var f in new Formula[] { Formula, MiddleFormula, RayFormula }) Vertex1.EffectedFormulas.Remove(f);
            Vertex1 = by;
            Vertex1.Relations.Add(Vertex2);
            foreach (var f in new Formula[] { Formula, MiddleFormula, RayFormula }) Vertex1.EffectedFormulas.Add(f);

            Vertex1.CreateBoardRelationsWith(Vertex2, this);
        }
        else if (Vertex2 == vertex)
        {
            Vertex2.Relations.Remove(Vertex1);
            foreach (var f in new Formula[] { Formula, MiddleFormula, RayFormula }) Vertex2.EffectedFormulas.Remove(f);
            Vertex2 = by;
            Vertex2.Relations.Add(Vertex1);
            foreach (var f in new Formula[] { Formula, MiddleFormula, RayFormula }) Vertex2.EffectedFormulas.Add(f);

            Vertex1.CreateBoardRelationsWith(Vertex2, this);
        }
        InvalidateVisual();
        return this;
    }

    public override void Render(DrawingContext context)
    {
        // Label
        Label.RenderTransform = new RotateTransform(Math.Atan(Formula.Slope) * 180 / Math.PI);
        Canvas.SetLeft(Label, MiddleFormula.PointOnRatio.X - Label.GuessTextWidth() / 2);
        Canvas.SetTop(Label, MiddleFormula.PointOnRatio.Y - Label.Height / 2);

        // Graphic is cleared
        context.DrawLine(UIColors.SegmentPen, new Point(Vertex1.X, Vertex1.Y), new Point(Vertex2.X, Vertex2.Y));
        // padding for easier dragging
        var pen2 = new Pen(new SolidColorBrush(Colors.Black, 0.01), Settings.SegmentGraphicWidth * 1.5);
        context.DrawLine(pen2, new Point(Vertex1.X, Vertex1.Y), new Point(Vertex2.X, Vertex2.Y));

    }

    public bool IsMadeOf(Vertex v1, Vertex v2) => (v1 == Vertex1 && v2 == Vertex2) || (v1 == Vertex2 && v2 == Vertex1);
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
