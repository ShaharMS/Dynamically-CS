using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Interfaces;

public interface ICanFollowFormula
{
    /// <summary>
    /// The X position at which the object resides.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// The Y position at which the object resides.
    /// </summary>
    public double Y { get; set; }


    /// <summary>
    /// Upon object movement, this list of functions will be called to evaluate the new position of the object.
    /// 
    /// For different types of objec this may be handled differently.
    /// </summary>
    public List<Func<double, double, (double X, double Y)>> PositioningByFormula { get; }

    /// <summary>
    /// A function to handle behavior upon object movement. Should use <c>PositioningByFormula</c> to evaluate the new position.
    /// </summary>
    /// <param name="px"> The previous X position. Defaults to <c>obj.X</c>.</param>
    /// <param name="py"> The previous Y position. Defaults to <c>obj.Y</c>.</param>
    public void DispatchOnMovedEvents(double? px = null, double? py = null);
}
