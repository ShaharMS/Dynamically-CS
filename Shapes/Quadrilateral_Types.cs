using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;

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
        // Square def: AB || CD
        D.X = C.X + dist * Math.Cos(radBA + remainingGap / 2 * oppose);
        D.Y = C.Y + dist * Math.Sin(radBA + remainingGap / 2 * oppose);
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

    public void MakeRectangleRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);
        var remainingGap = Math.PI / 2 - Math.Abs(radBC.RadiansBetween(radBA, true));
        double length1 = (A.DistanceTo(B) + C.DistanceTo(D)) / 2, length2 = (A.DistanceTo(D) + B.DistanceTo(C)) / 2;

        int oppose = Math.Abs((radBA + remainingGap / 2).RadiansBetween(radBC - remainingGap / 2, true)) < Math.PI / 2 ? -1 : 1;


        A.X = B.X + length1 * Math.Cos(radBA + remainingGap / 2 * oppose);
        A.Y = B.Y + length1 * Math.Sin(radBA + remainingGap / 2 * oppose);
        C.X = B.X + length2 * Math.Cos(radBC - remainingGap / 2 * oppose);
        C.Y = B.Y + length2 * Math.Sin(radBC - remainingGap / 2 * oppose);
        // Rectangle def: AB || CD
        D.X = C.X + length1 * Math.Cos(radBA + remainingGap / 2 * oppose);
        D.Y = C.Y + length1 * Math.Sin(radBA + remainingGap / 2 * oppose);
    }

    public void MakeParallelogramRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);

        D.X = A.X + B.DistanceTo(C) * Math.Cos(radBC);
        D.Y = A.Y + B.DistanceTo(C) * Math.Sin(radBC);
    }

    public void MakeKiteRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        double length1 = (A.DistanceTo(B) + C.DistanceTo(B)) / 2, length2 = (A.DistanceTo(D) + C.DistanceTo(D)) / 2;

        Segment? AB = A.GetConnectionTo(B), BC = B.GetConnectionTo(C);

        AB?.SetLength(length1, AB.joint1 == B);
        BC?.SetLength(length1, BC.joint1 == B);

        Segment? AD = A.GetConnectionTo(D), CD = C.GetConnectionTo(D);
        AD?.SetLength(length2, AD.joint1 == A);
        CD?.SetLength(length2, CD.joint1 == C);
    }

    /// <summary>
    /// AB represents the slope of the parallel pair. C's position is locked.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeTrapezoidRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);

        D.X = C.X + D.DistanceTo(C) * Math.Cos(radBA);
        D.Y = C.Y + D.DistanceTo(C) * Math.Sin(radBA);
    }

    /// <summary>
    /// AB represents the slope of the parallel pair.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeIsoscelesTrapezoidRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);

        D.X = C.X + D.DistanceTo(C) * Math.Cos(radBA);
        D.Y = C.Y + D.DistanceTo(C) * Math.Sin(radBA);
    }

    /// <summary>
    /// AB represents the slope of the parallel pair. ABC (or BAC) = 90deg.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeRightTrapezoidRelativeToABC(Joint A, Joint B, Joint C)
    {
        var D = new[] { joint1, joint2, joint3, joint4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);

        if (C.IsConnectedTo(A))
        {
            
        }
        else
        {

        }

        D.X = C.X + D.DistanceTo(C) * Math.Cos(radBA);
        D.Y = C.Y + D.DistanceTo(C) * Math.Sin(radBA);
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
                case QuadrilateralType.RECTANGLE: MakeRectangleRelativeToABC(A, B, C); break;
                case QuadrilateralType.PARALLELOGRAM: MakeParallelogramRelativeToABC(A, B, C); break;
                case QuadrilateralType.KITE: MakeKiteRelativeToABC(A, B, C); break;
                case QuadrilateralType.TRAPEZOID: MakeTrapezoidRelativeToABC(A, B, C); break;
                case QuadrilateralType.ISOSCELES_TRAPEZOID: MakeIsoscelesTrapezoidRelativeToABC(A, B, C); break;
                case QuadrilateralType.RIGHT_TRAPEZOID: MakeRightTrapezoidRelativeToABC(A, B, C); break;
                default: break;
            }
            A.DispatchOnMovedEvents(); B.DispatchOnMovedEvents(); C.DispatchOnMovedEvents(); D.DispatchOnMovedEvents();
        } while (!a.Equals(A) || !b.Equals(B) || !c.Equals(C) || !d.Equals(D));
    }

    QuadrilateralType ChangeType(QuadrilateralType type)
    {
        // Actual shape modification
        switch (type)
        {
            case QuadrilateralType.SQUARE:
                var angle1ClosenessTo90Deg = Math.Abs(90 - degrees1);
                var angle2ClosenessTo90Deg = Math.Abs(90 - degrees2);
                var angle3ClosenessTo90Deg = Math.Abs(90 - degrees3);
                var angle4ClosenessTo90Deg = Math.Abs(90 - degrees4);
                var closest = angle1ClosenessTo90Deg.Min(angle2ClosenessTo90Deg).Min(angle3ClosenessTo90Deg).Min(angle4ClosenessTo90Deg);
                if (closest == angle1ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                else if (closest == angle2ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle2Joints[0], angle2Joints[1], angle2Joints[2]);
                else if (closest == angle3ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle3Joints[0], angle3Joints[1], angle3Joints[2]);
                else if (closest == angle4ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, angle4Joints[0], angle4Joints[1], angle4Joints[2]);
                break;
            case QuadrilateralType.RECTANGLE:
                var _angle1ClosenessTo90Deg = Math.Abs(90 - degrees1);
                var _angle2ClosenessTo90Deg = Math.Abs(90 - degrees2);
                var _angle3ClosenessTo90Deg = Math.Abs(90 - degrees3);
                var _angle4ClosenessTo90Deg = Math.Abs(90 - degrees4);
                var _closest = Math.Min(Math.Min(_angle1ClosenessTo90Deg, _angle2ClosenessTo90Deg), Math.Min(_angle3ClosenessTo90Deg, _angle4ClosenessTo90Deg));
                if (_closest == _angle1ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                else if (_closest == _angle2ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle2Joints[0], angle2Joints[1], angle2Joints[2]);
                else if (_closest == _angle3ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle3Joints[0], angle3Joints[1], angle3Joints[2]);
                else if (_closest == _angle4ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, angle4Joints[0], angle4Joints[1], angle4Joints[2]);
                break;
            case QuadrilateralType.RHOMBUS:
                ForceType(QuadrilateralType.RHOMBUS, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                break;
            case QuadrilateralType.PARALLELOGRAM:
                ForceType(QuadrilateralType.PARALLELOGRAM, angle1Joints[0], angle1Joints[1], angle1Joints[2]);
                break;
            case QuadrilateralType.KITE:
                double similarity1 = adjacents[0].Item1.Length.GetSimilarityPercentage(adjacents[0].Item2.Length),
                        similarity2 = adjacents[1].Item1.Length.GetSimilarityPercentage(adjacents[1].Item2.Length),
                        similarity3 = adjacents[2].Item1.Length.GetSimilarityPercentage(adjacents[2].Item2.Length),
                        similarity4 = adjacents[3].Item1.Length.GetSimilarityPercentage(adjacents[3].Item2.Length);
                var similar = similarity1.Min(similarity2).Min(similarity3).Min(similarity4);
                IEnumerable<Joint> set = new List<Joint>();
                Joint? focus = null;
                if (similar == similarity1)
                {
                    set = new[] { adjacents[0].Item1.joint1, adjacents[0].Item1.joint2, adjacents[0].Item2.joint1, adjacents[0].Item2.joint2 }.Where(x => x != adjacents[0].Item1.GetSharedJoint(adjacents[0].Item2));
                    focus = adjacents[0].Item1.GetSharedJoint(adjacents[0].Item2);
                }
                else if (similar == similarity2)
                {
                    set = new[] { adjacents[1].Item1.joint1, adjacents[1].Item1.joint2, adjacents[1].Item2.joint1, adjacents[1].Item2.joint2 }.Where(x => x != adjacents[1].Item1.GetSharedJoint(adjacents[1].Item2));
                    focus = adjacents[1].Item1.GetSharedJoint(adjacents[1].Item2);
                }
                else if (similar == similarity3)
                {
                    set = new[] { adjacents[2].Item1.joint1, adjacents[2].Item1.joint2, adjacents[2].Item2.joint1, adjacents[2].Item2.joint2 }.Where(x => x != adjacents[2].Item1.GetSharedJoint(adjacents[2].Item2));
                    focus = adjacents[2].Item1.GetSharedJoint(adjacents[2].Item2);
                }
                else if (similar == similarity4)
                {
                    set = new[] { adjacents[3].Item1.joint1, adjacents[3].Item1.joint2, adjacents[3].Item2.joint1, adjacents[3].Item2.joint2 }.Where(x => x != adjacents[3].Item1.GetSharedJoint(adjacents[3].Item2));
                    focus = adjacents[3].Item1.GetSharedJoint(adjacents[3].Item2);
                }

                ForceType(QuadrilateralType.KITE, set.First(), focus ?? joint1 /*never happens anyways*/, set.Last());

                break;
            case QuadrilateralType.TRAPEZOID:
            case QuadrilateralType.ISOSCELES_TRAPEZOID:
            case QuadrilateralType.RIGHT_TRAPEZOID:
                var opposites1Similarity = opposites[0].Item1.Radians.RadiansBetween(opposites[0].Item2.Radians);
                var opposites2Similarity = opposites[1].Item1.Radians.RadiansBetween(opposites[1].Item2.Radians);
                var _similarity = opposites1Similarity.Min(opposites2Similarity);
                if (_similarity == opposites1Similarity) ForceType(type, opposites[0].Item1.joint1, opposites[0].Item1.joint2, opposites[0].Item2.joint1 /*the joint itself doesnt matter, just needs to be different from the others*/);
                else ForceType(type, opposites[1].Item1.joint1, opposites[1].Item1.joint2, opposites[1].Item2.joint1 /*the joint itself doesnt matter, just needs to be different from the others*/);
                break;
        }
        joint1.reposition(); joint2.reposition(); joint3.reposition(); joint4.reposition();
        _type = type;
        Provider.Regenerate();
        return type;
    }
}
