using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Dynamically.Backend.Graphics;

public class SolutionTable : DataGrid
{
    public bool hasFroms;
    public SolutionTable(bool hasFroms = false) : base()
    {
        this.hasFroms = hasFroms;

        CanUserResizeColumns = true;
        CanUserSortColumns = false;
        CanUserReorderColumns = false;
        
    }

    
}
