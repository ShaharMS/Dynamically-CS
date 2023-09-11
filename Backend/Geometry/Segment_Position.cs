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
            var val = joint1.DegreesTo(joint2);
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
        get => Math.Sqrt(Math.Pow(joint2.X - joint1.X, 2) + Math.Pow(joint2.Y - joint1.Y, 2));
        set
        {
            var rads = joint1.RadiansTo(joint2);

            joint2.X = joint1.X + value * Math.Cos(rads);
            joint2.Y = joint1.Y + value * Math.Sin(rads);
        }
    }
    public void SetLength(double len, bool isFirstStuck = true)
    {
        if (isFirstStuck)
        {
            Length = len; // First is stuck by default
            return;
        }
        var rads = joint2.RadiansTo(joint1);

        joint1.X = joint2.X + len * Math.Cos(rads);
        joint1.Y = joint2.Y + len * Math.Sin(rads);
    }
}
