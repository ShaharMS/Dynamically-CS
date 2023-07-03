using GeometryBackend;
using GraphicsBackend;

namespace Dynamically.Shapes;

class Triangle : DraggableGraphic
{
    public Joint joint1;
    public Joint joint2;
    public Joint joint3;

    TriangleType _type = TriangleType.SCALENE;

    public TriangleType type
    {
        get => _type;
        set => ChangeType(value);
    }

    public Triangle(Joint j1, Joint j2, Joint j3)
    {
        joint1 = j1;
        joint2 = j2;
        joint3 = j3;

        joint1.Connect(joint2, joint3);
        joint2.Connect(joint3);
    }

    TriangleType ChangeType(TriangleType type) 
    { 
        return type; 
    }
}

enum TriangleType
{
    EQUILATERAL,
    ISOSCELES,
    RIGHT,
    SCALENE,
}