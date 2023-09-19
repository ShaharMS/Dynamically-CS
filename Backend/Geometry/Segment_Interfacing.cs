using Avalonia;
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


    public void reposition()
    {
        org1X = joint1.X;
        org1Y = joint1.Y;
        org2X = joint2.X;
        org2Y = joint2.Y;
    }

    public override string ToString()
    {
        return ((char)Math.Min(joint1.Id, joint2.Id)).ToString() + ((char)Math.Max(joint1.Id, joint2.Id)).ToString();
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
        reposition();
    }

    public void __repositionLabel(double z, double x, double c, double v)
    {
        _ = z; _ = x; _ = c; _ = v; // Suppress unused params warning
        Canvas.SetLeft(Label, MiddleFormula.pointOnRatio.X - Label.GuessTextWidth() / 2);
        Canvas.SetTop(Label, MiddleFormula.pointOnRatio.Y - Label.Height / 2);
    }

    public bool SharesJointWith(Segment s)
    {
        return joint1 == s.joint1 || joint1 == s.joint2 || joint2 == s.joint1 || joint2 == s.joint2;
    }

    public Joint? GetSharedJoint(Segment s)
    {
        if (s.joint1 == joint1 || s.joint2 == joint1) return joint1;
        if (s.joint1 == joint2 || s.joint2 == joint2) return joint2;
        return null;
    }

    public bool Contains(Joint joint)
    {
        return joint1 == joint || joint2 == joint;
    }

    public bool Contains(Segment segment)
    {
        return segment.Contains(joint1) && segment.Contains(joint2);
    }

    public bool HasMounted(Joint joint)
    {
        return Formula.Followers.Contains(joint) || MiddleFormula.Followers.Contains(joint);
    }

    public bool HasMounted(Segment segment)
    {
        return false;
    }

    public void Dismantle()
    {
        joint1.Disconnect(joint2);
        MainWindow.BigScreen.Children.Remove(Label);
    }
#pragma warning restore IDE1006
    public bool EncapsulatedWithin(Rect rect)
    {
        return joint1.EncapsulatedWithin(rect) && joint2.EncapsulatedWithin(rect);
    }
}
