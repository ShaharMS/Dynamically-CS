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
    public List<TArc> Arcs = new();
    public List<TVertex> Relations
    {
        get => Segments.Select(s => s.Parts.First() == this ? s.Parts.Last() : s.Parts.First()).Cast<TVertex>().ToList();
    }

    public static TVertex Create(InfoPool pool) {
        var all = pool.Elements.Where(e => e is TVertex).Cast<TVertex>().Select(x => x.Id).ToList();

        char letter = Convert.ToChar(65);
        int sub = 0;
        string GetLetter() => letter + (sub > 0 ? $"_{sub}" : "");

        while (true) {
            for (letter = Convert.ToChar(65); letter < 91; letter++) {
                if (!all.Contains(GetLetter())) 
                return new TVertex {
                    Id = GetLetter(),
                    IsAuxiliary = true,
                    ParentPool = pool
                };
            }
            sub++;
        }
    }
}