using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;
using System.Collections.Generic;
using Dynamically.Formulas;
using Dynamically.Menus;
using Dynamically.Screens;

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }

    public static DockPanel MainDisplay { get; private set; }
    public static BigScreen BigScreen { get; private set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        MainDisplay = Instance.Find<DockPanel>("Display");
        Menus.TopMenu.applyDefaultStyling();

        var j = new Joint(30, 30, 'A').Connect(new Joint(130, 30, 'B'));
        var j11 = new Joint(120, 60, 'C');
        var t = new Triangle(new Joint(570, 120, 'G'), new Joint(750, 80, 'H'), new Joint(860, 320, 'I'));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInscribedCircle();
        var ca = new BigScreen();
        ca.Name = "BigScreen";
        BigScreen = ca;

        MainDisplay.Children.Add(BigScreen);
        
        foreach (var i in Connection.all)
            ca.Children.Add(i);
        foreach (var i in EllipseBase.all)
            ca.Children.Add(i);
        foreach (var i in Joint.all)
            ca.Children.Add(i);
    }
}