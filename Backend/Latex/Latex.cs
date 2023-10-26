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
            if (" \r".Contains(ch))
            {
                i++;
                continue;
            }
            if (ch == "\n")
            {
                tokens.Add(new NewLine());
            }
            else if ("1234567890.".Contains(ch))
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
            else if (ch == "\\" || new Regex(@"\w").Match(ch).Success)
            {
                var name = ch;
                i++;
                if (i < input.Length && input[i] == '\\')
                {
                    tokens.Add(new NewLine());
                    i--;
                }
                else
                {
                    while (i < input.Length && new Regex(@"\w").Match(input[i].ToString()).Success)
                    {
                        name += input[i];
                        i++;
                    }
                    i--;
                    tokens.Add(new Identifier(name));
                }
            }
            else if (new Regex(@"\W").Match(ch).Success)
            {
                tokens.Add(new Op(ch));
            }
            i++;
        }

        Log.Write(tokens.Select(x => x.ToDebug()).ToList());

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
                    else if (lookahead is Op __op && __op.Operator == sign_)
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
            else if (token is Closure closure) post.Add(new Closure(mergeClosures(closure.Tokens, _sign, sign_), closure.Parenthesis.open, closure.Parenthesis.close));
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
                LatexToken lookbehind;
                if (post.Count == 0) lookbehind = new Closure(new List<LatexToken>(), "(", ")");
                else
                {
                    lookbehind = post.Last();
                    post.Remove(lookbehind);
                }
                if (i + 1 == pre.Count) post.Add(new Division(lookbehind, new Identifier("")));
                else
                {
                    var lookahead = pre[i + 1];
                    if (lookahead is Closure closure) lookahead = new Closure(prettyDivision(closure.Tokens), closure.Parenthesis.open, closure.Parenthesis.close);
                    post.Add(new Division(lookbehind, lookahead));
                }
                i++;
            }
            else if (token is Closure closure) post.Add(new Closure(prettyDivision(closure.Tokens), closure.Parenthesis.open, closure.Parenthesis.close));
            else post.Add(token);
            i++;
        }

        return post;
    }
}
