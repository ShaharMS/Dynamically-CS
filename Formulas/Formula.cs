using Avalonia;
using Dynamically.Backend;
using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Formulas;

public abstract class Formula
{
    public List<Action> OnChange = new();
    public List<Action<double, double, double, double>> OnMoved = new();
    public List<Vertex> Followers = new();

    public bool queueRemoval = false;
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
        OnMoved.Add((curX, curY, preX, preY) => {
            foreach (var joint in Followers) {
                // If a formula moves an element encapsulated within the current selection,
                // We get double movement. to prevent this:
                if (MainWindow.BigScreen.Selection?.EncapsulatedElements.Contains(joint) ?? false) continue;
                joint.X = joint.X - preX + curX;
                joint.Y = joint.Y - preY + curY;
                joint.DispatchOnMovedEvents(joint.X, joint.Y, joint.X + preX - curX, joint.Y + preY - curY);
            }
        });
        OnChange.Add(UpdateFollowers);
    }

    public virtual void AddFollower(Vertex joint)
    {
        Followers.Add(joint);
        joint.PositioningByFormula.Add(UpdateJointPosition);
        joint.DispatchOnMovedEvents(joint.X, joint.Y, joint.X, joint.Y);
    }

    public virtual void RemoveFollower(Vertex joint)
    {
        Followers.Remove(joint);
        joint.PositioningByFormula.Remove(UpdateJointPosition);
    }

    public virtual void RemoveAllFollowers()
    {
        var c = Followers.ToArray();
        foreach (var f in c) RemoveFollower(f);
    }

    public virtual (double X, double Y) UpdateJointPosition(double inputX, double inputY)
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
        foreach(var joint in Followers)
        {
            joint.DispatchOnMovedEvents(joint.X, joint.Y, joint.X, joint.Y);
        }
    }

}

