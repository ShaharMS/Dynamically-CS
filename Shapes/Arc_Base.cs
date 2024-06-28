using Avalonia;
using Avalonia.Media;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Arc : DraggableGraphic
{
    double _startDegrees;
    public double StartDegrees
    {
        get => _startDegrees;
        set
        {
            _startDegrees = value;
            InvalidateVisual();
        }
    }

    double _endDegrees;
    public double EndDegrees
    {
        get => _endDegrees;
        set
        {
            _endDegrees = value;
            InvalidateVisual();
        }
    }
    public double TotalDegrees => Math.Abs(EndDegrees - StartDegrees);

    bool _showEdges = true;
    public bool ShowEdges
    {
        get => _showEdges;
        set
        {
            _showEdges = value;
            InvalidateVisual();
        }
    }

    public Vertex StartEdge { get; set; }
    public Vertex EndEdge { get; set; }

    public Vertex Center { get; set; }

    public double Radius { get; set; }

    public Arc(Vertex center, double radius) : this(center, radius, 0, 180) { }

    public Arc(Vertex center, double radius, double startAngle, double endAngle) : base(center.ParentBoard)
    {
        _startDegrees = startAngle;
        _endDegrees = endAngle;

        Center = center;
        Radius = radius;
        ParentBoard.Children.Insert(0, this);

        Formula = new CircleFormula(Radius, center.X, center.Y);

        StartEdge = new Vertex(ParentBoard, center.X + Radius * Math.Cos(StartDegrees * Math.PI / 180), center.Y + Radius * Math.Sin(StartDegrees * Math.PI / 180));
        EndEdge = new Vertex(ParentBoard, center.X + Radius * Math.Cos(EndDegrees * Math.PI / 180), center.Y + Radius * Math.Sin(EndDegrees * Math.PI / 180));

        Log.WriteVar(StartEdge, EndEdge);

        Formula.AddFollower(StartEdge);
        Formula.AddFollower(EndEdge);

        StartEdge.OnMoved.Add((_, _, _, _) => StartDegrees = center.DegreesTo(StartEdge));
        EndEdge.OnMoved.Add((_, _, _, _) => EndDegrees = center.DegreesTo(EndEdge));

        OnDragStart.Add(() => {
            double offsetX = ParentBoard.MouseX - Center.X;
            double offsetY = ParentBoard.MouseY - Center.Y;
            Center.ForceStartDrag(ParentBoard.Mouse, -offsetX, -offsetY);
        });

        Center.OnMoved.Add((_, _, _, _) => {
            Formula.CenterX = Center.X;
            Formula.CenterY = Center.Y;
        });
        
        InvalidateVisual();
    }

    public Arc(Vertex center, double radius, double totalAngle) : this(center, radius, 0, totalAngle) { }

    Pen pen = new Pen
    {
        Brush = UIColors.ConnectionColor,
        Thickness = UIDesign.ConnectionGraphicWidth,
        LineCap = PenLineCap.Round,
        LineJoin = PenLineJoin.Round
    };
    public override void Render(DrawingContext context)
    {
        var figure = new PathFigure
        {
            StartPoint = StartDegrees > EndDegrees ? StartEdge : EndEdge,
            IsClosed = false,
            IsFilled = false
        };
        figure.Segments?.Add(new ArcSegment()
        {
            Point = StartDegrees > EndDegrees ? EndEdge : StartEdge,
            Size = new Size(Radius, Radius),
            SweepDirection = StartDegrees > EndDegrees ? SweepDirection.CounterClockwise : SweepDirection.Clockwise,
            IsLargeArc = StartDegrees > EndDegrees == TotalDegrees > 180
        });

        context.DrawGeometry(null, new Pen(UIColors.ConnectionColor, UIDesign.ConnectionGraphicWidth), new PathGeometry()
        {
            Figures = { figure }
        });
    }
}
