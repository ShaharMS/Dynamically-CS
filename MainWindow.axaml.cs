using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;
using System.Collections.Generic;
using Dynamically.Formulas;
using Dynamically.Menus;
using Dynamically.Screens;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using Dynamically.Backend;

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }

    public static DockPanel MainDisplay { get; private set; }

    public static BigScreen BigScreen { get; private set; }

    public static bool Debug = true;

    public static PointerEventArgs Mouse { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Debug = true;
        Instance = this;
        MainDisplay = Instance.Find<DockPanel>("Display");
        var ca = new BigScreen
        {
            Name = "BigScreen"
        };
        BigScreen = ca;

        /*for (double a = 0; a < Math.PI * 2; a += Math.PI / 180)
        {
            Log.Write(a.RadiansBetween((a + Math.PI / 2) % (Math.PI * 2)) * 180 / Math.PI, a * 180 / Math.PI, ((a + Math.PI / 2) % (Math.PI * 2)) * 180 / Math.PI);
        }*/
        
        Menus.TopMenu.applyDefaultStyling();

        var j = new Joint(30, 30).Connect(new Joint(130, 30));
        var j11 = new Joint(120, 60);
        var t = new Triangle(new Joint(570, 120), new Joint(750, 80), new Joint(860, 320));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInCircle();

        var t2 = new Triangle(new Joint(70, 500), new Joint(250, 570), new Joint(160, 370));
        t2.Type = TriangleType.RIGHT;
        var t3 = new Triangle(new Joint(70, 500), new Joint(250, 570), new Joint(160, 370));


        AddHandler(PointerMovedEvent, (o, a) => { Mouse = a;}, RoutingStrategies.Tunnel);

        MainDisplay.Children.Add(BigScreen);
        BigScreen.Refresh();
    }
}