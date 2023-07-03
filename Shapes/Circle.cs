using GeometryBackend;
using GraphicsBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;
class Circle : EllipseBase
{
    public Joint center;

    public double radius;

    public Circle(Joint center, double radius) : base(center, center, radius * 2)
    {
        this.radius = radius;
        this.center = center;
    }
}

