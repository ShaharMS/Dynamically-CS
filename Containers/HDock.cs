using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Containers;

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
        Children.CollectionChanged += (_, _) =>
        {
            foreach (Control control in Children.Cast<Control>())
            {
                SetDock(control, Dock.Left);
            }
        };
    }
}
