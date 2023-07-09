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

            ContextMenu contextMenu = new ContextMenu();

            List<object> menuItems = new List<object>();

            MenuItem menuItem1 = new MenuItem
            {
                Header = "Option 1"
            };
            menuItems.Add(menuItem1);

            MenuItem submenuItem = new MenuItem
            {
                Header = "Submenu"
            };

            MenuItem subMenuItem1 = new MenuItem
            {
                Header = "Sub-option 1"
            };
            submenuItem.Items = new List<object> { subMenuItem1 };

            MenuItem subMenuItem2 = new MenuItem
            {
                Header = "Sub-option 2"
            };
            submenuItem.Items = new List<object> { subMenuItem2 };

            menuItems.Add(submenuItem);

            contextMenu.Items = menuItems;

            Button button = new Button
            {
                Content = "Right-click me!",
                ContextMenu = contextMenu
            };
            Content = button;
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
