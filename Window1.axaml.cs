using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically
{
    public partial class Log : Window
    {
        public static readonly Log current = new();
        private readonly TextBlock consoleTextBlock;
        private readonly ScrollViewer scrollViewer;
        public Log()
        {
            InitializeComponent();
            consoleTextBlock = new TextBlock
            {
                Width = this.Width,
                Height = this.Height - 30,
                FontSize = 16,
                Text = "Application start!\n--------------------------------\n"
            };

            scrollViewer = new ScrollViewer
            {
                Content = consoleTextBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            Content = scrollViewer;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static void Write(params object?[] text)
        {
            current.consoleTextBlock.Text += StringifyCollection(text) + "\n";
            if (current.consoleTextBlock.Text.Count(c => c.Equals('\n')) + 1 > 1000)
            {
                while (current.consoleTextBlock.Text.Count(c => c.Equals('\n')) + 1 > 1000)
                {
                    current.consoleTextBlock.Text = current.consoleTextBlock.Text.Remove(0, 1);
                }
            }
            current.scrollViewer.ScrollToEnd();
        }

        public static string StringifyCollection(IEnumerable collection)
        {
            var s = new List<string>();
            foreach (var item in collection)
            {
                var itemS = item?.ToString();
                if (itemS == null) {
                    s.Add("null"); 
                    continue; 
                }
                if (item.GetType().IsArray || (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))) itemS = StringifyCollection((IEnumerable)item);
                s.Add(itemS ?? "Value");
            }

            return string.Join(", ", s);
        }
    }
}
