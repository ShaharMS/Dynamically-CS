using Dynamically.Backend.Geometry;
using Dynamically.Backend.Helpers;
using Dynamically.Formulas;
using Dynamically.Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;
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

    public Dictionary<Role, List<object>> underlying = new();

    public Dictionary<Role, List<object>>.Enumerator GetEnumerator()
    {
        return underlying.GetEnumerator();
    }

    public Either<Joint, Segment> Subject;

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

    public RoleMap(Either<Joint, Segment> subject) : base()
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

        if (Subject.Is<Joint>())  Joint__AddToRole(role, item, Subject.L());
        else Segment__AddToRole(role, item, Subject.R());

        return item;
    }
    public T RemoveFromRole<T>(Role role, T item)
    {
        var list = Access<T>(role);
        if (list.Count == 0 || !list.Contains(item)) return item;
        if (list.Count == 1) Count--;
        underlying[role].Remove(item);

        if (Subject.Is<Joint>()) Joint__RemoveFromRole(role, item, Subject.L());
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
                Roles.underlying[role].Remove(item);

                // Add to this:
                if (Has(role, item)) continue;
                if (underlying.ContainsKey(role)) underlying[role].Add(item);
                else underlying[role] = new List<object> { item };
                if (underlying[role].Count == 1) Count++;

                if (Subject.Is<Joint>()) Joint__TransferRole(Roles.Subject.L(), role, item, Subject.L());
                else Segment__TransferRole(Roles.Subject.R(), role, item, Subject.R());
            }
        }

        return this;
    }
}
#pragma warning restore CS8602
#pragma warning restore CS8604
