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
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls.Presenters;
using Avalonia.Media;

namespace Dynamically.Backend.Graphics.SolutionTable;

public class MathTextBox : Canvas
{
    public TextBox TextBox;
    public MathView MathView;

    public string Text
    {
        get => TextBox.Text; set => TextBox.Text = value;
    }

    public new double Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            TextBox.Width = value;
            MathView.Width = value;
        }
    }

    public new double Height
    {
        get => TextBox.Height.Max(MathView.Height);
        set
        {
            base.Height = value;
        }
    }

    public MathTextBox()
    {
        TextBox = new TextBox
        {
            Opacity = 0,
            TextWrapping = TextWrapping.NoWrap,
            AcceptsReturn = true,
        };
        MathView = new MathView
        {
            FontSize = (float)TextBox.FontSize,
            DisplacementX = (float)TextBox.FontSize / 4,
            TextAlignment = CSharpMath.Rendering.FrontEnd.TextAlignment.Left,
            HighlightColor = Colors.Red,
        };

        TextBox.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(TextBox.Text) || MathView == null) return;
            MathView.LaTeX = Latex.Latex.Latexify(TextBox.Text ?? "");
            MathView.SetPosition(Canvas.GetLeft(TextBox), Canvas.GetTop(TextBox) - MathView.Bounds.Height / 2 + TextBox.Bounds.Height / 2);
            //PrintDebugPosition();
        };
        TextBox.LayoutUpdated += (s, e) =>
        {
            //Log.WriteVar(MathView.Bounds.Height);
            // Second pass, incase the first didnt catch the bounds update
            MathView.SetPosition(Canvas.GetLeft(TextBox), Canvas.GetTop(TextBox) - MathView.Bounds.Height / 2 + TextBox.Bounds.Height / 2);
        };
        TextBox.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(TextBox.Bounds)) return;
            if (TextBox.Opacity == 1) Height = TextBox.Bounds.Height;
            else Height = MathView.Bounds.Height;
        };

        TextBox.LostFocus += (s, e) =>
        {
            TextBox.Opacity = 0;
            MathView.Opacity = 1;
            
            Height = MathView.Bounds.Height;
        };
        EventHandler<GotFocusEventArgs> giveFocus = (s, e) =>
        {
            MathView.Opacity = 0.1;
            TextBox.Opacity = 1;
            TextBox.Focus();

            Height = TextBox.Bounds.Height;
        };

        TextBox.GotFocus += giveFocus;
        MathView.GotFocus += giveFocus;

        MathView.PointerEnter += (_, _) => Cursor = new Cursor(StandardCursorType.Ibeam);
        MathView.PointerLeave += (_, _) => Cursor = Cursor.Default;

        MathView.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name != nameof(MathView.Bounds)) return;
            if (TextBox.Opacity == 1) Height = TextBox.Bounds.Height;
            else Height = MathView.Bounds.Height;
        };

        MainWindow.Instance.AddHandler(PointerPressedEvent, (_, e) =>
        {
            //Log.WriteVar(e.Source);
            if (e.Source == TextBox || e.Source == MathView)
            {
                if (!TextBox.IsFocused) TextBox.Focus();
                else return;
            }
            else MainWindow.BigScreen.Focus();
        }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        TextBox.SetPosition(0, 0);


        Children.Add(MathView); Children.Add(TextBox);
        AttachedToVisualTree += (_, _) =>
        {
            TextBox.Text = TextBox.Text;
        };
    }

    public virtual int CaretPositionFromPoint(Point point) {

        if (MathView.Painter.Content == null) return 0;
        return 0;
    }

    public virtual void PrintDebugPosition()
    {
        Log.WriteVar(MathView.Content?.ToList());
        if (MathView.Content == null) return;

        foreach (var token in MathView.Content) 
        {
            Log.WriteAsTree(token);
        }
    }
}
