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

    /// <summary>
    /// Other elements this element is mounted on
    /// </summary>
    public List<IMountable> IsOn = new();

    /// <summary>
    /// Contains elements that use this element to construct themself
    /// </summary>
    public List<IConstructed> PartOf = new();

    public bool IsAuxilarry { get; private set; }

}