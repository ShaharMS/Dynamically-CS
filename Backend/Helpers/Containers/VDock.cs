﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers.Containers;

public class VDock : DockPanel
{
    public List<Control> ChildrenQueued
    {
        get => new List<Control>();
        set
        {
            foreach (Control control in value)
            {
                SetDock(control, Dock.Top);
                Children.Add(control);
            }
        }
    }
    public VDock() : base() { }
}