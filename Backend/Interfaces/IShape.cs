using Dynamically.Backend.Geometry;
using Dynamically.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Interfaces;

public interface IShape
{
    public bool Contains(Joint joint);
    public bool Contains(Segment segment);

    public bool HasMounted(Joint joint);
    public bool HasMounted(Segment segment);
}
