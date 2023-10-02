using Dynamically.Solver.Details;
using Dynamically.Solver.Information;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverSubProject;

public class Solver
{
    public static Solver Instance { get; } = new Solver(new InfoPool());

    public InfoPool InfoPool { get; }

    public SolverMode SolverMode { get; set; }

    public Solver(InfoPool pool)
    {
        InfoPool = pool;
    }


    /// <summary>
    /// Extracts as many details as possible from the already existing information.
    /// 
    /// Does not try to solve the question, but might accidentally end up solving it.
    /// </summary>
    public void StartGenericWave()
    {
        HashSet<Detail> wave = new();
        foreach (var element in InfoPool.Elements.ToList())
        {
            if (element is TVertex vertex)
            {

            }
            else if (element is TSegment segment)
            {

            }
            else if (element is TCircle circle)
            {

            }
            else if (element is TArc arc)
            {

            } 
            else if (element is TAngle line)
            {

            }
        }

        foreach (var detail in wave) InfoPool.AddDetail(detail);

        InfoPool.NextStage();
    }
}

public enum SolverMode
{
    STRICT,
    LOOSE,
    BAGRUT
}