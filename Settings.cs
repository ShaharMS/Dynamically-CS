using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically;

public class Settings
{
    public static int VertexMergeDistance = 20;
    public static int VertexMountDistance = 20;
    public static int SegmentMergeAngleOffset = 10;
    public static int SegmentParallelingAngleOffset = 10;
    public static int SegmentStraighteningAngleOffset = 10;
    public static double MakeDiameterLengthRatio = 0.9;

    public static int MakeEquilateralAngleOffset = 8;
    public static double MakeIsoscelesSideRatioDiff = 0.9;
    public static int MakeRightAngleOffset = 10;

    public static double DistanceCountsAsNear = 5;
    public static double IntersectionGenerationDistance = 10;
}
