using CSharpMath.Avalonia;
using CSharpMath.Rendering.FrontEnd;
using Dynamically.Backend.Helpers.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class MathTextBox : MathView
{
    private MathKeyboard mathKeyboard;

    public MathTextBox()
    {
        mathKeyboard = new MathKeyboard();
        mathKeyboard.K += MathKeyboard_KeyPressed;
    }

    private void MathKeyboard_KeyPressed(object sender, KeyPressedEventArgs e)
    {
        // Handle key presses from the MathKeyboard
        // Append the pressed key or perform other actions as needed
        // Update the text of the MathTextBox accordingly
        // For example:
        LaTeX = mathKeyboard.LaTeX;
    }

    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Show the MathKeyboard when the MathTextBox is clicked
        mathKeyboard.
    }
}
