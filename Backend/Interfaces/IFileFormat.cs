﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Interfaces;

public interface IFileFormat
{
    public static uint MagicNumber;
    public static string Extension { get => ""; }
}
