using Dynamically.Solver.Interfaces;
using Dynamically.Solver.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TCircle : ExerciseToken, IMountable
{
    public List<ExerciseToken> AllMounts { get; }

    public List<TVertex> Centers = new();

    public TCircle(string id)
    {
        AllMounts = new();
        Id = id;
    }

    public TCircle() { Id = ""; AllMounts = new(); }

    public TCircle(TVertex center)
    {
        AllMounts = new();
        Centers.Add(center);
        center.Circles.Add(this);
        Id = $"●{center.Id}{(center.Circles.Count > 0 ? $"_{center.Circles.Count}" : "")}";
    }

    public TCircle(params TVertex[] mounts)
    {
        AllMounts = new();
        AllMounts.AddRange(mounts);
        Id = "●" + mounts.Select(x => x.Id).Aggregate((a, b) => $"{a}{b}");
    }
}
