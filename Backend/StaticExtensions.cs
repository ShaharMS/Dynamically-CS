﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dynamically.Backend.Geometry;
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
    public static double DegreesTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        return angleInDegrees;
    }

    public static double RadiansTo(this Point from, Point to)
    {
        double angleInRadians = Math.Atan2(to.Y - from.Y, to.X - from.X);
        return angleInRadians;
    }

    public static double Pow(this double b, double exponent)
    {
        return Math.Pow(b, exponent);
    }


    public static Point GetPosition(this Control element)
    {
        Point position = new Point(0, 0);

        if (element != null)
        {
            IVisual visual = (IVisual)element;
            IVisual rootVisual = visual.GetVisualRoot();

            if (rootVisual != null)
            {
#pragma warning disable CS8629 // Nullable value type may be null.
                position = visual.TranslatePoint(new Point(0, 0), rootVisual).Value; // Must be non-null here
#pragma warning restore CS8629 // Nullable value type may be null.
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