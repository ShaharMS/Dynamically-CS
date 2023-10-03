using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamically.Solver.Helpers;
using Dynamically.Solver.Information.BuildingBlocks;
using Dynamically.Solver.Interfaces;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TTriangle : ExerciseToken, IConstructed
{
    public List<ExerciseToken> Parts {get; private set;}

    public TVertex V1 {get => (TVertex)Parts[0];}
    public TVertex V2 {get => (TVertex)Parts[1];}
    public TVertex V3 {get => (TVertex)Parts[2];}

    public TSegment V1V2 {get => V1.GetOrCreateSegment(V2); }
    public TSegment V2V3 {get => V2.GetOrCreateSegment(V3); }
    public TSegment V1V3 {get => V1.GetOrCreateSegment(V3); }
    public TAngle V1V2V3 {get => V2.GetAngle(V1, V3); }
    public TAngle V1V3V2 {get => V3.GetAngle(V1, V2); }
    public TAngle V2V1V3 {get => V1.GetAngle(V2, V3); }
}
