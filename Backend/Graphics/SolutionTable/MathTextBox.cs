using Avalonia.Controls;
using CSharpMath.Avalonia;
using CSharpMath.Atom;
using CSharpMath.Rendering.FrontEnd;
using Dynamically.Backend.Helpers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpMath;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class MathTextBox : Canvas
{
    public TextBox TextBox;
    public MathView MathView;


    public MathTextBox()
    {
        TextBox = new TextBox
        {
            Opacity = 0.05,
            Text = @"AB = AC = BC"
        };
        TextBox.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(TextBox.Text) || MathView == null) return;
            MathView.LaTeX = Latex.Latex.Latexify(TextBox.Text ?? "");
            Log.Write(TextBox.Text, Latex.Latex.Debugify(TextBox.Text ?? ""));
            MathView.SetPosition(Canvas.GetLeft(TextBox), Canvas.GetTop(TextBox) - MathView.Bounds.Height / 2 + TextBox.Bounds.Height / 2 + 200);
        };
        TextBox.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(TextBox.Bounds) || MathView == null) return;
            // Second pass, incase the first didnt catch the bounds update
            MathView.SetPosition(Canvas.GetLeft(TextBox), Canvas.GetTop(TextBox) - MathView.Bounds.Height / 2 + TextBox.Bounds.Height / 2 + 200);
        };

        MathView = new MathView
        {
            LaTeX = @"AB = AC = BC",
            FontSize = (float)TextBox.FontSize,
            DisplacementX = (float)TextBox.FontSize / 4,
        };
        MathView.PointerPressed += (s, e) =>
        {
            Log.Write("Clicked!");
        };

        this.SetPosition(100, 100);
        TextBox.SetPosition(0, 0); MathView.SetPosition(0, 0);


        Children.Add(MathView); Children.Add(TextBox);
        AttachedToVisualTree += (_, _) => TextBox.Text = TextBox.Text;
    }
}
