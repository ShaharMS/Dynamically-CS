using Avalonia;
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

        if (Resizable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            CurrentlyResizing = true;
            e.Pointer.Capture(this);

            var mousePoint = e.GetPosition(this);
            
            if (Bounds.TopLeft.DistanceTo(mousePoint) < cornerDistance)
            {
            }

            foreach (var l in OnResizeStart) l();
        }
    }


    void setPrevStats()
    {
        tX = X;
        tY = Y;
        tWidth = ((Control)Child).Width;
        tHeight = ((Control)Child).Height;
    }

    public void startResizeTopLeft(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            Y = e.GetPosition(Parent).Y;
            var width = p.w + (p.x - e.GetPosition(MainWindow.BigScreen).X);
            var height = p.h + (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            ((Control)Child).Width = width;
            ((Control)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((Control)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((Control)Child).Height = -height;
            }
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeTopRight(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }

            var width = p.w - (p.x - e.GetPosition(MainWindow.BigScreen).X);
            var height = p.h + (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            Log.Write("width: " + width + " height: " + height);

            ((Control)Child).Width = width;
            ((Control)Child).Height = height;

            Y = e.GetPosition(Parent).Y;

            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                Log.Write(width);
                ((Control)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((Control)Child).Height = -height;
            }
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottomLeft(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            var width = p.w + (p.x - e.GetPosition(MainWindow.BigScreen).X);
            var height = p.h - (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            ((Control)Child).Width = width;
            ((Control)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((Control)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((Control)Child).Height = -height;
            }
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottomRight(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var width = p.w - (p.x - e.GetPosition(MainWindow.BigScreen).X);
            var height = p.h - (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            ((Control)Child).Width = width;
            ((Control)Child).Height = height;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                ((Control)Child).Width = -width;
            }
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((Control)Child).Height = -height;
            }
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeLeft(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            X = e.GetPosition(Parent).X;
            var width = p.w + (p.x - e.GetPosition(MainWindow.BigScreen).X);
            ((Control)Child).Width = width;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X + width;
                ((Control)Child).Width = -width;
            }
            ((Control)Child).Height = p.h;
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeRight(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var width = p.w - (p.x - e.GetPosition(MainWindow.BigScreen).X);
            ((Control)Child).Width = width;
            if (width < 0)
            {
                X = e.GetPosition(Parent).X;
                ((Control)Child).Width = -width;
            }
            ((Control)Child).Height = p.h;
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeTop(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            Y = e.GetPosition(Parent).Y;
            var height = p.h + (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            ((Control)Child).Height = height;
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y + height;
                ((Control)Child).Height = -height;
            }
            ((Control)Child).Width = p.w;
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

    public void startResizeBottom(PointerEventArgs e)
    {
        var p = new
        {
            x = e.GetPosition(MainWindow.BigScreen).X,
            y = e.GetPosition(MainWindow.BigScreen).Y,
            w = ((Control)Child).Width, // gutter
            h = ((Control)Child).Height // gutter

        };
        setPrevStats();

        void res(object? sender, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                MainWindow.Instance.RemoveHandler(PointerPressedEvent, res);

                foreach (var l in OnResizeEnd) l(X, Y, ((Control)Child).Width, ((Control)Child).Height, tX, tY, tWidth, tHeight);
                return;
            }
            var height = p.h - (p.y - e.GetPosition(MainWindow.BigScreen).Y);
            ((Control)Child).Height = height;
            if (height < 0)
            {
                Y = e.GetPosition(Parent).Y;
                ((Control)Child).Height = -height;
            }
            ((Control)Child).Width = p.w;
        }

        MainWindow.Instance.AddHandler(PointerPressedEvent, res, RoutingStrategies.Tunnel);
    }

}
