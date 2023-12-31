using Dynamically.Formulas;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Segment
{
    public double Degrees
    {
        get
        {
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

            double px = Vertex2.X, py = Vertex2.Y;
            Vertex2.X = Vertex2.X + value * Math.Cos(rads);
            Vertex2.Y = Vertex2.Y + value * Math.Sin(rads);

            Vertex2.DispatchOnMovedEvents(px, py);
        }
    }
    public void SetLength(double len, bool isFirstStuck = true)
    {
        if (isFirstStuck)
        {
            Length = len; // First is stuck by default
            return;
        }
        else
        {
            var rads = Vertex2.RadiansTo(Vertex1);

            double px = Vertex1.X, py = Vertex1.Y;
            Vertex1.X = Vertex1.X + len * Math.Cos(rads);
            Vertex1.Y = Vertex1.Y + len * Math.Sin(rads);

            Vertex1.DispatchOnMovedEvents(px, py);
        }
    }
}
