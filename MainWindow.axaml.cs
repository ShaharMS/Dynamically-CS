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

namespace Dynamically;

public partial class MainWindow : Window
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static MainWindow Instance { get; private set; }
    public static PointerEventArgs Mouse { get; set; }

    public static bool Debug { get; set; } = true;

    public static Canvas MainDisplay { get; private set; }
    private static DockPanel MainPanel;

    public Board MainBoard { get; private set; }
    public Tabs WindowTabs { get; private set; }

    public TopMenu TopMenu { get; private set; }



#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public MainWindow()
    {
        InitializeComponent();
        Debug = true;
        Instance = this;
        MainDisplay = Instance.Find<Canvas>("Main");
        MainPanel = Instance.Find<DockPanel>("Display");
        WindowTabs = new Tabs(Instance.Find<TabControl>("Tabs"));
        TopMenu = new TopMenu(Instance.Find<Menu>("Menu"), this);
        var ca = new Board
        {
            Name = "MainBoard"
        };
        MainBoard = ca;
        MainBoard.SetPosition(0, 0);
        
        Menus.TopMenu.applyDefaultStyling();

        var j0 = new Vertex(130, 30);
        var j = new Vertex(30, 30);
        j.Connect(j0);
        var j11 = new Vertex(120, 90);
        j11.Connect(j);
        var t = new Triangle(new Vertex(570, 20), new Vertex(750, 10), new Vertex(860, 220));
        var circ = t.GenerateCircumCircle();
        var circ2 = t.GenerateInCircle();

        var t4 = new Triangle(new Vertex(70, 500), new Vertex(250, 570), new Vertex(160, 370));
        t4.Type = TriangleType.EQUILATERAL;
        var t6 = new Triangle(new Vertex(70, 500), new Vertex(250, 570), new Vertex(160, 370));

        var q1 = new Quadrilateral(new Vertex(600, 350), new Vertex(900, 300), new Vertex(600, 450), new Vertex(900, 600));


        _ = new Angle(j0, j, j11);

        AddHandler(PointerMovedEvent, (o, a) => { Mouse = a;}, RoutingStrategies.Tunnel);

        MainDisplay.Children.Add(MainBoard);
        MainBoard.SetPosition(0, 0);



        foreach (DraggableGraphic obj in Vertex.All.ToList<DraggableGraphic>().Concat(Segment.all).Concat(Ring.All)) obj.OnDragged.Add(RegenAll);

        MainBoard.Refresh();
         
        RegenAll(0, 0, 0, 0);

        _ = new BottomNote("Application Started!");
        MainBoard.Children.Add(new SolutionTable(true));
        MainBoard.Children.Add(new MathTextBox());

        Log.WriteVar(Latex.Latexify("123 + AB / 3 = 66 * 5"));

    }

    public static void RegenAll(double z, double x, double c, double v) {
        _ = z; _ = x; _ = c; _ = v;
        foreach (dynamic item in Vertex.All.Concat<dynamic>(Segment.all).Concat(Triangle.All).Concat(Quadrilateral.All).Concat(Circle.All).Concat(Angle.All))
        {
            item.Provider.Regenerate();
        }
    }
}