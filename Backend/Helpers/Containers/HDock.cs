using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers.Containers;

public class HDock : DockPanel
{

    public List<Control> ChildrenQueued
    {
        get => new List<Control>();
        set
        {

            foreach (Control control in value)
            {
                SetDock(control, Dock.Left);
                Children.Add(control);
            }
        }
    }
    public HDock() : base()
    {
    }
}
