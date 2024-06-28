using Avalonia.Controls;
using Dynamically.Backend.Geometry;
using Dynamically.Shapes;
using Dynamically.Containers;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dynamically.Backend;
using System.Linq;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Graphics.SolutionTable;
using Dynamically.Backend.Latex;
using Dynamically.Menus;
using System;

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }
    public static PointerEventArgs Mouse { get; set; }

    public static bool Debug { get; set; } = true;

    private static DockPanel MainPanel;

    public Board MainBoard { get; private set; }
    public Tabs WindowTabs { get; private set; }

    public TopMenu TopMenu { get; private set; }



#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Console.WriteLine("Hey There!");
        Debug = true;
        Instance = this;
        MainPanel = Instance.Find<DockPanel>("Display");
        WindowTabs = new Tabs(Instance.Find<TabControl>("Tabs"), this);
        TopMenu = new TopMenu(Instance.Find<Menu>("Menu"), this);
        var ca = new Board(this)
        {
            Name = "MainBoard"
        };
        MainBoard = ca;
        MainBoard.SetPosition(0, 0);
        Instance.Find<TabItem>("__MainBoard").Content = MainBoard;

        Menus.TopMenu.applyDefaultStyling();

        var j0 = new Vertex(MainBoard, 130, 30);
        var j = new Vertex(MainBoard, 30, 30);
        j.Connect(j0);
        var j11 = new Vertex(MainBoard, 120, 90);
        j11.Connect(j);
        var t = new Triangle(new Vertex(MainBoard, 570, 20), new Vertex(MainBoard, 750, 10), new Vertex(MainBoard, 860, 220));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInCircle();

        var t4 = new Triangle(new Vertex(MainBoard, 70, 500), new Vertex(MainBoard, 250, 570), new Vertex(MainBoard, 160, 370));
        t4.Type = TriangleType.EQUILATERAL;
        var t6 = new Triangle(new Vertex(MainBoard, 70, 500), new Vertex(MainBoard, 250, 570), new Vertex(MainBoard, 160, 370));

        var q1 = new Quadrilateral(new Vertex(MainBoard, 600, 350), new Vertex(MainBoard, 900, 300), new Vertex(MainBoard, 600, 450), new Vertex(MainBoard, 900, 600));

        var a1 = new Arc(new Vertex(MainBoard, 250, 400), 230);

        _ = new Angle(j0, j, j11);

        AddHandler(PointerMovedEvent, (o, a) => { Mouse = a;}, RoutingStrategies.Tunnel);



        foreach (DraggableGraphic obj in Vertex.All.ToList<DraggableGraphic>().Concat(Segment.All).Concat(Ring.All)) obj.OnDragged.Add(RegenAll);

        MainBoard.Refresh();
         
        RegenAll(0, 0, 0, 0);

        _ = new BottomNote("Application Started!");
        //MainBoard.Children.Add(new SolutionTable(MainBoard, true));

    }

    public static void RegenAll(double z, double x, double c, double v) {
        _ = z; _ = x; _ = c; _ = v;
        foreach (dynamic item in Vertex.All.Concat<dynamic>(Segment.All).Concat(Triangle.All).Concat(Quadrilateral.All).Concat(Circle.All).Concat(Angle.All))
        {
            item.Provider.Regenerate();
        }
    }

}