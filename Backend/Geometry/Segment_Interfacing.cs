﻿using Avalonia;
using Avalonia.Controls;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Interfaces;
using Dynamically.Formulas;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Segment : IDrawable, IDismantable, IStringifyable, ISupportsAdjacency, IHasFormula<SegmentFormula>, IContextMenuSupporter<SegmentContextMenuProvider>, Interfaces.ISelectable
{
    public SegmentFormula Formula { get; set; }
    public RatioOnSegmentFormula MiddleFormula { get; }

    public SegmentContextMenuProvider Provider { get; }


    public void Reposition()
    {
        org1X = Vertex1.X;
        org1Y = Vertex1.Y;
        org2X = Vertex2.X;
        org2Y = Vertex2.Y;
    }

    public override string ToString()
    {
        return ((char)Math.Min(Vertex1.Id, Vertex2.Id)).ToString() + ((char)Math.Max(Vertex1.Id, Vertex2.Id)).ToString();
    }

    public string ToString(bool descriptive)
    {
        if (!descriptive) return ToString();
        return "Segment " + ToString();
    }

    public override double Area()
    {
        return 3;
    }

#pragma warning disable IDE1006
    public void __reposition(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v; // Suppress unused params warning
        Reposition();
    }

    public void __repositionLabel(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v; // Suppress unused params warning
        Canvas.SetLeft(Label, MiddleFormula.PointOnRatio.X - Label.GuessTextWidth() / 2);
        Canvas.SetTop(Label, MiddleFormula.PointOnRatio.Y - Label.Height / 2);
    }

    public bool SharesJointWith(Segment s)
    {
        return Vertex1 == s.Vertex1 || Vertex1 == s.Vertex2 || Vertex2 == s.Vertex1 || Vertex2 == s.Vertex2;
    }

    public Vertex? GetSharedJoint(Segment s)
    {
        if (s.Vertex1 == Vertex1 || s.Vertex2 == Vertex1) return Vertex1;
        if (s.Vertex1 == Vertex2 || s.Vertex2 == Vertex2) return Vertex2;
        return null;
    }

    public bool Contains(Vertex joint)
    {
        return Vertex1 == joint || Vertex2 == joint;
    }

    public bool Contains(Segment segment)
    {
        return segment.Contains(Vertex1) && segment.Contains(Vertex2);
    }

    public bool HasMounted(Vertex joint)
    {
        return Formula.Followers.Contains(joint) || MiddleFormula.Followers.Contains(joint);
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public void Dismantle()
    {
        Vertex1.Disconnect(Vertex2);
        MainWindow.Instance.MainBoard.Children.Remove(Label);
    }
#pragma warning restore IDE1006
    public bool EncapsulatedWithin(Rect rect)
    {
        return Vertex1.EncapsulatedWithin(rect) && Vertex2.EncapsulatedWithin(rect);
    }
}
