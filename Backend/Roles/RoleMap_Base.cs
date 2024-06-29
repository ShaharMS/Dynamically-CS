using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Geometry.Basics;

namespace Dynamically.Backend.Roles;
#pragma warning disable CS8602
#pragma warning disable CS8604
public partial class RoleMap
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

    public Dictionary<Role, List<object>> Underlying = new();

    public Dictionary<Role, List<object>>.Enumerator GetEnumerator()
    {
        return Underlying.GetEnumerator();
    }

    public Either<Vertex, Circle /*May exist later*/, Segment> Subject;

    public int Count { get; set; }
    public List<object> this[Role role]
    {
        get => Access<object>(role);
        set
        {
            if (!Has(role) && value.Count > 0) Count++;
            else if (Has(role) && value.Count == 0) Count--;
            Underlying[role] = value;
        }
    }

    public RoleMap(Either<Vertex, Circle, Segment> subject) : base()
    {
        Subject = subject;
        Count = 0;
    }

    public List<T> Access<T>(Role role)
    {
        if (Underlying.ContainsKey(role)) return Underlying[role].Cast<T>().ToList();
        else
        {
            Underlying[role] = new List<object>();
            return Underlying[role].Cast<T>().ToList();
        }
    }

    public T Access<T>(Role role, int index)
    {
        return Access<T>(role).ElementAt(index);
    }

    public bool Has(Role role)
    {
        return Access<dynamic>(role).Count > 0;
    }

    public bool Has(params Role[] roles)
    {
        var count = 0;
        foreach (var role in roles) count += Access<dynamic>(role).Count;
        return count > 0;
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

    public bool Has(ITuple roles, object item)
    {
        var result = false;
        var arr = new Role[roles.Length];
        for (int i = 0; i < roles.Length; i++)
        {
            arr[i] = (Role?)roles[i] ?? Role.Null;
        }
        foreach (var role in arr)
        {
            result |= Has(role, item);
        }

        return result;
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
        foreach (var role in Underlying.Keys)
        {
            ClearRole<object>(role);
        }
    }

    public T AddToRole<T>(Role role, T item)
    {
        if (Has(role, item)) return item;
        if (Underlying.ContainsKey(role)) Underlying[role].Add(item);
        else Underlying[role] = new List<object> { item };
        if (Underlying[role].Count == 1) Count++;

        if (Subject.Is<Vertex>())  Vertex__AddToRole(role, item, Subject.L());
        else Segment__AddToRole(role, item, Subject.R());

        return item;
    }
    public T RemoveFromRole<T>(Role role, T item)
    {
        var list = Access<T>(role);
        if (list.Count == 0 || !list.Contains(item)) return item;
        if (list.Count == 1) Count--;
        Underlying[role].Remove(item);

        if (Subject.Is<Vertex>()) Vertex__RemoveFromRole(role, item, Subject.L());
        else Segment__RemoveFromRole(role, item, Subject.R());
        

        return item;
    }

    public RoleMap TransferFrom(RoleMap Roles)
    {
        if (Roles == null) return this;
        foreach (var (role, items) in Roles)
        {
            var c = items.ToArray();
            foreach (var item in c)
            {
                // Counting & Defining

                // Remove from given:
                var list = Roles.Access<dynamic>(role);
                if (list.Count == 0 || !list.Contains(item)) continue;
                if (list.Count == 1) Roles.Count--;
                Roles.Underlying[role].Remove(item);

                // Add to this:
                if (Has(role, item)) continue;
                if (Underlying.ContainsKey(role)) Underlying[role].Add(item);
                else Underlying[role] = new List<object> { item };
                if (Underlying[role].Count == 1) Count++;

                if (Subject.Is<Vertex>()) Vertex__TransferRole(Roles.Subject.L(), role, item, Subject.L());
                else Segment__TransferRole(Roles.Subject.R(), role, item, Subject.R());
            }
        }

        return this;
    }
}
#pragma warning restore CS8602
#pragma warning restore CS8604
