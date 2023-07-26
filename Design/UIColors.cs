using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Design;

public class UIColors
{
    public static IBrush ShapeHoverFill = new SolidColorBrush(Colors.LightGray);
    public static IBrush ShapeFill = new SolidColorBrush(Color.FromUInt32(0xFFFFFFFF));
    public static IBrush ConnectionColor = new SolidColorBrush(Colors.Black);
    public static IBrush JointOutlineColor = new SolidColorBrush(Colors.Black);
    public static IBrush JointFillColor = new SolidColorBrush(Colors.White);
}
