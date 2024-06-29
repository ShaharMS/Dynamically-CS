using Dynamically.Backend.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamically.Geometry.Basics;


namespace Dynamically.Backend.Helpers;

public class IDGenerator
{
    static readonly List<Vertex> ids = new();

    public static void GenerateFor(Vertex j)
    {
        char letter = 'A';
        bool changed;
        do
        {
            changed = false;
            foreach (var vertex in ids)
            {
                if (vertex.Id == letter)
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

    public static void Remove(Vertex j)
    {
        ids.Remove(j);
    }

    public static bool Has(char id)
    {
        foreach (var vertex in ids)
        {
            if (vertex.Id == id) return true;
        }
        return false;
    }
}
