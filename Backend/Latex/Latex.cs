using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls.Shapes;
using System.Linq.Expressions;
using static AngouriMath.Entity;
using SkiaSharp;

namespace Dynamically.Backend.Latex;

public class Latex
{
    public static List<LatexToken> Lex(string input)
    {
        var tokens = new List<LatexToken>();

        var i = 0;
        while (i < input.Length)
        {
            var ch = input[i].ToString();
            if (ch == " ")
            {
                i++;
                continue;
            }

            if ("1234567890.".Contains(ch))
            {
                var num = ch;
                i++;
                while (i < input.Length && "1234567890.".Contains(input[i]))
                {
                    num += input[i];
                    i++;
                }
                i--;
                if (num == ".") tokens.Add(new Op("."));
                else if (num.EndsWith("."))
                {
                    tokens.Add(new Identifier(num.Substring(0, num.Length - 1)));
                    tokens.Add(new Op("."));
                }
                else tokens.Add(new Identifier(num));

            }
            else if (new Regex(@"\W").Match(ch).Success)
            {
                tokens.Add(new Op(ch));
            }
            else if (new Regex(@"\w").Match(ch).Success)
            {
                var name = ch;
                i++;
                while (i < input.Length && new Regex(@"\w").Match(input[i].ToString()).Success)
                {
                    name += input[i];
                    i++;
                }
                i--;
                tokens.Add(new Identifier(name));
            }
            i++;
        }

        return tokens;
    }

    public static List<LatexToken> Parse(List<LatexToken> pre)
    {
        var post = pre;

        // Multiplication
        post = prettyMultiplication(post);

        // Closures
        post = mergeClosures(post, "(", ")");
        post = mergeClosures(post, "[", "]");
        post = mergeClosures(post, "{", "}");

        // Division
        post = prettyDivision(post);

        return post;

    }

    public static string Latexify(string input) => string.Join(" ", Parse(Lex(input)).Select(x => x.ToString()));
    public static string Debugify(string input) => string.Join(" ", Parse(Lex(input)).Select(x => x.ToDebug()));

    public static List<LatexToken> prettyMultiplication(List<LatexToken> pre) => pre.Select(e => (e is Op op && "×*".Contains(op.Operator)) ? new Identifier("\\cdot") : e).ToList();

    public static List<LatexToken> mergeClosures(List<LatexToken> pre, string _sign, string sign_)
    {
        var post = new List<LatexToken>();

        var i = 0;
        while (i < pre.Count)
        {
            var token = pre[i];
            if (token is Op op && op.Operator == _sign)
            {
                var expressionBody = new List<LatexToken>();
                var expressionStack = 1; // Open and close the block on the correct curly bracket
                while (i + 1 < pre.Count)
                {
                    var lookahead = pre[i + 1];
                    if (lookahead is Op _op && _op.Operator == _sign)
                    {
                        expressionStack++;
                        expressionBody.Add(lookahead);
                    }
                    if (lookahead is Op __op && __op.Operator == sign_)
                    {
                        expressionStack--;
                        if (expressionStack == 0) break;
                        expressionBody.Add(lookahead);
                    }
                    else expressionBody.Add(lookahead);
                    i++;
                }
                post.Add(new Closure(mergeClosures(expressionBody, _sign, sign_), _sign, sign_)); // The check performed above includes unmerged blocks inside the outer block. These unmerged blocks should be merged
                i++;
            }
            else if (token is Closure closure) post.Add(new Closure(mergeClosures(closure.Tokens, _sign, sign_), _sign, sign_));
            else post.Add(token);
            i++;
        }
        return post;
    }

    public static List<LatexToken> prettyDivision(List<LatexToken> pre) 
    { 
        var post = new List<LatexToken>();

        var i = 0;
        while (i < pre.Count)
        {
            var token = pre[i];

            if (token is Op op && "/÷".Contains(op.Operator))
            {
                var lookbehind = post.Last();
                post.Remove(lookbehind);
                if (i + 1 == pre.Count) post.Add(new Division(lookbehind, new Identifier("")));
                else post.Add(new Division(lookbehind, pre[i + 1]));
                i++;
            }
            else if (token is Closure closure) post.Add(new Closure(prettyDivision(closure.Tokens), closure.Parenthesis.open, closure.Parenthesis.close));
            else post.Add(token);
            i++;
        }

        return post;
    }
}
