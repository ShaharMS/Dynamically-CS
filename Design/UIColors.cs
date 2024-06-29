using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Design;

public class UIColors
{
    public static readonly IBrush ShapeHoverFill = new SolidColorBrush(Color.FromUInt32(0x22FFFFFF));
    public static readonly IBrush ShapeFill = new SolidColorBrush(Color.FromUInt32(0x00000000));
    public static readonly IBrush SegmentColor = new SolidColorBrush(Colors.White);
    public static readonly IBrush VertexOutlineColor = new SolidColorBrush(Colors.White);
    public static readonly IBrush VertexFillColor = new SolidColorBrush(Colors.Black);
    public static readonly IBrush BottomNoteFill = new SolidColorBrush(Color.FromUInt32(0x66000000));
    public static readonly IBrush BottomNoteColor = new SolidColorBrush(Colors.White);
    public static readonly IBrush SelectionFill = new SolidColorBrush(Color.FromUInt32(0x220000FF));

    public static Pen SelectionOutline => new()
    {
        Brush = new SolidColorBrush(Colors.White),
        DashStyle = DashStyle.Dash,
        Thickness = UIDesign.SelectionOutlineWidth
    };
    public static Pen SegmentPen => new() { Brush = SegmentColor, Thickness = UIDesign.SegmentGraphicWidth };


    public static readonly IBrush SolutionTableBorder = new SolidColorBrush(Colors.White);
    public static readonly IBrush SolutionTableFill = new SolidColorBrush(Colors.Black);
    public static readonly IBrush BoardSquaresColor = new SolidColorBrush(Colors.RoyalBlue, 0.5);
    public static readonly IBrush BoardColor = new SolidColorBrush(Colors.Black);
}
