﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AngouriMath.Entity;

namespace Dynamically.Backend.Graphics;

/// <summary>
/// Ported from <see href="github.com/ShaharMS/texter">my Haxe text utils library</see> 
/// </summary>
public partial class ResizableBorder : Border
{
    public virtual bool Resizable { get; set; }
    public bool CurrentlyResizing;

    public List<Action> OnResizeStart = new();
    public List<Action> OnResizing = new();
    public List<Action<double, double, double, double, double, double, double, double>> OnResizeEnd = new();


    public Cursor MouseOverHor = new(StandardCursorType.SizeWestEast);
    public Cursor MouseOverVer = new(StandardCursorType.SizeNorthSouth);
    public Cursor MouseOverToBottomRight = new(StandardCursorType.BottomRightCorner);
    public Cursor MouseOverToBottomLeft = new(StandardCursorType.BottomLeftCorner);
    public Cursor MouseOverDisabledCursor = new(StandardCursorType.Arrow);

    double tX;
	double tY;
	double tWidth;
	double tHeight;

    double cornerDistance = 5;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Log.Write("Attempting resize", Resizable);
        if (Resizable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            CurrentlyResizing = true;
            e.Pointer.Capture(this);

            var mousePoint = e.GetPosition(Parent);
            //Log.Write(mousePoint.ToString(), Bounds.ToString());
            //Log.Write("Bounds:", "tl", Bounds.TopLeft, "tr", Bounds.TopRight, "bl", Bounds.BottomLeft, "br", Bounds.BottomRight);

            foreach (var l in OnResizeStart) l();

            if (Bounds.TopLeft.DistanceTo(mousePoint) < cornerDistance) startResizeTopLeft(e);
            else if (Bounds.TopRight.DistanceTo(mousePoint) < cornerDistance) startResizeTopRight(e);
            else if (Bounds.BottomLeft.DistanceTo(mousePoint) < cornerDistance) startResizeBottomLeft(e);
            else if (Bounds.BottomRight.DistanceTo(mousePoint) < cornerDistance) startResizeBottomRight(e);
            else if (Math.Abs(Bounds.Top - mousePoint.Y) < cornerDistance) startResizeTop(e);
            else if (Math.Abs(Bounds.Bottom - mousePoint.Y) < cornerDistance) startResizeBottom(e);
            else if (Math.Abs(Bounds.Left - mousePoint.X) < cornerDistance) startResizeLeft(e);
            else if (Math.Abs(Bounds.Right - mousePoint.X) < cornerDistance) startResizeRight(e);

        }
    }


    void setPrevStats()
    {
        tX = X;
        tY = Y;
        tWidth = ((dynamic)Child).Width;
        tHeight = ((dynamic)Child).Height;
    }

    public void startResizeTopLeft(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            Y = e.GetPosition(Parent).Y;
            var width = p.w + (p.x - e.GetPosition(Parent).X);
            var height = p.h + (p.y - e.GetPosition(Parent).Y);
            ((dynamic)Child).Width = width;
            ((dynamic)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((dynamic)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((dynamic)Child).Height = -height;
            }
            foreach (var l in OnResizing) l();
        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeTopRight(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }

            var width = p.w - (p.x - e.GetPosition(Parent).X);
            var height = p.h + (p.y - e.GetPosition(Parent).Y);
            Log.Write("width: " + width + " height: " + height);

            ((dynamic)Child).Width = width;
            ((dynamic)Child).Height = height;

            Y = e.GetPosition(Parent).Y;

            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                Log.Write(width);
                ((dynamic)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((dynamic)Child).Height = -height;
            }
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottomLeft(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            var width = p.w + (p.x - e.GetPosition(Parent).X);
            var height = p.h - (p.y - e.GetPosition(Parent).Y);
            ((dynamic)Child).Width = width;
            ((dynamic)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((dynamic)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((dynamic)Child).Height = -height;
            }
            foreach (var l in OnResizing) l();
        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottomRight(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var width = p.w - (p.x - e.GetPosition(Parent).X);
            var height = p.h - (p.y - e.GetPosition(Parent).Y);
            ((dynamic)Child).Width = width;
            ((dynamic)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                ((dynamic)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((dynamic)Child).Height = -height;
            }
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeLeft(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            var width = p.w + (p.x - e.GetPosition(Parent).X);
            ((dynamic)Child).Width = width;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((dynamic)Child).Width = -width;
            }
            ((dynamic)Child).Height = p.h;
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeRight(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var width = p.w - (p.x - e.GetPosition(Parent).X);
            ((dynamic)Child).Width = width;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                ((dynamic)Child).Width = -width;
            }
            ((dynamic)Child).Height = p.h;
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeTop(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            Y = e.GetPosition(Parent).Y;
            var height = p.h + (p.y - e.GetPosition(Parent).Y);
            ((dynamic)Child).Height = height;
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((dynamic)Child).Height = -height;
            }
            ((dynamic)Child).Width = p.w;
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottom(PointerPressedEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(Parent).X,
            y = e.GetPosition(Parent).Y,
            w = ((dynamic)Child).Width, // gutter
            h = ((dynamic)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerMovedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((dynamic)Child).Width, ((dynamic)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var height = p.h - (p.y - e.GetPosition(Parent).Y);
            ((dynamic)Child).Height = height;
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((dynamic)Child).Height = -height;
            }
            ((dynamic)Child).Width = p.w;
            foreach (var l in OnResizing) l();

        }

        MainWindow.Instance.AddHandler(PointerMovedEvent, res, RoutingStrategies.Tunnel);
    }

}
