using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dynamically.Backend;

public static class StaticExtensions
{
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

    public static bool ContainsMany<T>(this IEnumerable<T> en, params T[] items) => items.Any(en.Contains);
    public static IEnumerable<T> RemoveMany<T>(this IEnumerable<T> en, params T[] items) => en.Where(e => !items.Contains(e));

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> en) => en.SelectMany(e => e);

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

    public static bool RoughlyEquals(this double a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this double a, int b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this int a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlyEquals(this int a, int b) => Math.Abs(a - b) < 0.0001;

    public static bool RoughlyLarger(this double a, double b) => Math.Abs(a - b) > 0.0001;
    public static bool RoughlyLarger(this double a, int b) => Math.Abs(a - b) > 0.0001;
    public static bool RoughlyLarger(this int a, double b) => Math.Abs(a - b) > 0.0001;
    public static bool RoughlyLarger(this int a, int b) => Math.Abs(a - b) > 0.0001;

    public static bool RoughlySmaller(this double a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlySmaller(this double a, int b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlySmaller(this int a, double b) => Math.Abs(a - b) < 0.0001;
    public static bool RoughlySmaller(this int a, int b) => Math.Abs(a - b) < 0.0001;

    public static bool ContainsRoughly(this IEnumerable<double> en, double value) => en.Any(v => Math.Abs(v - value) < 0.0001);
    public static bool ContainsRoughly(this IEnumerable<int> en, double value) => en.Any(v => Math.Abs(v - value) < 0.0001);
    public static bool ContainsRoughly(this IEnumerable<int> en, int value) => en.Any(v => Math.Abs(v - value) < 0.0001);
    public static bool ContainsRoughly(this IEnumerable<double> en, int value) => en.Any(v => Math.Abs(v - value) < 0.0001);
    public static string PrettifyJson(this string unPrettyJson)
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(unPrettyJson);

        return JsonSerializer.Serialize(jsonElement, options);
    }
}