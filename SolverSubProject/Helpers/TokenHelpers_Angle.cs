using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverSubProject.Helpers;

public static partial class TokenHelpers
{

    public static bool HasAdjacentAngles(this TAngle angle) => angle.Segment1?.AllMounts.Where(x => x is TVertex).Count() > 0 || angle.Segment2?.AllMounts.Where(x => x is TVertex).Count() > 0;

    public static TAngle[] GetAdjacentAngles(this TAngle angle)
    {
        if (!HasAdjacentAngles(angle)) return Array.Empty<TAngle>();


    }

    public static bool HasVertexAngle(this TAngle angle) => angle.Segment1?.AllMounts.Where(x => x is TVertex).Count() > 0 && angle.Segment2?.AllMounts.Where(x => x is TVertex).Count() > 0;

}
