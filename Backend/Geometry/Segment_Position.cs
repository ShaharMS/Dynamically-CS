using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Geometry;

public partial class Segment
{
    public double Length
    {
        get => Math.Sqrt(Math.Pow(joint2.X - joint1.X, 2) + Math.Pow(joint2.Y - joint1.Y, 2));
        set
        {
            var ray = new RayFormula(joint1, joint2);
            var p2Arr = ray.GetPointsByDistanceFrom(joint1, value);
            if (p2Arr[0].DistanceTo(joint2) < p2Arr[1].DistanceTo(joint2))
            {
                joint2.X = p2Arr[0].X;
                joint2.Y = p2Arr[0].Y;
            }
            else
            {
                joint2.X = p2Arr[1].X;
                joint2.Y = p2Arr[1].Y;
            }
        }
    }
    public void SetLength(double len, bool isFirstStuck = true)
    {
        if (isFirstStuck)
        {
            Length = len; // First is stuck by default
            return;
        }
        var ray = new RayFormula(joint1, joint2);
        var p1Arr = ray.GetPointsByDistanceFrom(joint2, len);
        if (p1Arr[0].DistanceTo(joint1) < p1Arr[1].DistanceTo(joint1))
        {
            joint1.X = p1Arr[0].X;
            joint1.Y = p1Arr[0].Y;
        }
        else
        {
            joint1.X = p1Arr[1].X;
            joint1.Y = p1Arr[1].Y;
        }
    }
}
