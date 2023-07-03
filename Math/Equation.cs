using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamically.Math;

class Equation
{
    public string equation = "";
    public Dictionary<string, FloatingPoint> parameters = new Dictionary<string, FloatingPoint>();

    public SymbolicExpression expression;

    public readonly EquationType type;

    /// <summary>
    /// The given equation must contain x & y.
    /// Each parameter's value must be specified either here or by adding to <code>this.parameters</code>
    /// </summary>
    /// <param name="equation"></param>
    /// <param name="type"></param>
    /// <param name="parameters"></param>
    public Equation(string equation, Dictionary<string, FloatingPoint> parameters)
    {
        this.equation = equation;
        expression = SymbolicExpression.Parse(equation);
        this.type = type;
        this.parameters = parameters;
    }

    public double SolveForX(double yValue)
    {
        parameters.Add("Y", yValue);
        var paramsAssigned = SymbolicExpression.Parse(equation);
        foreach (var item in parameters)
        {
            paramsAssigned.Substitute(SymbolicExpression.Parse(item.Key), SymbolicExpression.Parse("" + item.Value.RealValue));
        }

        // Todo - doesnt work
        //return SolveSimpleRoot(Expression.Symbol("x"), paramsAssigned.Expression;
        return -1;
    }
/*
    private Expression SolveSimpleRoot(Expression variable, Expression expr)
    {
        // try to bring expression into polynomial form
        Expression simple = Algebraic.Expand(Rational.Numerator(Rational.Simplify(variable, expr)));

        // extract coefficients, solve known forms of order up to 1
        Expression[] coeff = Polynomial.Coefficients(variable, simple);
        switch (coeff.Length)
        {
            case 1: return Expression.Zero.Equals(coeff[0]) ? variable : Expression.Undefined;
            case 2: return Rational.Simplify(variable, Algebraic.Expand(-coeff[0] / coeff[1]));
            default: return Expression.Undefined;
        }
    }*/
}

enum EquationType
{
    Polynomial,
    Algebraic,
    Exponential,
    Trigonometric
}