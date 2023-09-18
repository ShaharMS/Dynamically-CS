using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Interfaces;

public interface IContextMenuSupporter<T> where T : ContextMenuProvider
{
    T Provider { get; }
}
