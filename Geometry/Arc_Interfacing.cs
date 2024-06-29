using Avalonia;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Interfaces;
using Dynamically.Design;
using Dynamically.Formulas;
using Dynamically.Geometry.Basics;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Geometry;

public partial class Arc : IHasFormula<ArcFormula>, IShape, IStringifyable, IDismantable, IDrawable, IMovementFreezable, ISelectable, ISupportsAdjacency, IContextMenuSupporter<ArcContextMenuProvider>
{
    public ArcFormula Formula { get; set; }

    public CircleFormula CircleFormula { get; set; }

    public ArcContextMenuProvider Provider { get; }

    public bool Contains(Vertex vertex)
    {
        return Center == vertex || StartEdge == vertex || EndEdge == vertex;
    }

    public bool Contains(Segment segment)
    {
        return false;
    }

    public bool HasMounted(Vertex vertex)
    {
        return false;
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public override double Area()
    {
        return Radius * Radius * Math.PI * (TotalDegrees / 360);
    }

public override string ToString()
    {
        return $"◠${StartEdge}${EndEdge}";
    }
    public string ToString(bool descriptive)
    {
        return descriptive ? "Arc - " + ToString() : ToString();
    }

    public void Dismantle()
    {
        ParentBoard.Children.Remove(this);
    }

    public void Reposition()
    {
    }

    public bool IsMovable()
    {
        return Center.IsMovable() && StartEdge.IsMovable() && EndEdge.IsMovable();
    }

    public bool EncapsulatedWithin(Rect rect)
    {
        return Center.EncapsulatedWithin(rect) && StartEdge.EncapsulatedWithin(rect) && EndEdge.EncapsulatedWithin(rect);
    }
}