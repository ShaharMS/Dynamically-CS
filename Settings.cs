using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically;

public class Settings
{
    public static int VertexMergeDistance {get; set;} = 20;
    public static int VertexMountDistance {get; set;} = 20;
    public static int SegmentMergeAngleOffset {get; set;} = 10;
    public static int SegmentParallelingAngleOffset {get; set;} = 10;
    public static int SegmentStraighteningAngleOffset {get; set;} = 10;
    public static double MakeDiameterLengthRatio {get; set;} = 0.9;

    public static int MakeEquilateralAngleOffset {get; set;} = 8;
    public static double MakeIsoscelesSideRatioDiff {get; set;} = 0.9;
    public static int MakeRightAngleOffset {get; set;} = 10;

    public static bool Debug {get; set;} = true;


    public static int VertexGraphicCircleRadius {get; set;} = 10;
    public static int SegmentGraphicWidth {get; set;} = 4;
    public static int SelectionOutlineWidth {get; set;} = 4;
    public static int BoardSquareSize {get; set;} = 50;
}
