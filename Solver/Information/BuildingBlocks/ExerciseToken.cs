using Dynamically.Solver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;


public class ExerciseToken
{
    public string Id = "";

    public List<IMountable> On = new();

    public List<IConstructed> PartOf = new();

    public bool IsAuxilarry { get; private set; }

}