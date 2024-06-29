using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dynamically.Backend;
using Dynamically.Shapes;

namespace Dynamically.Menus.ContextMenus;

public class QuadrilateralContextMenuProvider : ContextMenuProvider
{

    public Quadrilateral Subject {get => _sub; set => _sub = value; }
    public QuadrilateralContextMenuProvider(Quadrilateral joint, ContextMenu menu)
    {
        Subject = joint;
        Menu = menu;
        Name = Subject.ToString(true);

        GenerateDefaults();
        GenerateSuggestions();
        GenerateRecommendations();
        if (MainWindow.Debug) AddDebugInfo();
    }

    public override void GenerateDefaults()
    {
        Defaults = new List<Control>
        {
            Defaults_Rotate(),
            Defaults_ChangeType(),
            Defaults_Dismantle(),
            Defaults_Remove()
        };
    }

    public override void GenerateSuggestions()
    {
        Suggestions = new List<Control?>
        {
            //Sgest_GenerateCircumCircle(),
            //Sgest_GenerateInCircle()
        }.FindAll((c) => c != null).Cast<Control>().ToList();
    }

    public override void GenerateRecommendations()
    {
        //Recommendations = Recom_ChangeType();
        Recommendations = new();
        Recommendations.Concat(new List<Control?>
        {

        }.FindAll((c) => c != null).Cast<Control>()).ToList();
    }

    public override void AddDebugInfo()
    {
        Debugging = new List<Control>
        {
            Debug_Type()
        };
    }
    public override void Regenerate()
    {
        base.Regenerate();
        Subject.ContextMenu = new ContextMenu
        {
            Items = Items
        };
    }





    // -------------------------------------------------------
    // ------------------------Default------------------------
    // -------------------------------------------------------
    MenuItem Defaults_Rotate()
    {
        var rotate = new MenuItem
        {
            Header = "Rotate"
        };
        rotate.Click += (sender, e) =>
        {
            Point p1 = new Point(Subject.Vertex1.X, Subject.Vertex1.Y), p2 = new Point(Subject.Vertex2.X, Subject.Vertex2.Y), p3 = new Point(Subject.Vertex3.X, Subject.Vertex3.Y), p4 = new Point(Subject.Vertex4.X, Subject.Vertex4.Y);
            Point rotationCenter = Subject.GetCentroid();
            double dist1 = Subject.Vertex1.DistanceTo(rotationCenter), dist2 = Subject.Vertex2.DistanceTo(rotationCenter), dist3 = Subject.Vertex3.DistanceTo(rotationCenter), dist4 = Subject.Vertex4.DistanceTo(rotationCenter);
            double initialRotationRad = rotationCenter.RadiansTo(Subject.ParentBoard.MousePosition);

            double rad1 = rotationCenter.RadiansTo(p1), rad2 = rotationCenter.RadiansTo(p2), rad3 = rotationCenter.RadiansTo(p3), rad4 = rotationCenter.RadiansTo(p4);

            void Move(object? sender, PointerEventArgs args)
            {
                var currentRotation = rotationCenter.RadiansTo(args.GetPosition(null)) - initialRotationRad;
                Subject.Vertex1.X = rotationCenter.X + dist1 * Math.Cos(rad1 + currentRotation); Subject.Vertex1.Y = rotationCenter.Y + dist1 * Math.Sin(rad1 + currentRotation);
                Subject.Vertex2.X = rotationCenter.X + dist2 * Math.Cos(rad2 + currentRotation); Subject.Vertex2.Y = rotationCenter.Y + dist2 * Math.Sin(rad2 + currentRotation);
                Subject.Vertex3.X = rotationCenter.X + dist3 * Math.Cos(rad3 + currentRotation); Subject.Vertex3.Y = rotationCenter.Y + dist3 * Math.Sin(rad3 + currentRotation);
                Subject.Vertex4.X = rotationCenter.X + dist4 * Math.Cos(rad4 + currentRotation); Subject.Vertex4.Y = rotationCenter.Y + dist4 * Math.Sin(rad4 + currentRotation);
                Subject.Vertex1.DispatchOnMovedEvents(); Subject.Vertex2.DispatchOnMovedEvents(); Subject.Vertex3.DispatchOnMovedEvents();
            }

            void Finish(object? sender, PointerPressedEventArgs arg)
            {
                Subject.ParentBoard.Window.PointerMoved -= Move;
                Subject.ParentBoard.Window.PointerPressed -= Finish;
            }

            Subject.ParentBoard.Window.PointerMoved += Move;
            Subject.ParentBoard.Window.PointerPressed += Finish;
        };


        return rotate;
    }

    MenuItem Defaults_ChangeType()
    {
        var items = new MenuItem[9];
        for (int i = 0; i < 9; i++)
        {
            var type = (QuadrilateralType)i;
            var item = new MenuItem
            {
                Header = new CultureInfo("en-US", false).TextInfo.ToTitleCase(type.ToString().ToLower().Replace('_', ' ')) + (Subject.Type == type ? " ✓" : "")
            };
            item.Click += (s, e) =>
            {
                Log.Write($"{Subject} changed type to {type}");
                Subject.Type = type;
            };
            items[i] = item;
        }

        return new MenuItem
        {
            Header = "Change Type",
            Items = items
        };
    }

    MenuItem Defaults_Dismantle()
    {
        var remove = new MenuItem
        {
            Header = "Dismantle"
        };
        remove.Click += (sender, e) =>
        {
            Subject.Dismantle();
        };
        return remove;
    }

    MenuItem Defaults_Remove()
    {
        var remove = new MenuItem
        {
            Header = $"Remove {Subject.Vertex1}, {Subject.Vertex2}, {Subject.Vertex3} & {Subject.Vertex4}"
        };
        remove.Click += (sender, e) =>
        {
            foreach (var item in new[] { Subject.Vertex1, Subject.Vertex3, Subject.Vertex4, Subject.Vertex2 }) item.RemoveFromBoard();
        };
        return remove;
    }




    // -------------------------------------------------------
    // --------------------------Debug------------------------
    // -------------------------------------------------------
    MenuItem Debug_Type()
    {
        return new MenuItem
        {
            Header = $"Type: {Subject.Type}"
        };
    }
}
