using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TVertex : ExerciseToken
{
    public List<TCircle> Circles = new();
    public List<TSegment> Segments = new();
    public List<TVertex> Relations
    {
        get => Segments.Select(s => s.Parts.First() == this ? s.Parts.Last() : s.Parts.First()).Cast<TVertex>().ToList();
    }
}