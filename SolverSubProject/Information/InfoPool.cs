using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using SolverSubProject.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information;

public class InfoPool
{
    public QuestionData QuestionDiagram { get; set; } = new();


    public List<Detail> Given = new();

    public List<Detail> Objectives = new();

    public List<List<Detail>> Stages = new();

    public List<Detail> AvailableDetails => Given.Concat(Stages.Flatten()).ToList();

    public uint CurrentStage { get; set; }

    public HashSet<ExerciseToken> Elements = new();

    public InfoPool()
    {
        CurrentStage = 1;
    }

    public void Reset()
    {
        CurrentStage = 1;
        Given.Clear();
        Objectives.Clear();
        Stages.Clear();
        Elements.Clear();
    }

    public void AddGiven(Detail detail)
    {
        Given.Add(detail);

        AddElementsOf(detail);
    }

    public void AddObjective(Detail detail)
    {
        Objectives.Add(detail);

        AddElementsOf(detail);
    }

    public void AddDetail(Detail detail)
    {
        Stages[(int)CurrentStage - 1].Add(detail);

        AddElementsOf(detail);
    }

    public void AddElement(ExerciseToken token) => Elements.Add(token);
    
    private void AddElementsOf(Detail detail) =>
        Elements.UnionWith(new[] { detail.Left, detail.Right }.Concat(detail.SideProducts));

    public void NextStage()
    {
        CurrentStage++;
        Stages[(int)CurrentStage - 1] ??= new();
    }
}
