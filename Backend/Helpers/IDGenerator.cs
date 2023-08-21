using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Helpers;

public class IDGenerator
{
    static readonly List<Joint> ids = new();

    public static void GenerateFor(Joint j)
    {
        char letter = 'A';
        bool changed;
        do
        {
            changed = false;
            foreach (var joint in ids)
            {
                if (joint.Id == letter)
                {
                    changed = true;
                    letter++;
                    break;
                }
            }

        } while (changed);
        j.Id = letter;
        ids.Add(j);
    }

    public static void Remove(Joint j)
    {
        ids.Remove(j);
    }

    public static bool Has(char id)
    {
        foreach (var joint in ids)
        {
            if (joint.Id == id) return true;
        }
        return false;
    }
}
