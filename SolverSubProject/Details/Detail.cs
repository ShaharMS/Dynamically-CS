using Dynamically.Solver.Information.BuildingBlocks;
using SolverSubProject.Information;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Details;

public class Detail
{
    public Reason Reason { get; set; }

    /// <summary>
    /// Details this detail references. You should be able to traverse this to get the full solution's path
    /// </summary>
    public List<Detail> References { get; set; } = new();

    /// <summary>
    /// Other details that referenced this
    /// </summary>
    public List<Detail> ReferencedBy { get; set; } = new();

    public ExerciseToken Left { get; private set; }

    public Relation Operator { get; private set; }

    public ExerciseToken Right { get; private set; }

    public bool DefinesAuxiliary { get; set; } = false;

    public ExerciseToken[] Operands => new[] {Left, Right};
    public IEnumerable<ExerciseToken> IncludedElements => new[] {Left, Right}.Concat(SideProducts);

    public List<ExerciseToken> SideProducts = new();

    /// <summary>
    /// A detail with side products, for example: intersection (AB intersects CD at E), bisection...
    /// </summary>
    /// <param name="left"></param>
    /// <param name="op"></param>
    /// <param name="right"></param>
    /// <param name="sideProducts"></param>
    public Detail(ExerciseToken left, Relation op, ExerciseToken right, params ExerciseToken[] sideProducts)
    {
        Operator = op;
        Left = left;
        Right = right;
        SideProducts = sideProducts.ToList();
    }

    /// <summary>
    /// A detail without side products
    /// </summary>
    /// <param name="left"></param>
    /// <param name="op"></param>
    /// <param name="right"></param>
    public Detail(ExerciseToken left, Relation op, ExerciseToken right) 
    { 
        Operator = op; 
        Left = left; 
        Right = right;
    }

    /// <summary>
    /// Used for declaring exitance of certain details
    /// </summary>
    /// <param name="left"></param>
    /// <param name="op"></param>
    public Detail(ExerciseToken left, Relation op)
    {
        Operator = op;
        Left = left;

        Right = new ExerciseToken(); // Non-extended ExerciseToken is equivalent to null
    }

}
