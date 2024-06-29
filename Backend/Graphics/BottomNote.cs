using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Dynamically.Backend.Helpers;
using Dynamically.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics;

public class BottomNote : Canvas
{
    public BottomNote(string note)
    {
        var label = new Label
        {
            Content = note,
            FontSize = 18,
            Foreground = UIColors.BottomNoteColor
        };
        label.Width = label.GuessTextWidth();

        Children.Add(label);
        Background = UIColors.BottomNoteFill;

        this.SetPosition(MainWindow.Instance.Width / 2 - label.Width / 2, MainWindow.Instance.Height - 150);

        EventHandler pos = null!;
        pos = (_, _) => 
        {
            this.SetPosition(MainWindow.Instance.Width / 2 - label.Width / 2, MainWindow.Instance.Height - 150);
            MainWindow.Instance.LayoutUpdated -= pos;
        };

        MainWindow.Instance.LayoutUpdated += pos;

        MainWindow.Instance.WindowTabs.CurrentBoard.Children.Add(this);
        DispatcherTimer.Run(() =>
        {
            MainWindow.Instance.WindowTabs.CurrentBoard.Children.Remove(this);
            return false;
        }, new TimeSpan(0, 0, 10));
    }
}
