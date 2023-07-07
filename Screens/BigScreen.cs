using Avalonia.Controls;
using Avalonia.Media;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Screens;

public class BigScreen : DraggableGraphic
{
    public DraggableGraphic FocusedObject;


    public BigScreen() : base()
    {
        FocusedObject = this;
        Draggable = false;
    }

    public override void Render(DrawingContext context)
    {
        //base.Render(context);
    }
}
