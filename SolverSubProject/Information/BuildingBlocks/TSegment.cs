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

    /// <summary>
    /// <code>
    /// <br/>                 / last
    /// <br/>                /
    /// <br/>               /
    /// <br/>              / first
    /// <br/>             /
    /// <br/>       here /
    /// </code>
    /// </summary>
    /// <returns></returns>
    public List<TVertex> MountsBeforeFirst = new();

    /// <summary>
    /// <code>
    /// <br/>            here / 
    /// <br/>                /
    /// <br/>               / last
    /// <br/>              / 
    /// <br/>             /
    /// <br/>            / first
    /// </code>
    /// </summary>
    /// <returns></returns>
    public List<TVertex> MountsAfterLast = new();

    public List<TVertex> MountsBeforeFirstOrAfterLast(TVertex v) => v == First ? MountsBeforeFirst : MountsAfterLast;

    public TVertex First {get => (TVertex)Parts.First(); }
    public TVertex Last {get => (TVertex)Parts.Last(); }

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
        first.PartOf.Add(this);
        last.Segments.Add(this);
        last.PartOf.Add(this);

    }
}