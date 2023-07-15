using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Dynamically.Backend.Graphics;
public class TextSeparator : DockPanel
{
    public string Text { get; set; }

    private Label label;
    public TextSeparator(string text)
    {
        Text = text;
        AttachedToVisualTree += (sender, args) => {
            Process(Text, args.Parent);
        };
        SetDock(this, Dock.Left);
    }

    private void Process(string text, IVisual Parent) {

        label.Content = "―" + label.Content;
        while(label.Bounds.Width < Parent.Bounds.Width - 8) {
            label.Content += "―";
        }
    }
}
