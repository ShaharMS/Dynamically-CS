using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsBackend
{
    interface IDrawable
    {
        public void reposition();
    }

    public class DraggableGraphic : Canvas
    {
        public bool draggable = true;
        public bool currentlyDragging;
        private Point _startPosition;
        private Point _startMousePosition;
        public List<Action<double, double, double, double>> onMoved = new List<Action<double, double, double, double>>();
        public List<Action<double, double, double, double>> onDragged = new List<Action<double, double, double, double>>();

        public static readonly StyledProperty<double> XProperty =
            AvaloniaProperty.Register<DraggableGraphic, double>(nameof(x));

        public double x
        {
            get => GetValue(XProperty);
            set
            {
                SetLeft(this, value);
                //Margin = new Thickness(value, Margin.Top, 0, 0);
                SetValue(XProperty, value);
            }
        }

        public static readonly StyledProperty<double> YProperty =
            AvaloniaProperty.Register<DraggableGraphic, double>(nameof(y));

        public double y
        {
            get => GetValue(YProperty);
            set 
            {
                SetTop(this, value);
                //Margin = new Thickness(Margin.Left, value, 0, 0);
                SetValue(YProperty, value);
            }
        }

        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            if (draggable) Cursor = new Cursor(StandardCursorType.SizeAll);
            else Cursor = new Cursor(StandardCursorType.No);
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerEnter(e);
            Cursor = new Cursor(StandardCursorType.Arrow);

        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (draggable && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                currentlyDragging = true;
                _startPosition = new Point(x, y);
                _startMousePosition = e.GetPosition(null);
                e.Pointer.Capture(this);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (currentlyDragging && e.Pointer.Captured == this)
            {
                currentlyDragging = false;
                e.Pointer?.Capture(null);

                var currentPosition = e.GetPosition(null);
                var endX = x + (currentPosition.X - _startMousePosition.X);
                var endY = y + (currentPosition.Y - _startMousePosition.Y);

                foreach (var listener in onDragged)
                {
                    listener(_startPosition.X, _startPosition.Y, endX, endY);
                }
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (currentlyDragging && e.Pointer.Captured == this)
            {
                var currentPosition = e.GetPosition(null);
                var offset = currentPosition - _startMousePosition;
                x = _startPosition.X + offset.X;
                y = _startPosition.Y + offset.Y;

                foreach (var listener in onMoved)
                {
                    listener(x, y, currentPosition.X, currentPosition.Y);
                }
            }
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Define a brush and pen for drawing
            var brush = new SolidColorBrush(Colors.Blue);
            var pen = new Pen(brush, 2);

            // Create a rectangle to draw based on X and Y properties
            var rect = new Rect(new Point(x, y), new Size(100, 100));

            // Draw the rectangle
            context.DrawRectangle(brush, pen, rect);
        }
    }
}
