using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public class ItemLoader : MenuItem
{
    public Queue<Func<Control>> Tasks;

    private MenuItem loadingItem = new();

    public bool Working { get; private set; }
    public ItemLoader(ContextMenu parent, params Func<Control>[] tasks)
    {
        
        loadingItem = new MenuItem
        {
            Header = "Loading...",
            Icon = new Image { Source = new Bitmap("Assets/Light/ContextMenu/itemLoader.gif") },
            BorderBrush = new SolidColorBrush(Colors.Red),
            BorderThickness = new Thickness(1),
        };

        Tasks = new Queue<Func<Control>>(tasks);
        Header = new StackPanel();
        (Header as StackPanel).Children.Add(loadingItem);
    }

    public void AddItem(Func<Control> func)
    {
        Tasks.Enqueue(func);
        if (!Working) Work();
    }

    public void AddItems(params Func<Control>[] funcs)
    {
        Tasks.Concat(new Queue<Func<Control>>(funcs));
        if (!Working) Work();
    }

    public void Work()
    {
        Working = true;
        var t = Task.Run(() =>
        {
            while (Tasks.Count > 0)
            {
                (Header as StackPanel).Children.Insert((Header as StackPanel).Children.Count - 1, Tasks.Dequeue()());
            }

            Working = false;
        });
    }
}
