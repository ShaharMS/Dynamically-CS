using Dynamically.Solver.Information;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Structs;

public struct SVertexOnCircleAtAngle
{
    public TVertex Vertex;

    public TCircle Circle;

    public double Degrees;
    public double Radians;

    public SVertexOnCircleAtAngle(TVertex v, TCircle c, double? r, double? d)
    {
        Vertex = v;
        Circle = c;
        Radians = r ?? (d ?? throw new ArgumentException("Radians/Degrees must be given. both were null")) * Math.PI / 180;
        Degrees = Radians * 180 / Math.PI;
    }
}
