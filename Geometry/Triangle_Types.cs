using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Geometry.Basics;

namespace Dynamically.Geometry;

public partial class Triangle
{
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

        // To correct the distances, we'll  make sure the moving vertex when setting connection length is not A:

        var distance = Math.Max(B.DistanceTo(A), B.DistanceTo(C));
        var radBA = B.RadiansTo(A);
        A.X = B.X + distance * Math.Cos(radBA);
        A.Y = B.Y + distance * Math.Sin(radBA);
        var radBC = B.RadiansTo(C);
        C.X = B.X + distance * Math.Cos(radBC);
        C.Y = B.Y + distance * Math.Sin(radBC);

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
