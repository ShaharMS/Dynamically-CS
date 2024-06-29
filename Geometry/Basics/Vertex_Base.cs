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
using Dynamically.Geometry;
using System.Runtime.CompilerServices;
using Avalonia.Styling;
using Dynamically.Containers;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using System.Collections.ObjectModel;
using Dynamically.Backend.Roles;

namespace Dynamically.Geometry.Basics;

public partial class Vertex : DraggableGraphic
{

    public static readonly List<Vertex> All = new();


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
    /// When this vertex is manipulated, the formulas contained here are supposed to change.
    /// </summary>
    public List<Formula> EffectedFormulas { get; set; } = new();


    public Label IdDisplay;

    public bool GotRemoved
    {
        get => !ParentBoard.Children.Contains(this);
    }

    public new double Opacity {
        get => base.Opacity;
        set {
            base.Opacity = value;
            IdDisplay.Opacity = value;
        }
    }
    public bool Hidden {
        get => !IsVisible;
        set {
            IdDisplay.IsVisible = IsVisible = !value;
        }
    }

    public Vertex(Board board, Point p) : this(board, p.X, p.Y) { }
    public Vertex(Board board, double x, double y, char id = '_') : base(board)
    {


        IdDisplay = new Label()
        {
            FontSize = 20,
            FontWeight = FontWeight.DemiBold,
            Background = null,
            BorderThickness = new Thickness(0, 0, 0, 0),
            Width = 25
        };
        
        if (id == '_') IDGenerator.GenerateFor(this);
        else Id = id;

        Roles = new RoleMap(this);
        Formula = new PointFormula(this);


        ParentBoard.Children.Add(IdDisplay);

        X = x;
        Y = y;

        ContextMenu = new ContextMenu();
        Provider = new VertexContextMenuProvider(this, ContextMenu);
        ContextMenu.Items = Provider.Items;

        OnDragStart.Add(() => { if (!IsMovable()) CurrentlyDragging = false; });
        OnMoved.Add((double _, double _, double _, double _) => Reposition());
        OnDragged.Add(MainWindow.RegenAll);

        InvalidateVisual();
        RepositionText();

        All.Add(this);

        ParentBoard.Children.Add(this);

        Width = Height = UIDesign.VertexGraphicCircleRadius * 2;
    }

    public void RepositionText()
    {
        var d = IdDisplay.FontSize + 4;
        var degs = new double[Relations.Count + 1];
        var i = 0;
        if (Relations.Count >= 1)
        {
            foreach (var other in Relations)
            {
                degs[i] = (X, Y).DegreesTo(other);
                i++;
            }
        }

        degs[Relations.Count] = 360 + degs[0];
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
    }
    public override void Render(DrawingContext context)
    {
        // Graphic is cleared
        var brush = UIColors.VertexFillColor;
        var pen = new Pen(UIColors.VertexOutlineColor, UIDesign.VertexGraphicCircleRadius / 2.5);
        context.DrawEllipse(brush, pen, new Point(0, 0), UIDesign.VertexGraphicCircleRadius, UIDesign.VertexGraphicCircleRadius);
        context.FillRectangle(new SolidColorBrush(Colors.Red), new Rect(-1, -1, 2, 2));
    }

    public void RemoveFromBoard()
    {
        double sx = X, sy = Y;
        DisconnectAll();

        IDGenerator.Remove(this);

        ParentBoard.RemoveChild(this);
        ParentBoard.RemoveChild(IdDisplay);

        OnMoved.Clear();
        OnDragged.Clear();

        foreach (var r in OnRemoved.ToList())
        {
            r(sx, sy);
        }

        OnRemoved.Clear();
        Roles.Clear();

        All.Remove(this);

        MainWindow.RegenAll(0, 0, 0, 0);
    }

    public static implicit operator Point(Vertex vertex) { return new Point(vertex.X, vertex.Y); }
    public static implicit operator Vertex(Point point) { return new Vertex(MainWindow.Instance.WindowTabs.CurrentBoard, point.X, point.Y); }


    public static Vertex? GetVertexById(char id)
    {
        foreach (var vertex in All)
        {
            if (vertex.Id == id) return vertex;
        }
        return null;
    }
}
