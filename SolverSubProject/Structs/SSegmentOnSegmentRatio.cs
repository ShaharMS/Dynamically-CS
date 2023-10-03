using Dynamically.Solver.Information;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Structs;

public struct SSegmentOnSegmentRatio
{
    public TSegment Intersector;

    public TSegment Segment;

    public double RatioV1;
    public double RatioV2;

    public SSegmentOnSegmentRatio(TSegment i, TSegment s, double? rv1, double? rv2)
    {
        Intersector = i;
        Segment = s;
        RatioV1 = (double)(rv1 != null ? rv1 : rv2 != null ? 1 - rv2 : throw new ArgumentException("rv1/rv2 must be non-null, and the other null. both are null"));
        RatioV2 = 1 - RatioV1;
    }
}