using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Formulas.Special;

public class IsocelesTriangleFormula : ShapeTypeFormula
{

    public Triangle Triangle;

    public Joint ISO_origin;

    public IsocelesTriangleFormula(Triangle subject)
    {
        Triangle = subject;
        Current = Triangle.joint1;
    }

    public override (double X, double Y) GetPositionForCurrent(double x, double y)
    {
        throw new NotImplementedException();
    }
}
