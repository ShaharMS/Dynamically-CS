﻿using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Solver.Helpers;

public static partial class TokenHelpers
{
    /// <summary>
    /// Checks whether or not operations on the given tokens "make sense":
    /// <list type="bullet">
    ///     <item>Tokens must share the same <c>InfoPool</c> instance</item>
    ///     <item>Tokens must be distinct</item>
    /// </list>
    /// </summary>
    /// <param name="tokens"></param>
    public static void Validate(params ExerciseToken[] tokens)
    {
        var c = tokens.Where(x => x.ParentPool == tokens[0].ParentPool).Count();
        if (c != tokens.Length)
        {
            throw new ArgumentException($"Some of the tokens given don't share the same InfoPool. Tokens:\n\t{tokens}");
        }

        if (tokens.ToHashSet().Count != tokens.Length)
        {
            throw new ArgumentException($"{tokens.Length - tokens.ToHashSet().Count} of the tokens given are not distinct. Tokens:\n\t{tokens}");
        }
    }

    public static TValue GetValue(this ExerciseToken token)
    {
        foreach (var detail in token.ParentPool.AvailableDetails)
        {
            if (detail.Operator == Relation.EQUALS && detail.Left == token) return (TValue)detail.Right;
        }

        var val = new TValue(token.Id)
        {
            ParentPool = token.ParentPool
        };

        return val;
    }

    public static bool IsNull(this ExerciseToken token) => token == ExerciseToken.Null;
}