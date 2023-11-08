using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Segment
{
    public double Degrees {
        get {
            var val = Vertex1.DegreesTo(Vertex2);
            if (val < 0) val += 180;
            if (val > 180) val -= 180;
            return val;
        }
    }

    public double Radians
    {
        get => Degrees.ToRadians();
    }
    public double Length
    {
        get => Math.Sqrt(Math.Pow(Vertex2.X - Vertex1.X, 2) + Math.Pow(Vertex2.Y - Vertex1.Y, 2));
        set
        {
            var rads = Vertex1.RadiansTo(Vertex2);

            Vertex2.X = Vertex1.X + value * Math.Cos(rads);
            Vertex2.Y = Vertex1.Y + value * Math.Sin(rads);
        }
    }
    public void SetLength(double len, bool isFirstStuck = true)
    {
        if (isFirstStuck)
        {
            Length = len; // First is stuck by default
            return;
        }
        var rads = Vertex2.RadiansTo(Vertex1);

        Vertex1.X = Vertex2.X + len * Math.Cos(rads);
        Vertex1.Y = Vertex2.Y + len * Math.Sin(rads);
    }
}
