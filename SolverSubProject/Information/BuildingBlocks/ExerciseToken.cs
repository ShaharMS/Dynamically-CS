using Dynamically.Solver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Information.BuildingBlocks;


public class ExerciseToken
{
    InfoPool _inf = null!;
    /// <summary>
    /// References the exercise's InfoPool instance, and thereby its Solver too.
    /// </summary>
    /// <value></value>
    public InfoPool ParentPool
    {
        get => _inf; 
        set
        {
            _inf = value;
            _inf.AddElement(this);
        }
    }
    public string Id = "";

    /// <summary>
    /// Other elements this element is mounted on
    /// </summary>
    public List<IMountable> IsOn = new();

    /// <summary>
    /// Contains elements that use this element to construct themself
    /// </summary>
    public List<IConstructed> PartOf = new();

    public bool IsAuxiliary { get; set; }

}