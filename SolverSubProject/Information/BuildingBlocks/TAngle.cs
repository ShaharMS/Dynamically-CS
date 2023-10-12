using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dynamically.Backend;
using Dynamically.Solver.Interfaces;

namespace Dynamically.Solver.Information.BuildingBlocks;

public class TAngle : ExerciseToken, IConstructed
{
    public TVertex Origin;

    public TVertex Other1;
    public TVertex Other2;

    public TSegment? Segment1;
    public TSegment? Segment2;

    public List<ExerciseToken> Parts => new() { Origin, Other1, Other2};

    public TAngle(TVertex origin, TVertex other1, TVertex other2)
    {
        if (other1 == other2) throw new ArgumentException("other1 and other2 cannot be the same");

        Origin = origin;
        Other1 = other1;
        Other2 = other2;

        Segment1 = origin.Segments.Where(x => x.Parts.ContainsMany(origin, other1)).SingleOrDefault();
        Segment2 = origin.Segments.Where(x => x.Parts.ContainsMany(origin, other2)).SingleOrDefault();
    }
    public TAngle(TSegment segment1, TSegment segment2)
    {
        if (segment1 == segment2) throw new ArgumentException("segment1 and segment2 cannot be the same");

        Segment1 = segment1;
        Segment2 = segment2;

        Origin = (TVertex)segment1.Parts.Intersect(segment2.Parts).Single();
        Other1 = (TVertex)segment1.Parts.Except(segment2.Parts).First();
        Other2 = (TVertex)segment2.Parts.Except(segment1.Parts).Last();
    }
}
