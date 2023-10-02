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
using Dynamically.Backend.Interfaces;
using System.Linq;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Graphics.SolutionTable;
using Dynamically.Backend.Latex;
using Dynamically.Solver.ExerciseInfoExtraction;

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }

    public static Canvas MainDisplay { get; private set; }

    public static DockPanel MainPanel { get; private set; }

    public static BigScreen BigScreen { get; private set; }

    public static bool Debug = true;

    public static PointerEventArgs Mouse { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Debug = true;
        Instance = this;
        MainDisplay = Instance.Find<Canvas>("Main");
        MainPanel = Instance.Find<DockPanel>("Display");
        var ca = new BigScreen
        {
            Name = "BigScreen"
        };
        BigScreen = ca;
        BigScreen.SetPosition(0, 0);
        
        Menus.TopMenu.applyDefaultStyling();

        var j0 = new Joint(130, 30);
        var j = new Joint(30, 30);
        j.Connect(j0);
        var j11 = new Joint(120, 90);
        j11.Connect(j);
        var t = new Triangle(new Joint(570, 20), new Joint(750, 10), new Joint(860, 220));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInCircle();

        var t4 = new Triangle(new Joint(70, 500), new Joint(250, 570), new Joint(160, 370));
        t4.Type = TriangleType.EQUILATERAL;
        var t6 = new Triangle(new Joint(70, 500), new Joint(250, 570), new Joint(160, 370));

        var q1 = new Quadrilateral(new Joint(600, 350), new Joint(900, 300), new Joint(600, 450), new Joint(900, 600));


        new Angle(j0, j, j11);

        AddHandler(PointerMovedEvent, (o, a) => { Mouse = a;}, RoutingStrategies.Tunnel);

        MainDisplay.Children.Add(BigScreen);
        BigScreen.SetPosition(0, 0);



        foreach (DraggableGraphic obj in Joint.all.ToList<DraggableGraphic>().Concat(Segment.all).Concat(Ring.all)) obj.OnDragged.Add(regenAll);

        BigScreen.Refresh();
         
        regenAll(0, 0, 0, 0);

        new BottomNote("Application Started!");
        BigScreen.Children.Add(new SolutionTable(true));
        BigScreen.Children.Add(new MathTextBox());

        Log.WriteVar(Latex.Latexify("123 + AB / 3 = 66 * 5"));

    }

    public static void regenAll(double z, double x, double c, double v) {
        _ = z; _ = x; _ = c; _ = v;
        foreach (dynamic item in Joint.all.Concat<dynamic>(Segment.all).Concat(Triangle.all).Concat(Quadrilateral.all).Concat(Circle.all).Concat(Angle.all))
        {
            item.Provider.Regenerate();
        }
    }
}