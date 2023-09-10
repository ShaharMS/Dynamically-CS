using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;

namespace Dynamically.Shapes;

public partial class Quadrilateral
{
    /// <summary>
    /// ABC most similar to 90deg, position of B is preserved.
    /// length of side is Avg of AB and BC
    /// </summary>
    public void MakeSquareRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);
        var remainingGap = Math.PI / 2 - Math.Abs(radBC.RadiansBetween(radBA, true));
        var dist = (A.DistanceTo(B) + B.DistanceTo(C)) / 2;

        int oppose = Math.Abs((radBA + remainingGap / 2).RadiansBetween(radBC - remainingGap / 2, true)) < Math.PI / 2 ? -1 : 1;


        A.X = B.X + dist * Math.Cos(radBA + remainingGap / 2 * oppose);
        A.Y = B.Y + dist * Math.Sin(radBA + remainingGap / 2 * oppose);
        C.X = B.X + dist * Math.Cos(radBC - remainingGap / 2 * oppose);
        C.Y = B.Y + dist * Math.Sin(radBC - remainingGap / 2 * oppose);
        D.X = B.X + dist * Math.Sqrt(2) * Math.Cos((radBA + radBC) / 2);
        D.Y = B.Y + dist * Math.Sqrt(2) * Math.Sin((radBA + radBC) / 2);
    }

    public void MakeRhombusRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

        var length = new[] { con1.Length, con2.Length, con3.Length, con4.Length }.Average();
        Segment? AB = A.GetConnectionTo(B), BC = B.GetConnectionTo(C);

        AB?.SetLength(length, AB.joint1 == B);
        BC?.SetLength(length, BC.joint1 == B);

        Segment? AD = A.GetConnectionTo(D), CD = C.GetConnectionTo(D);
        AD?.SetLength(length, AD.joint1 == A);
        CD?.SetLength(length, CD.joint1 == C);
    }

    public void ForceType(QuadrilateralType type, Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        Point a, b, c, d;
        _type = type;
        do
        {
            a = A; b = B; c = C; d = D;
            switch (type)
            {
                case QuadrilateralType.SQUARE: MakeSquareRelativeToABC(A, B, C); break;
                case QuadrilateralType.RHOMBUS: MakeRhombusRelativeToABC(A, B, C); break;
                default: break;
            }
            A.DispatchOnMovedEvents(); B.DispatchOnMovedEvents(); C.DispatchOnMovedEvents(); D.DispatchOnMovedEvents();
        } while (!a.Equals(A) || !b.Equals(B) || !c.Equals(C) || !d.Equals(D));
    }
}
