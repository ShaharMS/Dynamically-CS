using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Dynamically.Backend.Graphics;
public class TextSeparator
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
        }
    }

    public double GuessedTextWidth
    {
        get => Text.Length * 8 * 0.533;
    }

    static readonly double ContextMenuWidth = 133 - 20; //Rename arrow width subtracted

    private readonly List<Control> List;
    public TextSeparator(string text, List<Control> list)
    {
        Text = "― " + text + ": ";
        List = list;

        string getText()
        {
            // Bounds property has changed
            double lineWidth = 4;

            double spaceLeft = ContextMenuWidth - GuessedTextWidth;
            int fittingLines = (int)(spaceLeft / lineWidth);
            //Log.Write(ContextMenuWidth, GuessedTextWidth);
            //Log.Write($"{fittingLines} {spaceLeft} {lineWidth}");
            if (fittingLines > 0) Text += new string('―', fittingLines);
            //Log.Write(Text);
            return Text;
        }

        List.Add(new MenuItem
        {
            Header = getText(),
            FontSize = 8,
            FontWeight = FontWeight.Thin,
            FontFamily = new FontFamily("Consolas"),
            IsEnabled = false,
            Height = 20
        });
    }
}
