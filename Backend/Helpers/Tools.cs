using System;
using System.Collections.Generic;
using Avalonia;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend;
using Dynamically.Shapes;
using System.Diagnostics;

namespace Dynamically.Backend.Helpers;

class Tools
{
    static void FindIntersection(Point p1, Point p2, Point p3, Point p4, out bool segments_intersect, out Point intersection)
    {
        // Get the segments' parameters.
        double dx12 = p2.X - p1.X;
        double dy12 = p2.Y - p1.Y;
        double dx34 = p4.X - p3.X;
        double dy34 = p4.Y - p3.Y;

        // Solve for b1 and b2
        double denominator = (dy12 * dx34 - dx12 * dy34);

        double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
        double t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

        // Find the point of intersection.
        intersection = new Point(p1.X + dx12 * t1, p1.Y + dy12 * t1);

        // The segments intersect if b1 and b2 are between 0 and 1.
        segments_intersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
    }

    public static Circle CircleFrom3Joints(Vertex joint1, Vertex joint2, Vertex joint3)
    {


        // Get the perpendicular bisector of (X1, y1) and (x2, y2).
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
        var c = new Vertex(joint1.ParentBoard, center.X, center.Y);
        Circle circle = new(c, radius);
        c.Roles.AddToRole(Role.CIRCLE_Center, circle);
        foreach (var joint in new[] { joint1, joint2, joint3 })
        {
            joint.Roles.AddToRole(Role.CIRCLE_On, circle);
        }

        return circle;
    }

    public static Circle CircleFrom3Points(Point joint1, Point joint2, Point joint3)
    {

        // Get the perpendicular bisector of (X1, y1) and (x2, y2).
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

        var c = new Vertex(MainWindow.Instance.WindowTabs.CurrentBoard, center.X, center.Y);
        Circle circle = new(c, radius);

        return circle;
    }

    public static double GetDegreesBetween3Points(Point p1, Point center, Point p2, bool canGetLarge = false)
    {
        double angle1 = center.DegreesTo(p1);
        double angle2 = center.DegreesTo(p2);

        double angle = angle2 - angle1;
        if (angle < 0) return -angle;
        if (!canGetLarge && angle > 180) return 360 - angle;

        return angle;
    }

    public static double GetDegreesBetweenConnections(Segment s1, Segment s2, bool canGetLarge = false)
    {
        var common = s1.GetSharedJoint(s2);
        if (common == null) return double.NaN;
        var others = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s2.Vertex1, s2.Vertex2 };
        others.Remove(common);
        Vertex other1 = others.First(), other2 = others.Last();

        double angle1 = Math.Atan2(other1.Y - common.Y, other1.X - common.X);
        double angle2 = Math.Atan2(other2.Y - common.Y, other2.X - common.X);

        double angle = (angle2 - angle1) * (180.0 / Math.PI);
        if (angle < 0) angle = -angle;
        if (!canGetLarge && angle > 180) return 360 - angle;

