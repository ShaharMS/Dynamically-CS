using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Containers;

public class Tabs
{
    public TabControl TabContainer;

    public Window Window { get; private set; }

    public Board MainBoard
    {
        get => (Board)TabContainer.Items.OfType<TabItem>().First(item => item.Name == "__MainBoard").Content;
    }

    public TabItem[] OpenTabs
    {
        get => TabContainer.Items.OfType<TabItem>().ToArray();
    }

    public Board CurrentBoard
    {
        get => GetBoardOfTab((TabContainer.SelectedItem as TabItem)!);
    }

    public TabItem CurrentTab
    {
        get => (TabContainer.SelectedItem as TabItem)!;
        set => TabContainer.SelectedItem = value;
    }

    public Tabs(TabControl container, Window window)
    {
        TabContainer = container;
    }


    public Board GetBoardOfTab(TabItem tab) => (Board)tab.Content;

    public TabItem CreateNewTab(string name)
    {
        var board = new Board(Window);
        var item = new TabItem
        {
            Header = new Label
            {
                Content = name,
                Background = new SolidColorBrush(Colors.Black),
            },
            Content = board,

        };

        TabContainer.Items = OpenTabs.Concat(new[] { item }).ToArray();

        return item;
    }

    public TabItem LoadTab() => throw new NotImplementedException();
}
