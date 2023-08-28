using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically;

public class Settings
{
    public static int JointMergeDistance = 20;
    public static int JointMountDistance = 20;
    public static int ConnectionMergeAngleOffset = 10;
    public static int ConnectionParallelingAngleOffset = 10;
    public static int ConnectionStraighteningAngleOffset = 10;
    public static double MakeDiameterLengthRatio = 0.9;

    public static int MakeEquilateralAngleOffset = 5;
    public static double MakeIsocelesSideRatioDiff = 0.1;
    public static int MakeRightAngleOffset = 10;
}
