using Antlr4.Runtime;
using CSharpMath.Atom.Atoms;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend.Latex;

public class LatexToken
{
    protected dynamic _val = new ExpandoObject();
    public dynamic Value { get => _val; protected set => _val = value; }
    public string Name { get; protected set; }
    public LatexToken() 
    {
        Name = "LatexToken";
        _val.Name = Name;
        _val.Value = null;
    }

    public override string ToString() => $"{Name}({Value})";
    public virtual string ToDebug() => $"{Name}({Value})";
}

public class NewLine : LatexToken
{
    public NewLine()
    {
        Name = GetType().Name;
        _val.Name = Name;
    }

    public void Deconstruct(out string n) {n = Name; }

    public override string ToString() => @"\\";
    public override string ToDebug() => $"{Name}";

}

public class Identifier : LatexToken
{
    public readonly string Ident;
    public Identifier(string ident)
    {
        Ident = ident;
        Name = GetType().Name;
        _val.Name = Name;
        _val.Value = Ident;
    }

    public void Deconstruct(out string id, out string n) { id = Ident; n = Name; }
    public void Deconstruct(out string id) { id = Ident; }

    public override string ToString() => Ident;
    public override string ToDebug() => $"{Name}({Ident})";

}

public class Op : LatexToken
{
    public readonly string Operator;
    public Op(string op)
    {
        Operator = op.Trim();
        Name = GetType().Name;
        _val.Name = Name;
        _val.Value = Operator;
    }

    public void Deconstruct(out string p, out string n) { p = Operator; n = Name; }
    public void Deconstruct(out string p) { p = Operator; }
    public override string ToString() => Operator;
    public override string ToDebug() => $"{Name}({Operator})";

}
public class Closure : LatexToken
{
    public readonly List<LatexToken> Tokens;
    public (string open, string close) Parenthesis;
    public Closure(List<LatexToken> tokens, string s, string e)
    {
        Tokens = tokens;
        Parenthesis = (s, e);
        Name = GetType().Name;
        _val.Name = Name;
        _val.Value = Tokens;
    }

    public void Deconstruct(out List<LatexToken> id, out (string, string) p, out string n) { id = Tokens; p = Parenthesis;  n = Name; }
    public void Deconstruct(out List<LatexToken> id, out (string, string) p) { id = Tokens; p = Parenthesis; }
    public void Deconstruct(out List<LatexToken> id) { id = Tokens; }
    public void Deconstruct(out (string, string) p) { p = Parenthesis; }
    public override string ToString() => $"{Parenthesis.open}{string.Join("", Tokens.Select(e => e.ToString()))}{Parenthesis.close}";
    public string ToString(bool stripped = true) => stripped ? $"{string.Join("", Tokens.Select(e => e.ToString()))}" : ToString();
    public override string ToDebug() => $"{Name}([{Log.StringifyCollection(Tokens.Select(x => x.ToDebug()))}])";

}

public class Division : LatexToken
{
    public readonly LatexToken Lhs;
    public readonly LatexToken Rhs;

    public Division(LatexToken lhs, LatexToken rhs)
    {
        Name = GetType().Name;
        Lhs = lhs;
        Rhs = rhs;
        _val.Rhs = Rhs;
        _val.Lhs = Lhs;
    }

    public void Deconstruct(out LatexToken lhs, out LatexToken rhs, out string n) { lhs = Lhs; rhs = Rhs; n = Name; }
    public void Deconstruct(out LatexToken lhs, out LatexToken rhs) { lhs = Lhs; rhs = Rhs; }
    public override string ToString() => $"\\frac{{{(Lhs is Closure closure ? closure.ToString(true) : Lhs)}}}{{{(Rhs is Closure _closure ? _closure.ToString(true) : Rhs)}}}";
    public override string ToDebug() => $"{Name}({Lhs}, {Rhs})";

}