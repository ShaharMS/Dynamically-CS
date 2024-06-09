using Dynamically.Backend.Interfaces;
using Dynamically.Menus.ContextMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Containers;

public partial class Board : IContextMenuSupporter<BoardContextMenuProvider>, IStringifyable
{
    public override string ToString()
    {
        return Name ?? "Board";
    }

    public string ToString(bool descriptive)
    {
        return descriptive ? "Board: " + ToString() : ToString();
    }
}
