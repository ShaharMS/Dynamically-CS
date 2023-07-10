using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace Dynamically
{
    public partial class Log : Window
    {
        public static Log current = new Log();
        private TextBlock consoleTextBlock;

        public Log()
        {
            InitializeComponent();
            consoleTextBlock = new TextBlock();
            consoleTextBlock.Width = this.Width;
            consoleTextBlock.Height = this.Height;
            consoleTextBlock.FontSize = 16;
            Content = consoleTextBlock;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static void Write(object text)
        {
            current.consoleTextBlock.Text += text + "\n";
        }
    }
}
