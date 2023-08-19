using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;

namespace Dynamically.Formulas.Special;

public class EquilateralTriangleFormula
{
    public Triangle triangle;

    public Joint? Current;

    public (double X, double Y) GetPositionForCurrent(double z, double c) {
        _ = z; _ = c;
        return (0, 0);
    }
}
