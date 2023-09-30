using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend;

public static class StaticExtensions
{
    public static double DistanceTo(this Point from, Point to)
    {
        double x = from.X - to.X;
        double y = from.Y - to.Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this Joint from, Joint to)
    {
        double x = from.X - to.X;
        double y = from.Y - to.Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this Joint from, Point to)
    {
        double x = from.X - to.X;
        double y = from.Y - to.Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this Joint from, double X, double Y)
    {
        double x = from.X - X;
        double y = from.Y - Y;
        return Math.Sqrt(x * x + y * y);
    }
    public static double DistanceTo(this Point from, double X, double Y)
    {
        double x = from.X - X;
        double y = from.Y - Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this (double, double) from, (double, double) to)
    {
        double x = from.Item1 - to.Item1;
        double y = from.Item2 - to.Item2;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this (double, double) from, Point to)
    {
        double x = from.Item1 - to.X;
        double y = from.Item2 - to.Y;
        return Math.Sqrt(x * x + y * y);
    }
    public static double DistanceTo(this (double, double) from, Joint to)
    {
        double x = from.Item1 - to.X;
        double y = from.Item2 - to.Y;
        return Math.Sqrt(x * x + y * y);
    }
    public static double DistanceTo(this (double, double) from, double X, double Y)
    {
        double x = from.Item1 - X;
        double y = from.Item2 - Y;
        return Math.Sqrt(x * x + y * y);
    }

    public static double DistanceTo(this Formula from, Point p)
    {
        return from.GetClosestOnFormula(p) != null ? p.DistanceTo(from.GetClosestOnFormula(p) ?? new Point(double.NaN, double.NaN)) : double.NaN;
    }

    public static double DistanceTo(this Formula from, Joint j)
    {
        return DistanceTo(from, j);
    }
    public static double DistanceTo(this Formula from, double X, double Y)
    {
        return DistanceTo(from, new Point(X, Y));
    }
    public static double DegreesTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        if (angleInDegrees < 0) angleInDegrees += 360;
        return angleInDegrees;
    }

    public static double DegreesTo(this Joint from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        if (angleInDegrees < 0) angleInDegrees += 360;
        return angleInDegrees;
    }

    public static double RadiansTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        if (angleInRadians < 0) angleInRadians += Math.PI * 2;
        return angleInRadians;
    }

    public static double RadiansTo(this Joint from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        if (angleInRadians < 0) angleInRadians += Math.PI * 2;
        return angleInRadians;
    }
    public static double RadiansTo(this Point from, double x, double y) => RadiansTo(from, new Point(x, y));
    public static double RadiansTo(this Joint from, double x, double y) => RadiansTo(from, new Point(x, y));

    public static double Pow(this double b, double exponent)
    {
        return Math.Pow(b, exponent);
    }

    public static double Min(this double val, double val2) => Math.Min(val, val2);
    public static double Max(this double val, double val2) => Math.Max(val, val2);

    public static List<T> InsertR<T>(this List<T> en, int index, T value)
    {
        en.Insert(index, value);
        return en;
    }

    public static double RadiansBetween(this double radBA, double radBC, bool neg = false)
    {
        var a = (radBC - radBA);
        if (!neg)
        {
            if (a < 0) a += Math.PI * 2;
            if (a > Math.PI) a = Math.PI * 2 - a;
        }
        else
        {
            if (a < -Math.PI) a += Math.PI * 2;
            if (a > Math.PI) a -= Math.PI * 2;
        }
        return a;
    }

    public static double ToRadians(this double degs)
    {
        return degs * Math.PI / 180;
    }
    public static double ToDegrees(this double rads)
    {
        return rads * 180 / Math.PI;
    }

    public static bool IsSimilarTo(this double valueA, double valueB, double offsetUsingPercentage)
    {
        if (offsetUsingPercentage < 0 || offsetUsingPercentage > 1)
        {
            throw new ArgumentException("Percentage should be in the range 0 to 1.");
        }

        double ratio = valueA < valueB ? valueA / valueB : valueB / valueA;

        return ratio >= offsetUsingPercentage;
    }

    public static double GetSimilarityPercentage(this double valueA, double valueB)
    {
        double m = Math.Max(valueA, valueB);

        if (m == 0)
        {
            // Handle the case where both values are zero to avoid division by zero.
            return 1.0;
        }

        return valueA < valueB ? valueA / valueB : valueB / valueA;

    }

    public static bool RoughlyEquals(this Point p, Point o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this Joint p, Point o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this (double X, double Y) p, Point o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this Point p, Joint o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this Joint p, Joint o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this (double X, double Y) p, Joint o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    
    public static bool RoughlyEquals(this Point p, (double X, double Y) o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this Joint p, (double X, double Y) o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    public static bool RoughlyEquals(this (double X, double Y) p, (double X, double Y) o) => Math.Abs(p.X - o.X) < 0.0001 && Math.Abs(p.Y - o.Y) < 0.0001;
    
    public static bool RoughlyEquals(this Point p, double X, double Y) => Math.Abs(p.X - X) < 0.0001 && Math.Abs(p.Y - Y) < 0.0001;
    public static bool RoughlyEquals(this Joint p, double X, double Y) => Math.Abs(p.X - X) < 0.0001 && Math.Abs(p.Y - Y) < 0.0001;
    public static bool RoughlyEquals(this (double X, double Y) p, double X, double Y) => Math.Abs(p.X - X) < 0.0001 && Math.Abs(p.Y - Y) < 0.0001;

    public static bool RoughlyEquals(this double a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this double a, int b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this int a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this int a, int b) => Math.Abs(a - b) < 0.0001;

    public static Point GetPosition(this Control element)
    {
        Point position = new(-1, -1);

        if (element != null)
        {
            IVisual visual = element;
            IVisual rootVisual = visual.GetVisualRoot();

            if (rootVisual != null)
            {
#pragma warning disable CS8629 // Nullable value Type may be null.
                position = visual.TranslatePoint(new Point(0, 0), rootVisual).Value; // Must be non-null here
#pragma warning restore CS8629 // Nullable value Type may be null.
            }
        }

        return position;
    }

    public static void SetPosition(this Control element, double x, double y)
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
    }
}
