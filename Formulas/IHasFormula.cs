using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas;

public interface IHasFormula
{
    public Formula Formula { get; set; }
}
