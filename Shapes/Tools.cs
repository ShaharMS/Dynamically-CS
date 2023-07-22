using System;
using System.Collections.Generic;
using Avalonia;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
namespace Dynamically.Shapes;

class Tools
{
    public static Circle CircleFrom3Joints(Joint joint1, Joint joint2, Joint joint3)
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
        }

        // Get the perpendicular bisector of (x1, y1) and (x2, y2).
        double x1 = (joint2.X + joint1.X) / 2;
        double y1 = (joint2.Y + joint1.Y) / 2;
        double dy1 = joint2.X - joint1.X;
        double dx1 = -(joint2.Y - joint1.Y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        double x2 = (joint3.X + joint2.X) / 2;
        double y2 = (joint3.Y + joint2.Y) / 2;
        double dy2 = joint3.X - joint2.X;
        double dx2 = -(joint3.Y - joint2.Y);

        // See where the lines intersect.
        Point intersection;
        FindIntersection(new Point(x1, y1), new Point(x1 + dx1, y1 + dy1), new Point(x2, y2), new Point(x2 + dx2, y2 + dy2), out _, out intersection);

        var center = intersection;
        double dx = center.X - joint1.X;
        double dy = center.Y - joint1.Y;
        var radius = Math.Sqrt(dx * dx + dy * dy);
        var c = new Joint(center.X, center.Y);
        Circle circle = new Circle(c , radius);
        c.Roles.AddToRole(Role.CIRCLE_Center, circle);
        foreach (var joint in new[] { joint1, joint2, joint3 })
        {
            joint.Roles.AddToRole(Role.CIRCLE_On, circle);
        }

        return circle;
    }

    public static Circle CircleFrom3Points(Point joint1, Point joint2, Point joint3)
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
        double x1 = (joint2.X + joint1.X) / 2;
        double y1 = (joint2.Y + joint1.Y) / 2;
        double dy1 = joint2.X - joint1.X;
        double dx1 = -(joint2.Y - joint1.Y);

        // Get the perpendicular bisector of (x2, y2) and (x3, y3).
        double x2 = (joint3.X + joint2.X) / 2;
        double y2 = (joint3.Y + joint2.Y) / 2;
        double dy2 = joint3.X - joint2.X;
        double dx2 = -(joint3.Y - joint2.Y);

        // See where the lines intersect.
        Point intersection;
        FindIntersection(new Point(x1, y1), new Point(x1 + dx1, y1 + dy1), new Point(x2, y2), new Point(x2 + dx2, y2 + dy2), out _, out intersection);

        var center = intersection;
        double dx = center.X - joint1.X;
        double dy = center.Y - joint1.Y;
        var radius = Math.Sqrt(dx * dx + dy * dy);

        var c = new Joint(center.X, center.Y);
        Circle circle = new Circle(c, radius);
        c.Roles.AddToRole(Role.CIRCLE_Center, circle);

        return circle;
    }

    public static double GetDegreesBetween3Points(Point p1, Point center, Point p2) 
    {
        double angle1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X);
        double angle2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X);

        double angle = (angle2 - angle1) * (180.0 / Math.PI);
        if (angle < 0) angle += 360;

        return angle;
    }

    public static double GetDegreesBetweenConnections(Connection p1p2, Connection p1p3)
    {
        double angle1 = Math.Atan2(p1p2.joint2.Y - p1p2.joint1.Y, p1p2.joint2.X - p1p2.joint1.X);
        double angle2 = Math.Atan2(p1p3.joint2.Y - p1p3.joint1.Y, p1p3.joint2.X - p1p3.joint1.X);

        double angle = (angle2 - angle1) * (180.0 / Math.PI);
        if (angle < 0) angle += 360;

        return angle;
    }

    public static double GetRadiansBetween3Points(Point p1, Point center, Point p2)
    {
        double angle1 = Math.Atan2(p1.Y - center.Y, p1.X - center.X);
        double angle2 = Math.Atan2(p2.Y - center.Y, p2.X - center.X);

        double angle = (angle2 - angle1);
        if (angle < 0) angle += Math.PI * 2;

        return angle;
    }

    public static double GetRadiansBetweenConnections(Connection p1p2, Connection p1p3)
    {
        double angle1 = Math.Atan2(p1p2.joint2.Y - p1p2.joint1.Y, p1p2.joint2.X - p1p2.joint1.X);
        double angle2 = Math.Atan2(p1p3.joint2.Y - p1p3.joint1.Y, p1p3.joint2.X - p1p3.joint1.X);

        double angle = (angle2 - angle1);
        if (angle < 0) angle += Math.PI;

        return angle;
    }
}
