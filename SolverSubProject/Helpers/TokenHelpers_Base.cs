using Dynamically.Backend;
using Dynamically.Solver.Details;
using Dynamically.Solver.Information.BuildingBlocks;
using SolverSubProject.Information;
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
            if (detail.Operator == Relation.EQUALS && (detail.Left == token)) return (TValue)detail.Right;
            if (detail.Operator == Relation.EQUALS && (detail.Right == token)) return (TValue)detail.Left;
        }

        var val = new TValue(token.Id)
        {
            ParentPool = token.ParentPool
        };

        return val;
    }
    public static Detail? GetValueDetail(this ExerciseToken token)
    {
        foreach (var detail in token.ParentPool.AvailableDetails)
        {
            if (detail.Operator == Relation.EQUALS && (detail.Left == token)) return detail;
            if (detail.Operator == Relation.EQUALS && (detail.Right == token)) return detail;
        }

        return null;
    }

    /// <summary>
    /// Gets/creates a detail that equates between two values, if they are equal.
    /// returns either generic EQUALs or transitivity details
    /// </summary>
    /// <param name="token"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Detail GetEqualityDetail(this ExerciseToken token1, ExerciseToken token2) {
        Validate(token1, token2);
        var details = token1.ParentPool.AvailableDetails;
        List<(Detail, IEnumerable<TValue>)> token1Equalities = new(), token2Equalities = new();
        foreach (var detail in details.GetMany(Relation.EQUALS)) {
            if (detail.IncludedElements.ContainsSome(token1.GetValue(), new TValue(token1.Id)) && detail.IncludedElements.ContainsSome(token2.GetValue(), new TValue(token2.Id)))
                return detail;
            if (detail.IncludedElements.ContainsSome(token1.GetValue(), new TValue(token1.Id))) {
                token1Equalities.Add((detail, detail.IncludedElements.Except(token1.GetValue(), new TValue(token1.Id)).Cast<TValue>()));
            }
            if (!detail.IncludedElements.ContainsSome(token2.GetValue(), new TValue(token2.Id)))
            {
                token2Equalities.Add((detail, detail.IncludedElements.Except(token2.GetValue(), new TValue(token2.Id)).Cast<TValue>()));
            }
        }

        foreach (var (detail1, values1) in token1Equalities)
        {
            foreach (var (detail2, values2) in token2Equalities)
            {
                if (detail1 == detail2) throw new Exception("Details which reference two exclusive values should not be equal");
                if (values1.Intersect(values2).Any()) return token1.EqualsVal(token2).MarkReasonExplicit(Reason.TRANSITIVITY).AddReferences(detail1, detail2);
            }
        }
        throw new ArgumentException($"Equality detail requested, but not found. are {token1} and {token1} equal?");
    }

    public static bool IsNull(this ExerciseToken token) => token == ExerciseToken.Null;
}