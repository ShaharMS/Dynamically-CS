using Dynamically.Solver.Information.BuildingBlocks;
using Dynamically.Solver.Interfaces;
using Dynamically.Solver.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TArc : ExerciseToken, IConstructed
{
    public List<ExerciseToken> Parts { get; private set; }
    public List<ExerciseToken> AllMounts { get; private set; }

    public List<SVertexOnArcRatio> MountsWithRatio = new();
    public List<TVertex> Midpoints = new();
    public List<TSegment> Intersectors = new();
    public List<SSegmentOnArcRatio> IntersectsWithRatio = new();
    public List<TSegment> Bisectors = new();
    public TArc(TVertex start, TVertex end)
    {
        Parts = new List<ExerciseToken> { start, end };
        AllMounts = new();

        Id = start.Id + end.Id;

        start.Arcs.Add(this);
        end.Arcs.Add(this);

    }
}