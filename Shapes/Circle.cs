using Avalonia;
using Dynamically.Formulas;
using GeometryBackend;
using GraphicsBackend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;
public class Circle : EllipseBase
{
    public Joint center;

    public List<Action<double, double>> onResize = new List<Action<double, double>>();

    public double radius
    {
        get => distanceSum / 2;
        set
        {
            double prev = distanceSum / 2;
            distanceSum = value * 2;
            foreach (var l in onResize) l(value, prev);
        }
    }

    public CircleFormula formula;

    public Circle(Joint center, double radius) : base(center, center, radius * 2)
    {
        this.radius = radius;
        this.center = center;
        formula = new CircleFormula(radius, center.x, center.y);
        this.center.PropertyChanged += center_OnChange;
        onDistanceSumChange = updateFormula;
    }

    public void Set(Joint center, double radius)
    {
        this.center = center;
        this.radius = radius;
        updateFormula();
    }

    public void Set(Joint joint1, Joint joint2, Joint joint3) 
    {
        void FindIntersection(Point p1, Point p2, Point p3, Point p4, out bool segments_intersect, out Point intersection)
        {
            // Get the segments' parameters.
            double dx12 = p2.X - p1.X;
            double dy12 = p2.Y - p1.Y;
            double dx34 = p4.X - p3.X;
            double dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);

            double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            double t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0) t1 = 0;
            else if (t1 > 1) t1 = 1;

            if (t2 < 0) t2 = 0;
            else if (t2 > 1) t2 = 1;
        }

        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        double x1 = (joint2.x + joint1.x) / 2;
        double y1 = (joint2.y + joint1.y) / 2;
        double dy1 = joint2.x - joint1.x;
        double dx1 = -(joint2.y - joint1.y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        double x2 = (joint3.x + joint2.x) / 2;
        double y2 = (joint3.y + joint2.y) / 2;
        double dy2 = joint3.x - joint2.x;
        double dx2 = -(joint3.y - joint2.y);

        // See where the lines intersect.
        Point intersection;
        FindIntersection(new Point(x1, y1), new Point(x1 + dx1, y1 + dy1), new Point(x2, y2), new Point(x2 + dx2, y2 + dy2), out _, out intersection);

        var center = intersection;
        double dx = center.X - joint1.x;
        double dy = center.Y - joint1.y;
        var radius = Math.Sqrt(dx * dx + dy * dy);

        this.center = center;
        this.radius = radius;
        updateFormula();
    }

    public void updateFormula()
    {
        formula.centerX = center.x;
        formula.centerY = center.y;
        formula.radius = radius;
    }
    void center_OnChange(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        updateFormula();
    }
}

