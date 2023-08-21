using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Design;

public class UIColors
{
    public static IBrush ShapeHoverFill = new SolidColorBrush(Color.FromUInt32(0x22FFFFFF));
    public static IBrush ShapeFill = new SolidColorBrush(Color.FromUInt32(0x00000000));
    public static IBrush ConnectionColor = new SolidColorBrush(Colors.White);
    public static IBrush JointOutlineColor = new SolidColorBrush(Colors.White);
    public static IBrush JointFillColor = new SolidColorBrush(Colors.Black);
}
