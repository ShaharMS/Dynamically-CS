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

    /// <summary>
    /// V1 -> V2 -> V3
    /// </summary>
    public TVertex[] Vertices => Parts.Cast<TVertex>().ToArray();
    
    public TSegment V1V2 {get => V1.GetOrCreateSegment(V2); }
    public TSegment V2V3 {get => V2.GetOrCreateSegment(V3); }
    public TSegment V1V3 {get => V1.GetOrCreateSegment(V3); }

    /// <summary>
    /// V1V2 -> V2V3 -> V1V3
    /// </summary>
    public TSegment[] Sides => new[] {V1V2, V2V3, V1V3};

    public TAngle V1V2V3 {get => V2.GetAngle(V1, V3); }
    public TAngle V1V3V2 {get => V3.GetAngle(V1, V2); }
    public TAngle V2V1V3 {get => V1.GetAngle(V2, V3); }

    public TCircle? CircumCircle { get; set; }

    public TCircle? InCircle { get; set; }

    public TTriangle(TVertex v1, TVertex v2, TVertex v3)
    {
        Parts = new List<ExerciseToken> { v1, v2, v3 };
    }

    public TTriangle(TSegment v1v2, TSegment v2v3, TSegment v1v3) 
        : this(v1v2.GetSharedVertexOrThrow(v1v3), v1v2.GetSharedVertexOrThrow(v2v3), v1v3.GetSharedVertexOrThrow(v2v3)) { }
}
