using Avalonia;
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
    public List<Joint> Followers = new();

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


    public Formula()
    {
        OnChange.Add(UpdateFollowers);
    }

    public virtual void AddFollower(Joint joint)
    {
        Followers.Add(joint);
        joint.PositioningByFormula.Add(UpdateJointPosition);
        joint.DispatchOnMovedEvents(joint.X, joint.Y, joint.X, joint.Y);
    }

    public virtual void RemoveFollower(Joint joint)
    {
        Followers.Add(joint);
        joint.PositioningByFormula.Remove(UpdateJointPosition);
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

