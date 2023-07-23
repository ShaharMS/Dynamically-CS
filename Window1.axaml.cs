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
        public static Log current = new Log();
        private TextBlock consoleTextBlock;
        private ScrollViewer scrollViewer;
        public Log()
        {
            InitializeComponent();
            consoleTextBlock = new TextBlock
            {
                Width = this.Width,
                Height = this.Height,
                FontSize = 16
            };

            scrollViewer = new ScrollViewer
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
            current.consoleTextBlock.Text += StringifyCollection(text) + "\n";
            current.scrollViewer.ScrollToEnd();
        }

        public static string StringifyCollection(IEnumerable collection)
        {
            var s = new List<string>();
            foreach (var item in collection) 
            { 
                var itemS = item.ToString();
                if (item.GetType().IsArray || (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))) itemS = StringifyCollection((IEnumerable)item);
                s.Add(itemS ?? "Value");
            }

            return string.Join(", ", s);
        }
    }
}
