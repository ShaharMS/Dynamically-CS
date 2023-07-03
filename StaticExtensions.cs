using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaticExtensions
{
    public static class StaticExtensions
    {
        public static double distanceTo(this Point from, Point to)
        {
            double x = from.X - to.X;
            double y = from.Y - to.Y;
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
    }
}
