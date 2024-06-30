

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dynamically;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Menus;

public class TopMenu
{
    public Menu Options { get; set; }
    public AppWindow Window { get; set; }

    public TopMenu(Menu menu, AppWindow window)
    {
        Options = menu;
        Window = window;

        // Event Listeners

        foreach (var item in Flatten(Options.Items.OfType<MenuItem>()))
        {
            switch (item.Header)
            {
                case "Add New Board": item.Click += AddNewBoard; break;
                case "Select All": item.Click += SelectAll; break;
            }
        }
    }

    private MenuItem[] Flatten(IEnumerable<MenuItem> items)
    {
        var list = new List<MenuItem>();
        foreach (var item in items)
        {
            list.Add(item);
            list.AddRange(Flatten(item.Items.OfType<MenuItem>()));
        }
        return list.ToArray();
    }

    public void AddNewBoard(object? sender, RoutedEventArgs e)
    {
        Window.WindowTabs.CreateNewTab($"Board {Window.WindowTabs.OpenTabs.Length}");
    }

    public void SelectAll(object? sender, RoutedEventArgs e)
    {
        Window.WindowTabs.CurrentBoard.Selection?.Cancel();

        var pos = Window.WindowTabs.CurrentBoard.GetPosition();
        Window.WindowTabs.CurrentBoard.Selection = new Selection(new Avalonia.Point(-pos.X, -pos.Y), new Avalonia.Point(Window.Width, Window.Height), Window.WindowTabs.CurrentBoard);
    }
}