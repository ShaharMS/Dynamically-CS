using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;

namespace Dynamically.Backend.Interfaces;

public interface ISupportsAdjacency
{
    public bool Overlaps(Point p);
}
