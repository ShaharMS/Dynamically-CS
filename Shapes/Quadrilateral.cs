
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Shapes;

public class Quadrilateral : DraggableGraphic, IDismantable
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;
    public Joint joint4;

    QuadrilateralType _type = QuadrilateralType.QUADRILATERAL;

    public QuadrilateralType type
    {
        get => _type;
        set => ChangeType(value);
    }

    public Quadrilateral(Joint j1, Joint j2, Joint j3, Joint j4)
    {
        joint1 = j1;
        joint2 = j2;
        joint3 = j3;
        joint4 = j4;

        joint1.Connect(joint2, joint3);
        joint4.Connect(joint2, joint3);
    }

    public void Dismantle()
    {
        joint1.Disconnect(joint2, joint3);
        joint4.Disconnect(joint2, joint3);
    }


    QuadrilateralType ChangeType(QuadrilateralType type)
    {
        return type;
    }
}

public enum QuadrilateralType
{
    SQUARE,
    RECTANGLE,
    PARALLELOGRAM,
    RHOMBUS,
    TRAPEZOID,
    ISOCELES_TRAPESOID,
    KITE,
    QUADRILATERAL
}