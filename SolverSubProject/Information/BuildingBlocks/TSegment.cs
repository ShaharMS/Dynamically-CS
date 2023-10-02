using Dynamically.Backend;
using Dynamically.Solver.Interfaces;
using Dynamically.Solver.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TSegment : ExerciseToken, IMountable, IConstructed
{
    public List<ExerciseToken> Parts { get; private set; }
    public List<ExerciseToken> AllMounts { get; private set; }

    public List<SVertexOnSegmentRatio> MountsWithRatio = new();
    public List<TVertex> Midpoints = new();
    public List<TSegment> Intersectors = new();
    public List<SSegmentOnArcRatio> IntersectsWithRatio = new();
    public List<TSegment> Bisectors = new();
    public TSegment(TVertex first, TVertex last)
    {
        Parts = new List<ExerciseToken> { first, last };
        AllMounts = new();

        Id = first.Id + last.Id;

        first.Segments.Add(this);
        last.Segments.Add(this);

    }
}