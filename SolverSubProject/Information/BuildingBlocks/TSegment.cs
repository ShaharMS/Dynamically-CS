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
    /// <br/>                 / V2
    /// <br/>                /
    /// <br/>               /
    /// <br/>              / V1
    /// <br/>             /
    /// <br/>       here /
    /// </code>
    /// </summary>
    /// <returns></returns>
    public List<TVertex> OnV1sExtension = new();

    /// <summary>
    /// <code>
    /// <br/>              V1 / 
    /// <br/>                /
    /// <br/>               / V2
    /// <br/>              / 
    /// <br/>             /
    /// <br/>            / first
    /// </code>
    /// </summary>
    /// <returns></returns>
    public List<TVertex> OnV2sExtension = new();

    public List<TVertex> GetMountsOnExtension(TVertex v) => v == V1 ? OnV1sExtension : OnV2sExtension;

    public TVertex V1 {get => (TVertex)Parts.First(); }
    public TVertex V2 {get => (TVertex)Parts.Last(); }

    public TVertex[] Vertices => new[] { V1, V2 };

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

    public override bool Equals(object? obj)
    {
        if (obj is not TSegment) return base.Equals(obj);
        else return Id.ToCharArray().ContainsMany(((TSegment)obj).Id.ToCharArray());
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}