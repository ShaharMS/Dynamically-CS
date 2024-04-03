using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;

namespace Dynamically;

public partial class Log : Window
{
    public static readonly Log Instance = Program.HasConsole ? null! : new();
    private readonly TextBlock consoleTextBlock;
    private readonly ScrollViewer scrollViewer;
    public Log()
    {
        InitializeComponent();
        consoleTextBlock = new TextBlock
        {
            Width = Width,
            Height = Height - 30,
            FontSize = 16,
            Text = "Application start!\n--------------------------------\n"
        };

        scrollViewer = new ScrollViewer
        {
            Content = consoleTextBlock,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        consoleTextBlock.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == nameof(consoleTextBlock.Text))
            {
                scrollViewer.ScrollToEnd();
            }
        };

        Content = scrollViewer;

        LayoutUpdated += (_, _) =>
        {
            scrollViewer.Width = Width;
            scrollViewer.Height = Height;
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    public static uint Indent { get; set; } = 0;

    static void __Write(params object?[] text)
    {
        if (Program.HasConsole)
        {
            Console.Write(new string(' ', (int)Indent * 4) + StringifyCollection(text) + "\n");
        }
        else
        {
            Instance.consoleTextBlock.Text += new string(' ', (int)Indent * 4) + StringifyCollection(text) + "\n";
            if (Instance.consoleTextBlock.Text.Count(c => c.Equals('\n')) + 1 > 1000)
            {
                while (Instance.consoleTextBlock.Text.Count(c => c.Equals('\n')) + 1 > 1000)
                {
                    Instance.consoleTextBlock.Text = Instance.consoleTextBlock.Text.Remove(0, 1);
                    Instance.scrollViewer.ScrollToEnd();
                }
            }
        }

    }

    public static void Write(
        object? self, object? self1 = null, object? self2 = null, object? self3 = null, object? self4 = null, object? self5 = null, object? self6 = null, object? self7 = null, object? self8 = null, object? self9 = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        filePath = filePath.Split("Dynamically-CS").Last().Substring(1);

        var p = new object?[] { self, self1, self2, self3, self4, self5, self6, self7, self8, self9 }.Where(x => x != null).Select(x => Stringify(x));
        var str = string.Join(", ", p);
        __Write(filePath + ":" + lineNumber + ": " + str);
    }

    /// <summary>
    /// Pretty-prints up to 10 var-value pairs
    /// </summary>
    public static void WriteVar(
        object? self, object? self1 = null, object? self2 = null, object? self3 = null, object? self4 = null, object? self5 = null, object? self6 = null, object? self7 = null, object? self8 = null, object? self9 = null,
        [CallerArgumentExpression("self")] string paramName = "",
        [CallerArgumentExpression("self1")] string paramName1 = "",
        [CallerArgumentExpression("self2")] string paramName2 = "",
        [CallerArgumentExpression("self3")] string paramName3 = "",
        [CallerArgumentExpression("self4")] string paramName4 = "",
        [CallerArgumentExpression("self5")] string paramName5 = "",
        [CallerArgumentExpression("self6")] string paramName6 = "",
        [CallerArgumentExpression("self7")] string paramName7 = "",
        [CallerArgumentExpression("self8")] string paramName8 = "",
        [CallerArgumentExpression("self9")] string paramName9 = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        filePath = filePath.Split("Dynamically-CS").Last().Substring(1);

        if (paramName == "") throw new ArgumentException("WriteVar requires at least 1 argument"); else __Write($"{filePath}:{lineNumber}: {paramName}: {Stringify(self)}");
        if (paramName1 != "") __Write($"{filePath}:{lineNumber}: {paramName1}: {Stringify(self1)}");
        if (paramName2 != "") __Write($"{filePath}:{lineNumber}: {paramName2}: {Stringify(self2)}");
        if (paramName3 != "") __Write($"{filePath}:{lineNumber}: {paramName3}: {Stringify(self3)}");
        if (paramName4 != "") __Write($"{filePath}:{lineNumber}: {paramName4}: {Stringify(self4)}");
        if (paramName5 != "") __Write($"{filePath}:{lineNumber}: {paramName5}: {Stringify(self5)}");
        if (paramName6 != "") __Write($"{filePath}:{lineNumber}: {paramName6}: {Stringify(self6)}");
        if (paramName7 != "") __Write($"{filePath}:{lineNumber}: {paramName7}: {Stringify(self7)}");
        if (paramName8 != "") __Write($"{filePath}:{lineNumber}: {paramName8}: {Stringify(self8)}");
        if (paramName9 != "") __Write($"{filePath}:{lineNumber}: {paramName9}: {Stringify(self9)}");
    }

    public static void WriteAsTree(object? self, [CallerArgumentExpression("self")] string paramName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        filePath = filePath.Split("Dynamically-CS").Last().Substring(1);

        if (paramName == null) throw new ArgumentException("WriteAsTree requires at least 1 argument"); else __Write($"{filePath}:{lineNumber}: {paramName}: {Stringify(self)}");
        Indent++;
        foreach (PropertyInfo prop in self!.GetType().GetProperties().Where(p => !p.GetIndexParameters().Any()))
        {
            __Write($"{prop.Name}: {Stringify(prop.GetValue(self))}");
        }
        Indent--;
    }

    public static string Stringify(params object?[] objects) => StringifyCollection(objects);

    public static string StringifyCollection(IEnumerable collection)
    {
        var s = new List<string>();
        foreach (var item in collection)
        {
            var itemS = item is Catalyst.TokenData t ? JsonSerializer.Serialize(t) : item?.ToString();
            if (itemS == null)
            {
                s.Add("null");
                continue;
            }
            if (item!.GetType().IsArray || (item.GetType().IsGenericType && new[] { typeof(List<>), typeof(HashSet<>), typeof(ObservableCollection<>) }.Contains(item.GetType().GetGenericTypeDefinition()))) itemS = StringifyCollection((IEnumerable)item);
            else if (item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>)) itemS = StringifyCollection((IEnumerable)item);
            s.Add(itemS ?? "Value");
        }

        return string.Join(", ", s);
    }
}
