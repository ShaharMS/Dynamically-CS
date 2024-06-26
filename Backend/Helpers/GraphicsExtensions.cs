﻿using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

public static class GraphicsExtensions
{
    public static double GuessTextWidth(this Label label, string? text = null)
    {
        return (text ?? label.Content?.ToString())?.Length * label.FontSize * 0.533 ?? double.NaN;
    }
}
