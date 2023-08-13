using Avalonia;
using Avalonia.Controls;
using System;

namespace Dynamically;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        if (!MainWindow.Debug) 
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
        
        return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .AfterSetup((_) =>
                {
                    var logWindow = Log.current;
                    var mainWindow = new MainWindow();

                    var screen = mainWindow.Screens.Primary; // Get the primary screen

                    // Calculate the width and height of each window
                    var windowWidth = screen.WorkingArea.Width;
                    var windowHeight = screen.WorkingArea.Height;

                    // Set the size and position of the MainWindow
                    logWindow.Width = windowWidth / 4;
                    logWindow.Height = windowHeight;
                    logWindow.Position = new PixelPoint(screen.WorkingArea.TopLeft.X, screen.WorkingArea.TopLeft.Y);

                    // Set the size and position of the LogWindow
                    mainWindow.Width = windowWidth / 4 * 3;
                    mainWindow.Height = windowHeight;
                    mainWindow.Position = new PixelPoint(screen.WorkingArea.TopLeft.X + windowWidth / 4, screen.WorkingArea.TopLeft.Y);

                    logWindow.Show();
                }).LogToTrace();
    }
}
