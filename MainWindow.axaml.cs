using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GeometryBackend;
using Dynamically.Shapes;

namespace Dynamically;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();


        var j = new Joint(30, 30, 'A').Connect(new Joint(130, 30, 'B')).Connect(new Joint(120, 60, 'C'));
        var c = new EllipseBase(new Joint(250, 250, 'D'), new Joint(350, 250, 'E'), 150);
        var c2 = new EllipseBase(new Joint(400, 400, 'F'), new Joint(400, 400, 'F'), 100);
        var t = new Triangle(new Joint(20, 80, 'G'), new Joint(50, 85, 'H'), new Joint(60, 120, 'I'));
        var circ = new Circle(new Joint(300, 300, 'J'), 200);
        var ca = new Canvas();

        foreach (var i in Connection.all)
            ca.Children.Add(i);
        foreach (var i in EllipseBase.all)
            ca.Children.Add(i);
        foreach (var i in Joint.all)
            ca.Children.Add(i);

        //ca.Children.Add(c);
        //ca.Children.Add(c2);
        Content = ca;
        //var t = new Triangle(new Joint(20, 80, 'G'), new Joint(50, 85, 'H'), new Joint(60, 120, 'I'));
    }
}