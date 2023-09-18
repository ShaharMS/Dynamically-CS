using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Interfaces;

public interface ISelectable
{
    public bool EncapsulatedWithin(Rect rect);
}
