

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Dynamically;
using Dynamically.Backend;
using Dynamically.Backend.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Menus;

public class TopMenu
{
    public static Menu Instance = MainWindow.Instance.Find<Menu>("TopMenu");
    
    public static void applyDefaultStyling()
    {
        //BackgroundColor = null;
        //BorderColor = null;
        //TextColor = null;
    }


    /// <summary>
    /// Default:
    /// <code>Brushes.LightGray</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? BackgroundColor
    {
        get => Instance.Background ?? Brushes.LightGray;
        set => Instance.Background = value ?? Brushes.LightGray;
    }

    /// <summary>
    /// Default:
    /// <code>Brushes.DimGray</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? BorderColor
    {
        get => Instance.BorderBrush ?? Brushes.DimGray;
        set => Instance.BorderBrush = value ?? Brushes.DimGray;
    }

    /// <summary>
    /// Default:
    /// <code>Brushes.Black</code>
    /// set to <c>null</c> for auto default
    /// </summary>
    public static IBrush? TextColor
    {
        get => Instance.Foreground ?? Brushes.Black;
        set => Instance.Foreground = value ?? Brushes.Black;
    }


    public Menu Menu { get; set; }
    public MainWindow Parent { get; set; }

    public TopMenu(Menu menu, MainWindow window)
    {
        Menu = menu;
        Parent = window;

        // Event Listeners

        foreach (var item in Flatten(Menu.Items.OfType<MenuItem>()))
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
        Parent.WindowTabs.CreateNewTab($"Board {Parent.WindowTabs.OpenTabs.Length}");
    }

    public void SelectAll(object? sender, RoutedEventArgs e)
    {
        var pos = Parent.WindowTabs.CurrentBoard.GetPosition();
        Parent.WindowTabs.CurrentBoard.Selection?.Cancel();
        Parent.WindowTabs.CurrentBoard.Selection = new Selection(new Avalonia.Point(0, 0), new Avalonia.Point(Parent.Width - pos.X, Parent.Height - pos.Y), Parent.WindowTabs.CurrentBoard);
    }
}