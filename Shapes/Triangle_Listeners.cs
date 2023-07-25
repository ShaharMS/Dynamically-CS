using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Formulas;
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
    private void Right_OnJointMove(Joint moved, Joint other1, Joint other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;

        if (moved == R_origin)
        {
            other1.X += moved.X - px;
            other1.Y += moved.Y - py;
            other2.X += moved.X - px;
            other2.Y += moved.Y - py;
            other1.DispatchOnMovedEvents(other1.X, other1.Y, other1.X, other1.Y);
            other2.DispatchOnMovedEvents(other2.X, other2.Y, other2.X, other2.Y);
        }
        else
        {
            var other = other1 == R_origin ? other2 : other1;
            var radToOrigin = Math.Atan2(moved.Y - R_origin.Y, moved.X - R_origin.X);
            var radToOther = Math.Atan2(other.Y - R_origin.Y, other.X - R_origin.X);
            var dist = other.DistanceTo(R_origin);
            if (radToOrigin > radToOther)
            {
                radToOrigin -= Math.PI / 2;
                other.X = R_origin.X + dist * Math.Cos(radToOrigin);
                other.Y = R_origin.Y + dist * Math.Sin(radToOrigin);
                other.DispatchOnMovedEvents(other.X, other.Y, other.X, other.Y);
            }
            else
            {
                radToOrigin += Math.PI / 2;
                other.X = R_origin.X + dist * Math.Cos(radToOrigin);
                other.Y = R_origin.Y + dist * Math.Sin(radToOrigin);
                other.DispatchOnMovedEvents(other.X, other.Y, other.X, other.Y);
            }

        }

    }

    private Joint ISO_origin;

    private void Isoceles_OnJointMove(Joint moved, Joint other1, Joint other2, double px, double py)
    {
        if (moved.X == px && moved.Y == py) return;
        Joint j1 = other1, j2 = other2;
        if (j1 == ISO_origin) j1 = moved;
        else if (j2 == ISO_origin) j2 = moved;

        var dist = Math.Max(ISO_origin.DistanceTo(other1), ISO_origin.DistanceTo(other2));

        if (ISO_origin.DistanceTo(j1) != dist)
        {
            var radsToOther1 = Math.Atan2(j1.Y - ISO_origin.Y, j1.X - ISO_origin.X);
            j1.X = ISO_origin.X + dist * Math.Cos(radsToOther1);
            j1.Y = ISO_origin.Y + dist * Math.Sin(radsToOther1);
            j1.DispatchOnMovedEvents(j1.X, j1.Y, j1.X, j1.Y);

        }
        else
        {
            var radsToOther2 = Math.Atan2(j2.Y - ISO_origin.Y, j2.X - ISO_origin.X);
            j2.X = ISO_origin.X + dist * Math.Cos(radsToOther2);
            j2.Y = ISO_origin.Y + dist * Math.Sin(radsToOther2);
            j2.DispatchOnMovedEvents(j2.X, j2.Y, j2.X, j2.Y);
        }
    }
}
