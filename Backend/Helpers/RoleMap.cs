using Dynamically.Backend.Geometry;
using Dynamically.Backend.Graphics;
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
public class RoleMap : IEnumerable<KeyValuePair<Role, List<object>>>
{
    public Dictionary<Role, List<object>> underlying = new();

    public Joint Subject;

    public RoleMap(Joint subject) : base()
    {
        Subject = subject;
    }

    public List<T> Access<T>(Role role)
    {
        if (underlying.ContainsKey(role)) return underlying[role] as List<T> ?? new List<T>();
        else
        {
            underlying[role] = new List<object>();
            return underlying[role] as List<T> ?? new List<T>();
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

    public T RemoveFromRole<T>(Role role, T item)
    {
        var list = Access<T>(role);
        if (list.Count == 0 || !list.Contains(item)) return item; 
        switch (role)
        {
            case Role.SEGMENT_Corner:
                var s1 = item as Connection;
                Subject.OnMoved.Remove(s1.__updateFormula);
                Subject.OnDragged.Remove(s1.__reposition);
                break;

            case Role.CIRCLE_Contact:
                (item as Circle).Formula.RemoveFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                var circ = item as Circle;
                circ.center.OnMoved.Remove(circ.__circle_OnChange);
                break;
            default: break;
        }

        return item;
    }

    public List<T> ClearRole<T>(Role role)
    {
        List<T> list = new List<T>();
        foreach (var item in Access<T>(role))
        {
            list.Add(RemoveFromRole(role, item));
        }

        return list;
    }

    public T AddToRole<T>(Role role, T item)
    {
        if (Has(role ,item)) return item;
        if (underlying.ContainsKey(role)) underlying[role].Add(item);
        else
        {
            underlying[role] = new List<object> { item };
        }

        switch (role)
        {
            case Role.SEGMENT_Corner:
                var s1 = item as Connection;
                Subject.OnMoved.Add(s1.__updateFormula);
                Subject.OnDragged.Add(s1.__reposition);
                break;

            case Role.CIRCLE_Contact:
                (item as Circle).Formula.AddFollower(Subject);
                break;
            case Role.CIRCLE_Center:
                Subject.OnMoved.Add((item as Circle).__circle_OnChange);
                break;
            default: break;
        }

        return item;
    }

    public IEnumerator<KeyValuePair<Role, List<object>>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<Role, List<object>>>)underlying).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)underlying).GetEnumerator();
    }

    public List<object> this[Role role]
    {
        get => Access<object>(role);
        set => underlying[role] = value;
    }
}
#pragma warning restore CS8602
#pragma warning restore CS8604
