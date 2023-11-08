using Avalonia;
using Dynamically.Backend.Interfaces;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Angle : IStringifyable, ISupportsAdjacency, IContextMenuSupporter<AngleContextMenuProvider>, ISelectable
{
    public AngleContextMenuProvider Provider { get; }
    private void __updateAngle(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v;
        BisectorRay.Set(Center, Math.Tan(GetBisectorRadians()));
        InvalidateVisual();
    }
    public override string ToString()
    {
        return $"∠{Vertex1}{Center}{Vertex2}";
    }
    public string ToString(bool descriptive)
    {
        return descriptive ? "Angle " + ToString() : ToString();
    }

    public override double Area()
    {
        return 2;
    }

    public bool EncapsulatedWithin(Rect rect)
    {
        return Vertex1.EncapsulatedWithin(rect) && Center.EncapsulatedWithin(rect) && Vertex2.EncapsulatedWithin(rect);
    }
}
