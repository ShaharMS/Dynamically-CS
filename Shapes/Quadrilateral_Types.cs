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
    public void MakeSquareRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

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

    public void MakeRhombusRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

        var length = new[] { Con1.Length, Con2.Length, Con3.Length, Con4.Length }.Average();
        Segment? AB = A.GetConnectionTo(B), BC = B.GetConnectionTo(C);

        AB?.SetLength(length, AB.Vertex1 == B);
        BC?.SetLength(length, BC.Vertex1 == B);

        Segment? AD = A.GetConnectionTo(D), CD = C.GetConnectionTo(D);
        AD?.SetLength(length, AD.Vertex1 == A);
        CD?.SetLength(length, CD.Vertex1 == C);
    }

    public void MakeRectangleRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);

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

    public void MakeParallelogramRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);

        D.X = A.X + B.DistanceTo(C) * Math.Cos(radBC);
        D.Y = A.Y + B.DistanceTo(C) * Math.Sin(radBC);
    }

    public void MakeKiteRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        double length1 = (A.DistanceTo(B) + C.DistanceTo(B)) / 2, length2 = (A.DistanceTo(D) + C.DistanceTo(D)) / 2;

        Segment? AB = A.GetConnectionTo(B), BC = B.GetConnectionTo(C);

        AB?.SetLength(length1, AB.Vertex1 == B);
        BC?.SetLength(length1, BC.Vertex1 == B);

        Segment? AD = A.GetConnectionTo(D), CD = C.GetConnectionTo(D);
        AD?.SetLength(length2, AD.Vertex1 == A);
        CD?.SetLength(length2, CD.Vertex1 == C);
    }

    /// <summary>
    /// AB represents the Slope of the parallel pair. C's position is locked.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeTrapezoidRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);

        D.X = C.X + D.DistanceTo(C) * Math.Cos(radBA);
        D.Y = C.Y + D.DistanceTo(C) * Math.Sin(radBA);
    }

    /// <summary>
    /// AB represents the Slope of the parallel pair.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeIsoscelesTrapezoidRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);

        D.X = C.X + D.DistanceTo(C) * Math.Cos(radBA);
        D.Y = C.Y + D.DistanceTo(C) * Math.Sin(radBA);
    }

    /// <summary>
    /// AB represents the Slope of the parallel pair. ABC (or BAC) = 90deg.
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeRightTrapezoidRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
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
    public void ForceType(QuadrilateralType type, Vertex A, Vertex B, Vertex C)
    {
        var D = new[] { Vertex1, Vertex2, Vertex3, Vertex4 }.Where(j => j != A && j != B && j != C).ElementAt(0);
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
        } while (!a.RoughlyEquals(A) || !b.RoughlyEquals(B) || !c.RoughlyEquals(C) || !d.RoughlyEquals(D));
    }

    QuadrilateralType ChangeType(QuadrilateralType type)
    {
        // Actual shape modification
        switch (type)
        {
            case QuadrilateralType.SQUARE:
                var angle1ClosenessTo90Deg = Math.Abs(90 - Degrees1);
                var angle2ClosenessTo90Deg = Math.Abs(90 - Degrees2);
                var angle3ClosenessTo90Deg = Math.Abs(90 - Degrees3);
                var angle4ClosenessTo90Deg = Math.Abs(90 - Degrees4);
                var closest = angle1ClosenessTo90Deg.Min(angle2ClosenessTo90Deg).Min(angle3ClosenessTo90Deg).Min(angle4ClosenessTo90Deg);
                if (closest == angle1ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, Angle1Joints[0], Angle1Joints[1], Angle1Joints[2]);
                else if (closest == angle2ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, Angle2Joints[0], Angle2Joints[1], Angle2Joints[2]);
                else if (closest == angle3ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, Angle3Joints[0], Angle3Joints[1], Angle3Joints[2]);
                else if (closest == angle4ClosenessTo90Deg) ForceType(QuadrilateralType.SQUARE, Angle4Joints[0], Angle4Joints[1], Angle4Joints[2]);
                break;
            case QuadrilateralType.RECTANGLE:
                var _angle1ClosenessTo90Deg = Math.Abs(90 - Degrees1);
                var _angle2ClosenessTo90Deg = Math.Abs(90 - Degrees2);
                var _angle3ClosenessTo90Deg = Math.Abs(90 - Degrees3);
                var _angle4ClosenessTo90Deg = Math.Abs(90 - Degrees4);
                var _closest = Math.Min(Math.Min(_angle1ClosenessTo90Deg, _angle2ClosenessTo90Deg), Math.Min(_angle3ClosenessTo90Deg, _angle4ClosenessTo90Deg));
                if (_closest == _angle1ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, Angle1Joints[0], Angle1Joints[1], Angle1Joints[2]);
                else if (_closest == _angle2ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, Angle2Joints[0], Angle2Joints[1], Angle2Joints[2]);
                else if (_closest == _angle3ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, Angle3Joints[0], Angle3Joints[1], Angle3Joints[2]);
                else if (_closest == _angle4ClosenessTo90Deg) ForceType(QuadrilateralType.RECTANGLE, Angle4Joints[0], Angle4Joints[1], Angle4Joints[2]);
                break;
            case QuadrilateralType.RHOMBUS:
                ForceType(QuadrilateralType.RHOMBUS, Angle1Joints[0], Angle1Joints[1], Angle1Joints[2]);
                break;
            case QuadrilateralType.PARALLELOGRAM:
                ForceType(QuadrilateralType.PARALLELOGRAM, Angle1Joints[0], Angle1Joints[1], Angle1Joints[2]);
                break;
            case QuadrilateralType.KITE:
                double similarity1 = Adjacents[0].Item1.Length.GetSimilarityPercentage(Adjacents[0].Item2.Length),
                        similarity2 = Adjacents[1].Item1.Length.GetSimilarityPercentage(Adjacents[1].Item2.Length),
                        similarity3 = Adjacents[2].Item1.Length.GetSimilarityPercentage(Adjacents[2].Item2.Length),
                        similarity4 = Adjacents[3].Item1.Length.GetSimilarityPercentage(Adjacents[3].Item2.Length);
                var similar = similarity1.Min(similarity2).Min(similarity3).Min(similarity4);
                IEnumerable<Vertex> set = new List<Vertex>();
                Vertex? focus = null;
                if (similar == similarity1)
                {
                    set = new[] { Adjacents[0].Item1.Vertex1, Adjacents[0].Item1.Vertex2, Adjacents[0].Item2.Vertex1, Adjacents[0].Item2.Vertex2 }.Where(x => x != Adjacents[0].Item1.GetSharedJoint(Adjacents[0].Item2));
                    focus = Adjacents[0].Item1.GetSharedJoint(Adjacents[0].Item2);
                }
                else if (similar == similarity2)
                {
                    set = new[] { Adjacents[1].Item1.Vertex1, Adjacents[1].Item1.Vertex2, Adjacents[1].Item2.Vertex1, Adjacents[1].Item2.Vertex2 }.Where(x => x != Adjacents[1].Item1.GetSharedJoint(Adjacents[1].Item2));
                    focus = Adjacents[1].Item1.GetSharedJoint(Adjacents[1].Item2);
                }
                else if (similar == similarity3)
                {
                    set = new[] { Adjacents[2].Item1.Vertex1, Adjacents[2].Item1.Vertex2, Adjacents[2].Item2.Vertex1, Adjacents[2].Item2.Vertex2 }.Where(x => x != Adjacents[2].Item1.GetSharedJoint(Adjacents[2].Item2));
                    focus = Adjacents[2].Item1.GetSharedJoint(Adjacents[2].Item2);
                }
                else if (similar == similarity4)
                {
                    set = new[] { Adjacents[3].Item1.Vertex1, Adjacents[3].Item1.Vertex2, Adjacents[3].Item2.Vertex1, Adjacents[3].Item2.Vertex2 }.Where(x => x != Adjacents[3].Item1.GetSharedJoint(Adjacents[3].Item2));
                    focus = Adjacents[3].Item1.GetSharedJoint(Adjacents[3].Item2);
                }

                ForceType(QuadrilateralType.KITE, set.First(), focus ?? Vertex1 /*never happens anyways*/, set.Last());

                break;
            case QuadrilateralType.TRAPEZOID:
            case QuadrilateralType.ISOSCELES_TRAPEZOID:
            case QuadrilateralType.RIGHT_TRAPEZOID:
                var opposites1Similarity = Opposites[0].Item1.Radians.RadiansBetween(Opposites[0].Item2.Radians);
                var opposites2Similarity = Opposites[1].Item1.Radians.RadiansBetween(Opposites[1].Item2.Radians);
                var _similarity = opposites1Similarity.Min(opposites2Similarity);
                if (_similarity == opposites1Similarity) ForceType(type, Opposites[0].Item1.Vertex1, Opposites[0].Item1.Vertex2, Opposites[0].Item2.Vertex1 /*the joint itself doesnt matter, just needs to be different from the others*/);
                else ForceType(type, Opposites[1].Item1.Vertex1, Opposites[1].Item1.Vertex2, Opposites[1].Item2.Vertex1 /*the joint itself doesnt matter, just needs to be different from the others*/);
                break;
        }
        Vertex1.Reposition(); Vertex2.Reposition(); Vertex3.Reposition(); Vertex4.Reposition();
        _type = type;
        Provider.Regenerate();
        return type;
    }
}
