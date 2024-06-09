using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Triangle
{
    private Point EQ_temp_incircle_center = new(-1, -1);
    private void Equilateral_OnJointMove(Vertex moved, Vertex other1, Vertex other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;
        if (moved.Anchored || other1.Anchored || other2.Anchored) return;
        var center = EQ_temp_incircle_center;
        var len = center.DistanceTo(moved);
        var angle = center.RadiansTo(moved);
        var arr = new Vertex[2];

        if (angle.RadiansBetween(center.RadiansTo(other1), true) > angle.RadiansBetween(center.RadiansTo(other2), true)) arr = new[] { other1, other2 };
        else arr = new[] { other2, other1 };
        foreach (var o in arr)
        {
            angle += 2 * Math.PI / 3;
            o.X = center.X + len * Math.Cos(angle);
            o.Y = center.Y + len * Math.Sin(angle);
            o.DispatchOnMovedEvents();
        }
    }

    private Vertex R_origin;
    private void Right_OnJointMove(Vertex moved, Vertex other1, Vertex other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;

        if (moved == R_origin)
        {
            var pos = new Point(px, py);

            if (other1.Anchored && other2.Anchored)
            {
                moved.X = px; moved.Y = py;
            }
            else if (other1.Anchored)
            {
                var ray = new RayFormula(pos, other1);
                var p = ray.GetClosestOnFormula(new Point(ParentBoard.MouseX, ParentBoard.MouseY));
                if (p != null)
                {
                    moved.X = p.Value.X; moved.Y = p.Value.Y;
                    other2.X += moved.X - px;
                    other2.Y += moved.Y - py;
                }
            }
            else if (other2.Anchored)
            {
                var ray = new RayFormula(pos, other2);
                var p = ray.GetClosestOnFormula(new Point(ParentBoard.MouseX, ParentBoard.MouseY));
                if (p != null)
                {
                    moved.X = p.Value.X; moved.Y = p.Value.Y;
                    other1.X += moved.X - px;
                    other1.Y += moved.Y - py;
                }
            }
            else
            {
                other1.X += moved.X - px; other1.Y += moved.Y - py;
                other2.X += moved.X - px; other2.Y += moved.Y - py;
            }

        }
        else
        {
            var other = other1 == R_origin ? other2 : other1;

            var radToMoved = R_origin.RadiansTo(moved);
            var radToOther = R_origin.RadiansTo(other);
            var dist = other.DistanceTo(R_origin);
            if ((radToMoved + Math.PI / 2).RadiansBetween(radToOther) < (radToOther + Math.PI / 2).RadiansBetween(radToMoved))
            {
                if (other.Anchored)
                {
                    dist = moved.DistanceTo(R_origin);
                    radToOther -= Math.PI / 2;
                    moved.X = R_origin.X + dist * Math.Cos(radToOther);
                    moved.Y = R_origin.Y + dist * Math.Sin(radToOther);
                }
                else
                {
                    radToMoved += Math.PI / 2;
                    other.X = R_origin.X + dist * Math.Cos(radToMoved);
                    other.Y = R_origin.Y + dist * Math.Sin(radToMoved);
                }
            }
            else
            {
                if (other.Anchored)
                {
                    dist = moved.DistanceTo(R_origin);
                    radToOther += Math.PI / 2;
                    moved.X = R_origin.X + dist * Math.Cos(radToOther);
                    moved.Y = R_origin.Y + dist * Math.Sin(radToOther);
                }
                else
                {
                    radToMoved -= Math.PI / 2;
                    other.X = R_origin.X + dist * Math.Cos(radToMoved);
                    other.Y = R_origin.Y + dist * Math.Sin(radToMoved);
                }
            }

        }

    }

    private Vertex ISO_origin;

    private void Isoceles_OnJointMove(Vertex moved, Vertex other1, Vertex other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;

        if (moved == ISO_origin)
        {
            var ray = new RatioOnSegmentFormula(new SegmentFormula(other1, other2), 0.5).GetPerpendicular();

            var p = ray.GetClosestOnFormula(moved);
            if (p != null)
            {
                moved.X = p.Value.X; moved.Y = p.Value.Y;
            }
            return;
        }

        Vertex j1 = other1, j2 = other2;
        if (j1 == ISO_origin) j1 = moved;
        else if (j2 == ISO_origin) j2 = moved;

        var dist = Math.Max(ISO_origin.DistanceTo(other1), ISO_origin.DistanceTo(other2));

        if (ISO_origin.DistanceTo(j1) != dist)
        {
            var radsToOther1 = Math.Atan2(j1.Y - ISO_origin.Y, j1.X - ISO_origin.X);
            j1.X = ISO_origin.X + dist * Math.Cos(radsToOther1);
            j1.Y = ISO_origin.Y + dist * Math.Sin(radsToOther1);
        }
        else
        {
            var radsToOther2 = Math.Atan2(j2.Y - ISO_origin.Y, j2.X - ISO_origin.X);
            j2.X = ISO_origin.X + dist * Math.Cos(radsToOther2);
            j2.Y = ISO_origin.Y + dist * Math.Sin(radsToOther2);
        }
    }

    /// <summary>
    /// ABC is 90degs
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeRightRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        // ∠ABC is the most similar to 90deg, therefore it should be preserved.

        // Fixing the angle is easy, its just editing either A or C
        // For user comfort, we'll modify both A & C to make ABC 90deg.
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);
        var remainingGap = Math.PI / 2 - Math.Abs(radBC.RadiansBetween(radBA, true));
        double distAB = A.DistanceTo(B), distBC = C.DistanceTo(B);

        int oppose = Math.Abs((radBA + remainingGap / 2).RadiansBetween(radBC - remainingGap / 2, true)) < Math.PI / 2 ? -1 : 1;

        A.X = B.X + distAB * Math.Cos(radBA + remainingGap / 2 * oppose);
        A.Y = B.Y + distAB * Math.Sin(radBA + remainingGap / 2 * oppose);
        C.X = B.X + distBC * Math.Cos(radBC - remainingGap / 2 * oppose); 
        C.Y = B.Y + distBC * Math.Sin(radBC - remainingGap / 2 * oppose); 

        R_origin = B;
        // And we're done :)
    }
    
    /// <summary>
    /// AB = BC
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    public void MakeIsoscelesRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        // ∠ABC is the head angle, therefore its position should be preserved, and should be where the two equals start from.
        // We'll do this by averaging AB and BC, resetting their length, and BC will 
        // automatically be the same length as AC, because of equilateral definition.

        // To correct the distances, we'll  make sure the moving joint when setting connection length is not A:

        var distance = Math.Max(B.DistanceTo(A), B.DistanceTo(C));
        var radBA = B.RadiansTo(A);
        A.X = B.X + distance * Math.Cos(radBA);
        A.Y = B.Y + distance * Math.Sin(radBA);
        var radBC = B.RadiansTo(C);
        C.X = B.X + distance * Math.Cos(radBC);
        C.Y = B.Y + distance * Math.Sin(radBC);

        ISO_origin = B;
        // Now, After equating the two sides, we're pretty much dones - we've reached teh definition of an isoceles Triangle
    }
    public void MakeEquilateralRelativeToABC(Vertex A, Vertex B, Vertex C)
    {
        // AB and BC are the most similar to each other, so B was chosen. Now, reset the angle

        
        var radBA = Math.Atan2(A.Y - B.Y, A.X - B.X);
        var radBC = Math.Atan2(C.Y - B.Y, C.X - B.X);
        var remainingGap = Math.PI / 3 - Math.Abs(radBC.RadiansBetween(radBA, true));
        double dist = (A.DistanceTo(B) + C.DistanceTo(B)) / 2;

        int oppose = Math.Abs((radBA + remainingGap / 2).RadiansBetween(radBC - remainingGap / 2, true)) < Math.PI / 3 ? -1 : 1;

        A.X = B.X + dist * Math.Cos(radBA + remainingGap / 2 * oppose);
        A.Y = B.Y + dist * Math.Sin(radBA + remainingGap / 2 * oppose);
        C.X = B.X + dist * Math.Cos(radBC - remainingGap / 2 * oppose); 
        C.Y = B.Y + dist * Math.Sin(radBC - remainingGap / 2 * oppose); 

        EQ_temp_incircle_center = new Point(GetCircleStats().x, GetCircleStats().y);
    }

    public void MakeIsoscelesRightRelativeToABC(Vertex A, Vertex B, Vertex C) {
        MakeRightRelativeToABC(A, B, C);
        MakeIsoscelesRelativeToABC(A, B, C);
    }
    public void ForceType(TriangleType type, Vertex A, Vertex B, Vertex C) {
        Point a, b, c;
        _type = type;
        do {
            a = A; b = B; c = C;
            switch (type) {
                case TriangleType.ISOSCELES_RIGHT: MakeIsoscelesRightRelativeToABC(A, B, C); break;
                case TriangleType.EQUILATERAL: MakeEquilateralRelativeToABC(A, B, C); break;
                case TriangleType.ISOSCELES: MakeIsoscelesRelativeToABC(A, B, C); break;
                case TriangleType.RIGHT: MakeRightRelativeToABC(A, B, C); break;
                case TriangleType.SCALENE: return;
            }
            A.DispatchOnMovedEvents(); B.DispatchOnMovedEvents(); C.DispatchOnMovedEvents();
        } while (!a.RoughlyEquals(A) || !b.RoughlyEquals(B) || !c.RoughlyEquals(C));
    }
    TriangleType ChangeType(TriangleType type)
    {
        // Actual shape modification
        switch (type)
        {
            case TriangleType.EQUILATERAL:
                var a_ABBC_SimilarityOfSides = Math.Abs(Segment12.Length - Segment23.Length);
                var a_ACCB_SimilarityOfSides = Math.Abs(Segment13.Length - Segment23.Length);
                var a_BAAC_SimilarityOfSides = Math.Abs(Segment13.Length - Segment12.Length);
                if (a_ABBC_SimilarityOfSides < a_ACCB_SimilarityOfSides && a_ABBC_SimilarityOfSides < a_BAAC_SimilarityOfSides) ForceType(TriangleType.EQUILATERAL, Vertex1, Vertex2, Vertex3);
                else if (a_ACCB_SimilarityOfSides < a_ABBC_SimilarityOfSides && a_ACCB_SimilarityOfSides < a_BAAC_SimilarityOfSides) ForceType(TriangleType.EQUILATERAL, Vertex1, Vertex3, Vertex2);
                else ForceType(TriangleType.EQUILATERAL, Vertex2, Vertex1, Vertex3);
                break;
            case TriangleType.ISOSCELES:
                var con12_to_con13_Diff = Math.Abs(Segment12.Length - Segment13.Length);
                var con12_to_con23_Diff = Math.Abs(Segment12.Length - Segment23.Length);
                var con13_to_con23_Diff = Math.Abs(Segment13.Length - Segment23.Length);
                if (con12_to_con23_Diff < con13_to_con23_Diff && con12_to_con23_Diff < con12_to_con13_Diff) ForceType(TriangleType.ISOSCELES, Vertex1, Vertex2, Vertex3);
                else if (con12_to_con13_Diff < con13_to_con23_Diff && con12_to_con13_Diff < con12_to_con23_Diff) ForceType(TriangleType.ISOSCELES, Vertex2, Vertex1, Vertex3);
                else ForceType(TriangleType.ISOSCELES, Vertex1, Vertex3, Vertex2);
                break;
            case TriangleType.RIGHT:
                var a_ABC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3));
                var a_ACB_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex3, Vertex2));
                var a_BAC_ClosenessTo90Deg = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex2, Vertex1, Vertex3));
                if (a_ABC_ClosenessTo90Deg < a_ACB_ClosenessTo90Deg && a_ABC_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) ForceType(TriangleType.RIGHT, Vertex1, Vertex2, Vertex3);
                else if (a_ACB_ClosenessTo90Deg < a_ABC_ClosenessTo90Deg && a_ACB_ClosenessTo90Deg < a_BAC_ClosenessTo90Deg) ForceType(TriangleType.RIGHT, Vertex1, Vertex3, Vertex2);
                else ForceType(TriangleType.RIGHT, Vertex2, Vertex1, Vertex3);
                break;
            case TriangleType.ISOSCELES_RIGHT:
                var a_ABC_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex2, Vertex3));
                var a_ACB_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex1, Vertex3, Vertex2));
                var a_BAC_ClosenessTo90Deg1 = Math.Abs(90 - Tools.GetDegreesBetween3Points(Vertex2, Vertex1, Vertex3));
                if (a_ABC_ClosenessTo90Deg1 < a_ACB_ClosenessTo90Deg1 && a_ABC_ClosenessTo90Deg1 < a_BAC_ClosenessTo90Deg1)
                {
                    ForceType(TriangleType.RIGHT, Vertex1, Vertex2, Vertex3);
                    ForceType(TriangleType.ISOSCELES, Vertex1, Vertex2, Vertex3);
                }
                else if (a_ACB_ClosenessTo90Deg1 < a_ABC_ClosenessTo90Deg1 && a_ACB_ClosenessTo90Deg1 < a_BAC_ClosenessTo90Deg1)
                {
                    ForceType(TriangleType.RIGHT, Vertex1, Vertex3, Vertex2);
                    ForceType(TriangleType.ISOSCELES, Vertex1, Vertex3, Vertex2);
                }
                else
                {
                    ForceType(TriangleType.RIGHT, Vertex2, Vertex1, Vertex3);
                    ForceType(TriangleType.ISOSCELES, Vertex2, Vertex1, Vertex3);
                }
                break;
            case TriangleType.SCALENE:
                break;
        }
        Vertex1.Reposition(); Vertex2.Reposition(); Vertex3.Reposition();
        _type = type;
        Provider.Regenerate();
        return type;
    }

}
