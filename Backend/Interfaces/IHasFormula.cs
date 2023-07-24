using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Formulas;

namespace Dynamically.Backend.Interfaces;

public interface IHasFormula
{
    public Formula Formula { get; set; }
}
