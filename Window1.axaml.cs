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
            current.consoleTextBlock.Text += StringifyCollection(text) + "\n";
        }

        public static string StringifyCollection(IEnumerable collection)
        {
            var s = new List<string>();
            foreach (var item in collection) 
            { 
                var itemS = item.ToString();
                if (item.GetType().IsArray || (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(List<>))) itemS = StringifyCollection((IEnumerable)item);
                s.Add(itemS);
            }

            return string.Join(", ", s);
        }
    }
}
