using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically
{
    public partial class Log : Window
    {
        public static Log current = new Log();
        private TextBlock consoleTextBlock;

        public Log()
        {
            InitializeComponent();
            consoleTextBlock = new TextBlock
            {
                Width = this.Width,
                Height = this.Height,
                FontSize = 16
            };

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = consoleTextBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            Content = scrollViewer;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static void Write(params object[] text)
        {
            current.consoleTextBlock.Text += string.Join(", ", text) + "\n";
        }
    }
}
