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

public class MathTextBox : MathView
{
   public TextBox TextInput;

   public MathTextBox()
   {
        TextInput = new();
        TextInput.KeyDown += (_, _) => {
            Log.Write("Parsing latex...");
            var c = Convert(TextInput.Text ?? "");
            Log.Write("Result:", c, "pre:", TextInput.Text ?? "");
            LaTeX = Convert(TextInput.Text ?? "");
            InvalidateVisual();
        };
        TextInput.SetPosition(200, MainWindow.BigScreen.Height - MainWindow.BigScreen.Y);
        MainWindow.BigScreen.Children.Add(TextInput);

        this.SetPosition(100, 100);
        this.LaTeX = @"X^4 + \frac{x^2}{q_{12}} = 4";
   }

   public static string Convert(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex)
        .Bind(list => Evaluation.Interpret(list))
        .Match(success => success, error => latex);
}
