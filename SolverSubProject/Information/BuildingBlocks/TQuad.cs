using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dynamically.Solver.Helpers;
using Dynamically.Solver.Information.BuildingBlocks;
using Dynamically.Solver.Interfaces;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TQuad : ExerciseToken, IConstructed
{
    public List<ExerciseToken> Parts {get; private set;}

    public TVertex V1 {get => (TVertex)Parts[0];}
    public TVertex V2 {get => (TVertex)Parts[1];}
    public TVertex V3 {get => (TVertex)Parts[2];}
    public TVertex V4 {get => (TVertex)Parts[3];}

    /// <summary>
    /// V1 -> V2 -> V3 -> V4
    /// </summary>
    public TVertex[] Vertices => Parts.Cast<TVertex>().ToArray();
    
    public TSegment V1V2 {get => V1.GetOrCreateSegment(V2); }
    public TSegment V2V3 {get => V2.GetOrCreateSegment(V3); }
    public TSegment V3V4 {get => V3.GetOrCreateSegment(V4); }
    public TSegment V1V4 {get => V1.GetOrCreateSegment(V4); }    

    /// <summary>
    /// V1V2 -> V2V3 -> V3V4 -> V1V4
    /// </summary>
    public TSegment[] Sides => new[] {V1V2, V2V3, V3V4, V1V4};

    public TAngle V1V2V3 {get => V2.GetAngle(V1, V3); }
    public TAngle V2V3V4 {get => V3.GetAngle(V2, V4); }
    public TAngle V1V4V3 {get => V4.GetAngle(V1, V3); }
    public TAngle V2V1V4 {get => V1.GetAngle(V2, V4); }


    public TCircle? CircumCircle { get; set; }

    public TCircle? InCircle { get; set; }

    public TQuad(TVertex v1, TVertex v2, TVertex v3, TVertex v4)
    {
        Parts = new List<ExerciseToken> { v1, v2, v3, v4 };
    }

    public TQuad(TSegment v1v2, TSegment v2v3, TSegment v3v4, TSegment v1v4) 
        : this(v1v2.GetSharedVertexOrThrow(v1v4), v1v2.GetSharedVertexOrThrow(v2v3), v2v3.GetSharedVertexOrThrow(v3v4), v3v4.GetSharedVertexOrThrow(v1v4)) { }
}
