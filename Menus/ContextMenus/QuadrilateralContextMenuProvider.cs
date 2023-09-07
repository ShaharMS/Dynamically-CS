using System;
using System.Collections.Generic;
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

    public Quadrilateral Subject;
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
            Point p1 = new Point(Subject.joint1.X, Subject.joint1.Y), p2 = new Point(Subject.joint2.X, Subject.joint2.Y), p3 = new Point(Subject.joint3.X, Subject.joint3.Y), p4 = new Point(Subject.joint4.X, Subject.joint4.Y);
            Point rotationCenter = MainWindow.Mouse.GetPosition(null);
            double dist1 = Subject.joint1.DistanceTo(rotationCenter), dist2 = Subject.joint2.DistanceTo(rotationCenter), dist3 = Subject.joint3.DistanceTo(rotationCenter), dist4 = Subject.joint4.DistanceTo(rotationCenter);
            double initialRotationRad = rotationCenter.RadiansTo(MainWindow.Mouse.GetPosition(null));

            double rad1 = rotationCenter.RadiansTo(p1), rad2 = rotationCenter.RadiansTo(p2), rad3 = rotationCenter.RadiansTo(p3), rad4 = rotationCenter.RadiansTo(p4);

            void Move(object? sender, PointerEventArgs args)
            {
                var currentRotation = rotationCenter.RadiansTo(args.GetPosition(null)) - initialRotationRad;
                Subject.joint1.X = rotationCenter.X + dist1 * Math.Cos(rad1 + currentRotation); Subject.joint1.Y = rotationCenter.Y + dist1 * Math.Sin(rad1 + currentRotation);
                Subject.joint2.X = rotationCenter.X + dist2 * Math.Cos(rad2 + currentRotation); Subject.joint2.Y = rotationCenter.Y + dist2 * Math.Sin(rad2 + currentRotation);
                Subject.joint3.X = rotationCenter.X + dist3 * Math.Cos(rad3 + currentRotation); Subject.joint3.Y = rotationCenter.Y + dist3 * Math.Sin(rad3 + currentRotation);
                Subject.joint4.X = rotationCenter.X + dist4 * Math.Cos(rad4 + currentRotation); Subject.joint4.Y = rotationCenter.Y + dist4 * Math.Sin(rad4 + currentRotation);
                Subject.joint1.DispatchOnMovedEvents(); Subject.joint2.DispatchOnMovedEvents(); Subject.joint3.DispatchOnMovedEvents();
            }

            void Finish(object? sender, PointerPressedEventArgs arg)
            {
                MainWindow.Instance.PointerMoved -= Move;
                MainWindow.Instance.PointerPressed -= Finish;
            }

            MainWindow.Instance.PointerMoved += Move;
            MainWindow.Instance.PointerPressed += Finish;
        };


        return rotate;
    }

    MenuItem Defaults_ChangeType()
    {
        var sq = new MenuItem
        {
            Header = "Square " + (Subject.Type == QuadrilateralType.SQUARE ? "✓" : "")
        };
        sq.Click += (s, e) => { Subject.Type = QuadrilateralType.SQUARE; Regenerate(); };

        return new MenuItem
        {
            Header = "Change Type",
            Items = new[] { sq }
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
            Header = $"Remove {Subject.joint1}, {Subject.joint2}, {Subject.joint3} & {Subject.joint4}"
        };
        remove.Click += (sender, e) =>
        {
            foreach (var item in new[] { Subject.joint1, Subject.joint3, Subject.joint4, Subject.joint2 }) item.RemoveFromBoard();
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
