using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GeometryBackend;
using Dynamically.Shapes;
using System.Collections.Generic;
using Dynamically.Formulas;
using Menus;

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }
    public static Canvas BigScreen { get; private set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;

        Menus.TopMenu.applyDefaultStyling();

        var j = new Joint(30, 30, 'A').Connect(new Joint(130, 30, 'B'));
        var j11 = new Joint(120, 60, 'C');
        var t = new Triangle(new Joint(670, 320, 'G'), new Joint(850, 280, 'H'), new Joint(960, 520, 'I'));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInscribedCircle();
        var ca = this.Find<Canvas>("Screen");
        BigScreen = ca;
        //var j1 = new Joint(100, 500);
        //j1.geometricPosition.Add(new RayFormula(500, 0));
        //j1.geometricPosition.Add(new CircleFormula(50, 120, 500));
        
        foreach (var i in Connection.all)
            ca.Children.Add(i);
        foreach (var i in EllipseBase.all)
            ca.Children.Add(i);
        foreach (var i in Joint.all)
            ca.Children.Add(i);

        //ca.Children.Add(c);
        //ca.Children.Add(c2);
        //Content = ca;
        //var t = new Triangle(new Joint(20, 80, 'G'), new Joint(50, 85, 'H'), new Joint(60, 120, 'I'));
    }
}