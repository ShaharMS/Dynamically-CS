using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Menus.ContextMenus;

public interface IContextMenuSupporter
{
    public Dictionary<Role, List<DraggableGraphic>> PartOf { get; set; }
}
