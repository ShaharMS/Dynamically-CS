using Avalonia;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
using Dynamically.Backend.Helpers;
using Dynamically.Backend.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Formulas;

public abstract class Formula
{
    public List<Action> OnChange = new();
    public List<Action<double, double, double, double>> OnMoved = new();
    public List<ICanFollowFormula> Followers = new();

    public bool QueueRemoval = false;
    public abstract double[] SolveForX(double y);
    public abstract double[] SolveForY(double x);

    public abstract Point? GetClosestOnFormula(double x, double y);
    public virtual Point? GetClosestOnFormula(Point point)
    {
        return GetClosestOnFormula(point.X, point.Y);
    }

    public abstract void Move(double x, double y);
    public virtual void Move(Point point)
    {
        Move(point.X, point.Y);
    }

    public virtual double DistanceTo(Point p)
    {
        return GetClosestOnFormula(p) != null ? p.DistanceTo(GetClosestOnFormula(p) ?? /* The following point never gets through, no worries */new Point(double.NaN, double.NaN)) : double.NaN;
    }
    public double DistanceTo(double X, double Y)
    {
        return DistanceTo(new Point(X, Y));
    }

    public Formula()
    {
        OnMoved.Add((curX, curY, preX, preY) =>
        {
            foreach (var obj in Followers)
            {
                // If a formula moves an element encapsulated within the Instance selection,
                // We get double movement. to prevent this:
                if (obj is DraggableGraphic draggable && (draggable.ParentBoard.Selection?.EncapsulatedElements.Contains(draggable) ?? false)) continue;
                obj.X = obj.X - preX + curX;
                obj.Y = obj.Y - preY + curY;
                obj.DispatchOnMovedEvents(obj.X + preX - curX, obj.Y + preY - curY);
            }
        });
        OnChange.Add(UpdateFollowers);
    }

    public virtual void AddFollower(ICanFollowFormula obj)
    {
        Followers.Add(obj);
        obj.PositioningByFormula.Add(UpdateVertexPosition);
        obj.DispatchOnMovedEvents();
    }

    public virtual void RemoveFollower(ICanFollowFormula obj)
    {
        Followers.Remove(obj);
        obj.PositioningByFormula.Remove(UpdateVertexPosition);
    }

    public virtual void RemoveAllFollowers()
    {
        var c = Followers.ToArray();
        foreach (var f in c) RemoveFollower(f);
    }

    public virtual (double X, double Y) UpdateVertexPosition(double inputX, double inputY)
    {
        var X = inputX; var Y = inputY;
        var pos = GetClosestOnFormula(inputX, inputY);
        if (pos == null) return (X, Y);
        X = pos.Value.X;
        Y = pos.Value.Y;

        return (X, Y);
    }

    public virtual void UpdateFollowers()
    {
        foreach(var obj in Followers.ToList())
        {
            obj.DispatchOnMovedEvents();
        }
    }
}

