using Avalonia;
using GeometryBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticExtensions;

public static class StaticExtensions
{
    public static double distanceTo(this Point from, Point to)
    {
        double x = from.X - to.X;
        double y = from.Y - to.Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double distanceTo(this Joint from, Joint to)
    {
        double x = from.x - to.x;
        double y = from.y - to.y;
        return Math.Sqrt(x * x + y * y);
    }
    public static double degreesTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        return angleInDegrees;
    }

    public static double radiansTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        return angleInRadians;
    }

    public static double Pow(this double b, double exponent)
    {
        return Math.Pow(b, exponent);
    }
}
