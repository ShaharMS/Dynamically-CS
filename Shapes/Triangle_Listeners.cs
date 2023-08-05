using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;
using Dynamically.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public partial class Triangle
{
    private Point EQ_temp_incircle_center = new Point(-1, -1);
    private void Equilateral_OnJointMove(Joint moved, Joint other1, Joint other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;
        var center = EQ_temp_incircle_center;
        var len = center.DistanceTo(moved);
        var angle = Math.Atan2(moved.Y - center.Y, moved.X - center.X);
        Log.Write(angle * 180 / Math.PI);
        foreach (var o in new[] { other1, other2 })
        {
            angle += 2 * Math.PI / 3;
            o.X = center.X + len * Math.Cos(angle);
            o.Y = center.Y + len * Math.Sin(angle);

        }
    }

    private Joint R_origin;
    private RayFormula R_O;
    private void Right_OnJointMove(Joint moved, Joint other1, Joint other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;

        if (moved == R_origin)
        {
            RayFormula optRay1 = new RayFormula(moved, other1), optRay2 = new RayFormula(moved, other2);

            var pos = new Point(px, py);
            var distOA = pos.DistanceTo(other1);
            var distOB = pos.DistanceTo(other2);

            if (other1.Anchored && other2.Anchored)
            {
                moved.X = px; moved.Y = py;
            } else if (other1.Anchored)
            {
                var ray = new RayFormula(pos, other1);
                var p = ray.GetClosestOnFormula(new Point(BigScreen.MouseX, BigScreen.MouseY));
                if (p != null)
                {
                    moved.X = p.Value.X; moved.Y = p.Value.Y;
                    other2.X += moved.X - px;
                    other2.Y += moved.Y - py;
                }
            } else if (other2.Anchored)
            {
                var ray = new RayFormula(pos, other2);
                var p = ray.GetClosestOnFormula(new Point(BigScreen.MouseX, BigScreen.MouseY));
                if (p != null)
                {
                    moved.X = p.Value.X; moved.Y = p.Value.Y;
                    other1.X += moved.X - px;
                    other1.Y += moved.Y - py;
                }
            } else
            {
                other1.X += moved.X - px; other1.Y += moved.Y - py;
                other2.X += moved.X - px; other2.Y += moved.Y - py;
            }

        }
        else
        {
            var other = other1 == R_origin ? other2 : other1;

            var radToMoved = R_origin.RadiansTo(moved);
            var radToOther = R_origin.RadiansTo(other);
            Log.Write(radToMoved.RadiansBetween(radToOther).ToDegrees(), radToMoved.ToDegrees(), radToOther.ToDegrees());
            var dist = other.DistanceTo(R_origin);
            if ((radToMoved + Math.PI / 2).RadiansBetween(radToOther) < (radToOther + Math.PI / 2).RadiansBetween(radToMoved))
            {
                if (other.Anchored)
                {
                    dist = moved.DistanceTo(R_origin);
                    radToOther -= Math.PI / 2;
                    moved.X = R_origin.X + dist * Math.Cos(radToOther);
                    moved.Y = R_origin.Y + dist * Math.Sin(radToOther);
                }
                else
                {
                    radToMoved += Math.PI / 2;
                    other.X = R_origin.X + dist * Math.Cos(radToMoved);
                    other.Y = R_origin.Y + dist * Math.Sin(radToMoved);
                }
            }
            else
            {
                if (other.Anchored)
                {
                    dist = moved.DistanceTo(R_origin);
                    radToOther += Math.PI / 2;
                    moved.X = R_origin.X + dist * Math.Cos(radToOther);
                    moved.Y = R_origin.Y + dist * Math.Sin(radToOther);
                }
                else
                {
                    radToMoved -= Math.PI / 2;
                    other.X = R_origin.X + dist * Math.Cos(radToMoved);
                    other.Y = R_origin.Y + dist * Math.Sin(radToMoved);
                }
            }

        }

    }

    private Joint ISO_origin;

    private void Isoceles_OnJointMove(Joint moved, Joint other1, Joint other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;

        if (moved == ISO_origin)
        {
            var ray = new RatioOnSegmentFormula(new SegmentFormula(other1, other2), 0.5).GetPerpendicular();

            var p = ray.GetClosestOnFormula(moved);
            if (p != null) {
                moved.X = p.Value.X; moved.Y = p.Value.Y;
            }
            return;
        }

        Joint j1 = other1, j2 = other2;
        if (j1 == ISO_origin) j1 = moved;
        else if (j2 == ISO_origin) j2 = moved;

        var dist = Math.Max(ISO_origin.DistanceTo(other1), ISO_origin.DistanceTo(other2));

        if (ISO_origin.DistanceTo(j1) != dist)
        {
            var radsToOther1 = Math.Atan2(j1.Y - ISO_origin.Y, j1.X - ISO_origin.X);
            j1.X = ISO_origin.X + dist * Math.Cos(radsToOther1);
            j1.Y = ISO_origin.Y + dist * Math.Sin(radsToOther1);
        }
        else
        {
            var radsToOther2 = Math.Atan2(j2.Y - ISO_origin.Y, j2.X - ISO_origin.X);
            j2.X = ISO_origin.X + dist * Math.Cos(radsToOther2);
            j2.Y = ISO_origin.Y + dist * Math.Sin(radsToOther2);
        }
    }
}
