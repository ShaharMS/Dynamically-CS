using Dynamically.Solver.Information;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Structs;

public struct SSegmentOnArcRatio
{
    public TSegment Intersector;

    public TArc Arc;

    public double RatioV1;
    public double RatioV2;

    public SSegmentOnArcRatio(TSegment i, TArc a, double? rv1, double? rv2)
    {
        Intersector = i;
        Arc = a;
        RatioV1 = (double)(rv1 != null ? rv1 : rv2 != null ? 1 - rv2 : throw new Exception("rv1/rv2 must be non-null, and the other null. both are null"));
        RatioV2 = 1 - RatioV1;
    }
}