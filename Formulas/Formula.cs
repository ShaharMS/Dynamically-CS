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
    public virtual Point? GetClosestOnFormula(Point point) {
        return GetClosestOnFormula(point.X, point.Y);
    }

    public abstract void Move(double x, double y);
    public virtual void Move(Point point) {
        Move(point.X, point.Y);
    }

    public virtual void AddFollower(Joint joint)
    {
        Followers.Add(joint);
        joint.OnMoved.Add((double _, double _, double _, double _) => UpdateFollowers());
        UpdateFollowers();
    }

    public virtual void RemoveFollower(Joint joint)
    {
        Followers.Add(joint);
        joint.OnMoved.Remove((double _, double _, double _, double _) => UpdateFollowers());
    }

    public virtual void UpdateFollowers() 
    {
        foreach (var follower in Followers)
        {
            var pos = GetClosestOnFormula(follower);
            if (pos == null) continue;
            follower.X = pos.Value.X;
            follower.Y = pos.Value.Y;

            follower.reposition();
        }
    }
}

