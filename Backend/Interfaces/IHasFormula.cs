using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;

namespace Dynamically.Backend.Interfaces;

public interface IHasFormula<T> where T : Formula
{
    public T Formula { get; set; }
    public bool Contains(Vertex joint);
    public bool Contains(Segment segment);

    public bool HasMounted(Vertex joint);
    public bool HasMounted(Segment segment);
}
