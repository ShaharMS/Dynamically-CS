﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public partial class DraggableGraphic
{
    public bool IsHovered { get; set; }
    public bool IsPressed { get; set; }
    public new bool IsFocused
    {
        get => MainWindow.Instance.MainBoard.FocusedObject == this;
        set => MainWindow.Instance.MainBoard.FocusedObject = this;
    }

    public DraggableGraphic()
    {
        Draggable = true;
        PointerEnter += MouseInfo_PointerHover;
        PointerLeave += MouseInfo_PointerOut;
        PointerPressed += MouseInfo_PointerPressed;
        PointerReleased += MouseInfo_PointerReleased;
        // IsFocused handled in MainBoard
    }

    private void MouseInfo_PointerHover(object? sender, PointerEventArgs e)
    {
        IsHovered = true;
    }
    private void MouseInfo_PointerOut(object? sender, PointerEventArgs e)
    {
        IsHovered = false;
    }
    private void MouseInfo_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IsPressed = true;
    }
    private void MouseInfo_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsPressed = false;
    }
}
