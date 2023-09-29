using Dynamically.Solver.Information;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Interfaces;

public interface IConstructed 
{
    List<ExerciseToken> Parts { get; }
}
