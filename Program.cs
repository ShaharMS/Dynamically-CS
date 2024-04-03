using Avalonia;
using Avalonia.Controls;
using System;
using System.Runtime.InteropServices;

namespace Dynamically;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        HasConsole = AttachConsole(-1);
        if (!HasConsole)
        {
            HasConsole = AllocConsole();
        }
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int pid);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

    public static bool HasConsole;

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        if (!MainWindow.Debug) 
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
        
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .AfterSetup((_) =>
                {
                    var mainWindow = new MainWindow();

                    var screen = mainWindow.Screens.Primary; // Get the primary screen

                    // Calculate the width and height of each window
                    var windowWidth = screen.WorkingArea.Width;
                    var windowHeight = screen.WorkingArea.Height;

                    // Set the size and position of the MainWindow
                    if (HasConsole)
                    {
                        //Console.SetWindowSize(windowWidth / 3 / 9, windowHeight / 22);
                        MoveWindow(GetConsoleWindow(), screen.WorkingArea.TopLeft.X, screen.WorkingArea.TopLeft.Y, windowWidth / 3, windowHeight, true);
                        Log.Write(HasConsole);
                    } 
                    else
                    {
                        var logWindow = Log.Instance;
                        logWindow.Width = windowWidth / 3;
                        logWindow.Height = windowHeight - 50;
                        logWindow.Position = new PixelPoint(screen.WorkingArea.TopLeft.X, screen.WorkingArea.TopLeft.Y);
                        logWindow.Show();
                    }

                    // Set the size and position of the LogWindow
                    mainWindow.Width = windowWidth / 3 * 2;
                    mainWindow.Height = windowHeight;
                    mainWindow.Position = new PixelPoint(screen.WorkingArea.TopLeft.X + windowWidth / 3, screen.WorkingArea.TopLeft.Y);

                }).LogToTrace();
    }
}
