using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;
#pragma warning disable CS8602
#pragma warning disable CS8604
public class RoleMap
{

    public static Dictionary<Role, List<object>> QuickCreateMap(params (Role, dynamic[])[] elements)
    {
        var m = new Dictionary<Role, List<object>>();
        foreach (var element in elements)
        {
            m[element.Item1] = element.Item2.ToList();
        }
        return m;
    }

    public Dictionary<Role, List<object>> underlying = new();

    public Dictionary<Role, List<object>>.Enumerator GetEnumerator()
    {
        return underlying.GetEnumerator();
    }

    public Joint Subject;

    public int Count { get; private set; }
    public List<object> this[Role role]
    {
        get => Access<object>(role);
        set
        {
            if (!Has(role) && value.Count > 0) Count++;
            else if (Has(role) && value.Count == 0) Count--;
            underlying[role] = value;
        }
    }

    public RoleMap(Joint subject) : base()
    {
        Subject = subject;
        Count = 0;
    }

    public List<T> Access<T>(Role role)
    {
        if (underlying.ContainsKey(role)) return underlying[role].Cast<T>().ToList();
        else
        {
            underlying[role] = new List<object>();
            return underlying[role].Cast<T>().ToList();
        }
    }

    public T Access<T>(Role role, int index)
    {
        return Access<T>(role).ElementAt(index);
    }

    public bool Has(Role role)
    {
        return Access<object>(role).Count > 0;
    }

    public bool Has(Role role, object item)
    {
        return Access<object>(role).Contains(item);
    }

    public bool Has(Dictionary<Role, List<object>> group)
    {
        foreach (var pair in group)
        {
            if (!Has(pair.Key) && pair.Value.Count > 0) return false;
            foreach (var item in pair.Value)
            {
                if (!Has(pair.Key, item)) return false;
            }
        }

        return true;
    }

    public int CountOf(Role role)
    {
        return Access<object>(role).Count;
    }



    public List<T> ClearRole<T>(Role role)
    {
        List<T> list = new();
        foreach (var item in Access<T>(role))
        {
            list.Add(RemoveFromRole(role, item));
        }
        Count--;
        return list;
    }

    public void Clear()
    {
        foreach (var role in underlying.Keys)
        {
            ClearRole<object>(role);
        }
    }

    public T AddToRole<T>(Role role, T item)
    {
        if (Has(role, item)) return item;
        if (underlying.ContainsKey(role)) underlying[role].Add(item);
        else underlying[role] = new List<object> { item };
        if (underlying[role].Count == 1) Count++;

        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).AddFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnMoved.Add(s1.__updateFormula);
                Subject.OnDragged.Add(s1.__reposition);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.AddFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Add((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }

        return item;
    }
    public T RemoveFromRole<T>(Role role, T item)
    {
        var list = Access<T>(role);
        if (list.Count == 0 || !list.Contains(item)) return item;
        if (list.Count == 1) Count--;
        underlying[role].Remove(item);
        switch (role)
        {
            // Ray
            case Role.RAY_On:
                (item as RayFormula).AddFollower(Subject);
                break;
            // Segment
            case Role.SEGMENT_Corner:
                var s1 = item as Segment;
                Subject.OnMoved.Remove(s1.__updateFormula);
                Subject.OnDragged.Remove(s1.__reposition);
                break;
            // Circle
            case Role.CIRCLE_On:
                (item as Circle).Formula.RemoveFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                var circ = item as Circle;
                circ.center.OnMoved.Remove(circ.__circle_OnChange);
                break;
            // Triangle
            case Role.TRIANGLE_Corner:
                Subject.OnRemoved.Remove((_, _) => (item as Triangle).Dismantle());
                break;
            default: break;
        }

        return item;
    }
}
#pragma warning restore CS8602
#pragma warning restore CS8604
