using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarboLifeAPI
{
    public class StringToFormula
    {
        private string[] _operators = { "-", "+", "/", "*", "^" };
        private Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2)
    };

        public double Eval(string expression)
        {
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Eval(subExpr));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_operators, token) >= 0)
                {
                    //Equal ranks evaluate left to right, so "100/2*3" is 150 and "1-2+3" is 2; only ^ groups right to left.
                    while (operatorStack.Count > 0 &&
                        (precedence(operatorStack.Peek()) > precedence(token) ||
                         (precedence(operatorStack.Peek()) == precedence(token) && token != "^")))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    //Always invariant: the tokeniser has already normalised any decimal comma to a point,
                    //so an expression means the same thing on an en-GB and an it-IT machine.
                    operandStack.Push(double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture));
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                double arg1 = operandStack.Pop();
                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }

        //The position in _operators used to double as the rank, which put - below + and / below *.
        private static int precedence(string op)
        {
            switch (op)
            {
                case "+":
                case "-":
                    return 1;
                case "*":
                case "/":
                    return 2;
                default:
                    return 3;
            }
        }

        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            //The grammar has no comma operator, so a comma can only ever be a decimal separator typed by
            //a user on a locale that uses one. Normalise it so the parse below can stay invariant.
            foreach (char c in expression.Replace(" ", string.Empty).Replace(',', '.'))
            {
                //The sign of an exponent belongs to the number: .NET writes volumes below 1e-4 as
                //"3.6E-05", and splitting at that '-' left "3.6E", which threw.
                bool exponentSign = (c == '-' || c == '+') && sb.Length > 1 &&
                    (sb[sb.Length - 1] == 'E' || sb[sb.Length - 1] == 'e') && char.IsDigit(sb[sb.Length - 2]);

                if (operators.IndexOf(c) >= 0 && exponentSign == false)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
    }
}