        return angle;
    }

    public static double GetRadiansBetween3Points(Point p1, Point center, Point p2, bool canGetLarge = false)
    {
        double angle1 = center.RadiansTo(p1);
        double angle2 = center.RadiansTo(p2);

        double angle = angle2 - angle1;
        if (angle < 0) angle = -angle;
        if (!canGetLarge && angle > Math.PI) return Math.PI * 2 - angle;

        return angle;
    }

    public static double GetRadiansBetweenConnections(Segment s1, Segment s2, bool canGetLarge = false)
    {
        var common = s1.GetSharedJoint(s2);
        if (common == null) return double.NaN;
        var others = new HashSet<Vertex> { s1.Vertex1, s1.Vertex2, s2.Vertex1, s2.Vertex2 };
        others.Remove(common);
        Vertex other1 = others.First(), other2 = others.Last();

        double angle1 = Math.Atan2(other1.Y - common.Y, other1.X - common.X);
        double angle2 = Math.Atan2(other2.Y - common.Y, other2.X - common.X);

        double angle = (angle2 - angle1);
        if (angle < 0) angle = -angle;
        if (!canGetLarge && angle > Math.PI) return Math.PI * 2 - angle;

        return angle;
    }

    public static bool QualifiesForMerge(Vertex j1, Vertex j2)
    {
        // Case 1: CIRCLE_center & CIRCLE_on
        var a1 = j1.Roles.Access<Circle>(Role.CIRCLE_Center);
        var a2 = j2.Roles.Access<Circle>(Role.CIRCLE_On);
        if (a1.Intersect(a2).Any()) return false;
        a1 = j1.Roles.Access<Circle>(Role.CIRCLE_On);
        a2 = j2.Roles.Access<Circle>(Role.CIRCLE_Center);
        if (a1.Intersect(a2).Any()) return false;

        // Case 2: Circum & Incircle
        var e1 = j1.Roles.Access<Triangle>(Role.TRIANGLE_CircumCircleCenter);
        var e2 = j2.Roles.Access<Triangle>(Role.TRIANGLE_InCircleCenter);
        if (e1.Intersect(e2).Any()) return false;
        e1 = j2.Roles.Access<Triangle>(Role.TRIANGLE_CircumCircleCenter);
        e2 = j1.Roles.Access<Triangle>(Role.TRIANGLE_InCircleCenter);
        if (e1.Intersect(e2).Any()) return false;

        // Case 3: corners of the same Triangle
        var b1 = j1.Roles.Access<Triangle>(Role.TRIANGLE_Corner);
        var b2 = j2.Roles.Access<Triangle>(Role.TRIANGLE_Corner);
        if (b1.Intersect(b2).Any()) return false;

        // Case 4: Triangle corner & in\Circumcircle Center
        var c1 = j1.Roles.Access<Circle>(Role.CIRCLE_Center);
        var c2 = j2.Roles.Access<Triangle>(Role.TRIANGLE_Corner).Select(t => new[] { t.Incircle, t.Circumcircle }).SelectMany(item => item).Where(c => c != null).Cast<Circle>();
        if (c1.Intersect(c2).Any()) return false;
        c1 = j2.Roles.Access<Circle>(Role.CIRCLE_Center);
        c2 = j1.Roles.Access<Triangle>(Role.TRIANGLE_Corner).Select(t => new[] { t.Incircle, t.Circumcircle }).SelectMany(item => item).Where(c => c != null).Cast<Circle>();
        if (c1.Intersect(c2).Any()) return false;

        // Case 5: Triangle corner & in/Circumcircle on

        var d1 = j1.Roles.Access<Triangle>(Role.TRIANGLE_Corner).Select(t => new[] { t.Incircle, t.Circumcircle }).SelectMany(item => item).Where(c => c != null).Cast<Circle>().Select(c => c.Formula.Followers).SelectMany(item => item);
        if (d1.Contains(j2)) return false;
        var d2 = j2.Roles.Access<Triangle>(Role.TRIANGLE_Corner).Select(t => new[] { t.Incircle, t.Circumcircle }).SelectMany(item => item).Where(c => c != null).Cast<Circle>().Select(c => c.Formula.Followers).SelectMany(item => item);
        if (d2.Contains(j1)) return false;

        return true;
    }

    public static bool QualifiesForMount(Vertex j, dynamic shape)
    {
        if (shape is Circle circle)
        {
            // Case C1: Center & its circle
            var a1 = j.Roles.Access<Circle>(Role.CIRCLE_Center);
            if (a1.Contains(circle)) return false;

            // Case C2: Triangle corner & Incircle
            var b1 = j.Roles.Access<Triangle>(Role.TRIANGLE_Corner).Select(t => t.Incircle).Where(item => item != null).Cast<Circle>();
            if (b1.Contains(circle)) return false;
        }

        if (shape is Segment segment)
        {
            // Case S1: Center & circle chord (stack overflow)
            var a1 = j.Roles.Access<Circle>(Role.CIRCLE_Center);
            var a2 = segment.Roles.Access<Circle>(Role.CIRCLE_Chord);
            if (a1.Intersect(a2).Any()) return false;

            // Case S2: Triangle corner & side
            var b1 = j.Roles.Access<Triangle>(Role.TRIANGLE_Corner);
            var b2 = segment.Roles.Access<Triangle>(Role.TRIANGLE_Side);
            if (b1.Intersect(b2).Any()) return false;
        }

        return true;
    }

    public static bool QualifiesForStraighten(Vertex v1, Vertex v2, Vertex common)
    {
        // Case 1: 2 radii -> diameter
        foreach (Circle circle in common.Roles.Access<Circle>(Role.CIRCLE_Center))
        {
            if (v1.Roles.Has(Role.CIRCLE_On, circle) && v2.Roles.Has(Role.CIRCLE_On, circle)) return false;
        }

        return true;
    }

    static double NormalizeAngle(double angle)
    {
        while (angle < 0)
        {
            angle += 2 * Math.PI;
        }
        while (angle >= 2 * Math.PI)
        {
            angle -= 2 * Math.PI;
        }
        return angle;
    }

    public static double[] OrderRadiansBySmallArc(double rad1, double rad2)
    {
        // Normalize angles to be between 0 and 2 * PI
        rad1 = NormalizeAngle(rad1);
        rad2 = NormalizeAngle(rad2);

        if (Math.Min(rad1, rad2) + Math.PI >= Math.Max(rad1, rad2)) return new double[] { Math.Min(rad1, rad2), Math.Max(rad1, rad2) };
        else return new double[] { Math.Max(rad1, rad2), Math.Min(rad1, rad2) };
    }
}
