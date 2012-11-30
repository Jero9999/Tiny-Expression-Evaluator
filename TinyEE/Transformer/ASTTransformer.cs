using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable PossibleNullReferenceException
namespace TinyEE
{
    internal static class Expressions
    {
        internal static class Create
        {
            


        }
    }

    /// <summary>
    /// Transform the parsed tree into an abstract syntax tree (AST)
    /// </summary>
    internal class Transformer
    {
        private readonly IDictionary<string, Tuple<ParameterExpression,object>> _params;
        private Func<string, object> _resolver;

        public Transformer(Func<string,object> resolver)
        {
            _resolver = resolver;
            _params = new Dictionary<string, Tuple<ParameterExpression, object>>();
        }

        internal Expression GetAST(ParseNode node)
        {
            Expression result;
            var children = node.Nodes;
            switch (node.Token.Type)
            {
                case TokenType.OrExpression:
                case TokenType.AndExpression:
                case TokenType.Addition:
                case TokenType.Multiplication:
                    result = children.Count >= 3
                                 ? GetBinaryAST(children, children.Count - 1)
                                 : GetInnerAST(children);
                    break;
                case TokenType.Power:
                    result = children.Count >= 3
                                 ? GetPowerAST(children, children.Count - 1)
                                 : GetInnerAST(children);
                    break;
                case TokenType.Compare:
                    result = children.Count >= 3
                                 ? GetCompareAST(children, children.Count - 1)
                                 : GetInnerAST(children);
                    break;
                case TokenType.CoalesceExpression:
                    result = children.Count >= 3
                                 ? GetIfNullThenAST(children, children.Count - 1)
                                 : GetInnerAST(children);
                    break;
                case TokenType.Negation:
                case TokenType.NotExpression:
                    result = children.Count == 2
                                 ? GetUnaryAST(node, children[1])
                                 : GetInnerAST(children);
                    break;
                case TokenType.Start:
                case TokenType.Expression:
                case TokenType.Base:
                case TokenType.Literal:
                case TokenType.IndexAccess://has 2 childs, but uses the first one only
                    result = GetInnerAST(children);
                    break;
                case TokenType.ListLiteral:
                    result = GetListAST(children);
                    break;
                case TokenType.HashLiteral:
                    result = GetHashAST(children);
                    break;
                case TokenType.INTRANGE:
                    result = GetRangeAST(node);
                    break;
                case TokenType.INTEGER:
                    //TODO: big integer
                    result = Expression.Constant(Int32.Parse(node.Token.Text), typeof(int));
                    break;
                case TokenType.DECIMAL:
                    //TODO: big decimal
                    result = Expression.Constant(Double.Parse(node.Token.Text), typeof(double));
                    break;
                case TokenType.STRING:
                    var nodeText = node.Token.Text;
                    Debug.Assert(nodeText.Length >= 2 && nodeText.StartsWith("\"") && nodeText.EndsWith("\""));
                    nodeText = nodeText.Substring(1, nodeText.Length - 2).Replace("\\\"", "\"");
                    result = Expression.Constant(nodeText, typeof(string));
                    break;
                case TokenType.TRUE:
                    result = Expression.Constant(true, typeof(bool));
                    break;
                case TokenType.FALSE:
                    result = Expression.Constant(false, typeof(bool));
                    break;
                case TokenType.ConditionalExpression:
                    result = children.Count == 5 
                                ? GetIfThenElseAST(children[0], children[2], children[4]) 
                                : GetInnerAST(children);
                    break;
                case TokenType.Group:
                    Debug.Assert(children.Count == 3);
                    result = GetAST(children[1]);
                    break;
                case TokenType.Variable:
                    var variableName = children[0].Token.Text;
                    result = GetVariableAST(variableName);
                    break;
                case TokenType.Member:
                    result = children.Count >= 3
                                 ? GetMemberExpression(children, children.Count - 1)
                                 : GetInnerAST(children);
                    break;
                case TokenType.FunctionCall:
                    result = GetFunctionAST(children, FunctionsExpression, true);
                    break;
                
                case TokenType.NULL:
                case TokenType.EOF://reached EOF means that expression is empty
                    result = Expression.Constant(null);
                    break;
                default:
                    throw new InvalidOperationException("Should never reached here");
            }
            return result;
        }
        
        #region Get specific expressions
        private Expression GetRangeAST(ParseNode node)
        {
            var intStrs = node.Token.Text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(intStrs.Length == 2);
            var lower = Int32.Parse(intStrs[0]);
            var upper = Int32.Parse(intStrs[1]);
            if (lower > upper)
            {
                var tmp = lower;
                lower = upper;
                upper = tmp;
            }
            var range = Range<int>.Numeric(lower, upper);
            return Expression.Constant(range, typeof(Range<int>));
        }

        private Expression GetHashAST(List<ParseNode> children)
        {
            var dictType = typeof(Dictionary<string, object>);
            var addMethod = dictType.GetMethod("Add");
            var pairExprs = (children.Count == 3
                                ? GetPairsAST(children[1].Nodes, addMethod)
                                : Enumerable.Empty<ElementInit>()).ToArray();
            return pairExprs.Length == 0
                       ? (Expression)Expression.New(typeof(Dictionary<string, object>))
                       : Expression.ListInit(
                           Expression.New(typeof(Dictionary<string, object>)),
                           pairExprs
                        );
        }

        private IEnumerable<ElementInit> GetPairsAST(IEnumerable<ParseNode> nodes, MethodInfo addMethod)
        {
            return nodes.Where(node => node.Token.Type != TokenType.COMMA)
                        .Select(n => GetPairAST(n.Nodes, addMethod));
        }

        private ElementInit GetPairAST(IList<ParseNode> nodes, MethodInfo addMethod)
        {
            Debug.Assert(nodes.Count == 3);
            return Expression.ElementInit(
                                addMethod,
                                Expression.Constant(nodes[0].Token.Text, typeof(string)),
                                Expression.Convert(
                                    GetAST(nodes[2]), typeof(object)));
        }

        private Expression GetListAST(IList<ParseNode> children)
        {
            var items = children.Count == 3
                        ? children[1].Nodes
                                    .Where(node => node.Token.Type != TokenType.COMMA)
                                    .Select(n => Expression.Convert(GetAST(n), typeof(object)))
                        : Enumerable.Empty<Expression>();
            return Expression.NewArrayInit(typeof(object), items);
        }

        private Expression GetInnerAST(IList<ParseNode> childNodes)
        {
            if (childNodes.Count == 0)
            {
                throw new InvalidOperationException("Invalid syntax");
            }
            return GetAST(childNodes[0]);
        }

        private Expression GetBinaryAST(IList<ParseNode> nodes, int start)
        {
            //chain from left to right
            //2 + 3 + 4 is calculated as (2+3)+4, 15%12%5 as (15%12)%5
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = GetAST(nodes[start]);
            }
            else
            {
                var creator = GetBinaryExpressionCreator(nodes[start - 1].Token.Type);
                result = creator(GetBinaryAST(nodes, start - 2), GetAST(nodes[start]));
            }
            return result;
        }

        private Expression GetCompareAST(IList<ParseNode> nodes, int start, Expression chain = null)
        {
            //Rewrite chained compare expressions to chained AND expressions, e.g. 5>4>3> --> 5>4 AND 4>3
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = chain;
            }
            else
            {
                var creator = GetBinaryExpressionCreator(nodes[start - 1].Token.Type);
                var link = creator(GetAST(nodes[start - 2]), GetAST(nodes[start]));
                chain = chain != null
                            ? Expression.AndAlso(link, chain)
                            : link;
                result = GetCompareAST(nodes, start - 2, chain);
            }
            return result;
        }

        private Expression GetPowerAST(IList<ParseNode> nodes, int start)
        {
            //Have to rewrite power expressions to Math.Pow function calls because C# runtime does not support the ^ operator like VB
            //a^b^c is rewritten as Math.Pow(Math.Pow(a, b), c) and calculated as (a^b)^c 
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = GetAST(nodes[start]);
            }
            else
            {
                var @params = new[]{ GetPowerAST(nodes, start - 2), GetAST(nodes[start]) };
                result = Expression.Call(typeof (Math), "Pow", @params.Select(x => x.Type).ToArray(), @params);
            }
            return result;
        }

        private Expression GetUnaryAST(ParseNode @operator, ParseNode target)
        {
            //NOTE:unary expressions are unchainable without grouping
            var creator = GetUnaryExpressionCreator(@operator.Token.Type);
            return creator(GetAST(target));
        }

        private Expression GetMemberExpression(IList<ParseNode> nodes, int start)
        {
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = GetInnerAST(nodes);
            }
            else
            {
                var @operator = nodes[start - 1].Token;
                var baseExpr = GetMemberExpression(nodes, start - 2);
                if (@operator.Type == TokenType.DOT)
                {
                    var child = nodes[start].Nodes[0];
                    if (child.Token.Type == TokenType.FunctionCall)
                    {
                        result = GetFunctionAST(child.Nodes, baseExpr, false);
                    }
                    else if (child.Token.Type == TokenType.IDENTIFIER)
                    {
                        var fieldName = nodes[start].Nodes[0].Token.Text;
                        result = Expression.PropertyOrField(baseExpr, fieldName);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid children, expect either Property or Method call at this point");
                    }
                }
                else if (@operator.Type == TokenType.LBRACKET)
                {
                    var indexExpr = GetAST(nodes[start]);
                    result = Expression.MakeIndex(baseExpr, null, new[] {indexExpr});
                    throw new NotImplementedException();
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        private Expression GetFunctionAST(IList<ParseNode> childNodes, Expression baseExpr, bool isStatic)
        {
            //restrict function calls to functions defined on class Functions only
            Debug.Assert(childNodes.Count == 3 || childNodes.Count == 2);
            var argExprs = childNodes.Count == 3
                            ? GetArgumentsAST(childNodes[1].Nodes)
                            : new Expression[0];
            var binder = GetFunctionCallBinder(childNodes, argExprs.Length, isStatic);
            return Expression.Dynamic(binder, typeof(object), new[] { baseExpr }.Concat(argExprs));
        }

        private CallSiteBinder GetFunctionCallBinder(IList<ParseNode> nodes, int argCount, bool isStatic)
        {
            var funcText = nodes[0].Token.Text;
            Debug.Assert(funcText.Length >= 2 && funcText.EndsWith("("));
            var funcName = funcText.Substring(0, funcText.Length - 1);
            if (isStatic)
            {
                funcName = funcName.ToUpperInvariant();
            }

            return DLRUtil.GetFunctionCallBinder(funcName, argCount, isStatic);
        }

        private Expression[] GetArgumentsAST(IEnumerable<ParseNode> nodes)
        {
            return nodes.Where(node => node.Token.Type != TokenType.COMMA)
                        .Select(n => GetAST(n))
                        .ToArray();
        }

        private Expression GetVariableAST(string variableName)
        {
            //Rewrite variable expressions to function calls that invoke the context (getVar) functor
            var varVal = _resolver(variableName);
            var varType = varVal != null ? varVal.GetType() : typeof (object);
            return Expression.Variable(varType, variableName);
        }

        private Expression GetIfNullThenAST(IList<ParseNode> nodes, int start)
        {
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            return start == 0
                    ? GetAST(nodes[start])
                    : Expression.Coalesce(
                        GetIfNullThenAST(nodes, start - 2),
                        GetAST(nodes[start]));
        }

        private Expression GetIfThenElseAST(ParseNode condition, ParseNode then, ParseNode @else)
        {
            return Expression.Condition(Expression.Convert(GetAST(condition), typeof(bool)), GetAST(then), GetAST(@else));
        }

        private ConstantExpression _functionsExpr;
        public ConstantExpression FunctionsExpression
        {
            get { return _functionsExpr ?? (_functionsExpr = Expression.Constant(typeof (Functions))); }
        }

        internal static Func<Expression, Expression, BinaryExpression> GetBinaryExpressionCreator(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.PLUS: return Expression.Add;
                case TokenType.MINUS: return Expression.Subtract;
                case TokenType.STAR: return Expression.Multiply;
                case TokenType.FSLASH: return Expression.Divide;
                case TokenType.MODULO: return Expression.Modulo;
                case TokenType.EQUAL: return Expression.Equal;
                case TokenType.LT: return Expression.LessThan;
                case TokenType.GT: return Expression.GreaterThan;
                case TokenType.LTE: return Expression.LessThanOrEqual;
                case TokenType.GTE: return Expression.GreaterThanOrEqual;
                case TokenType.NOTEQUAL: return Expression.NotEqual;
                case TokenType.AND: return Expression.AndAlso;
                case TokenType.OR: return Expression.OrElse;
                default: throw new ArgumentOutOfRangeException("tokenType", "Token type is not a binary operator " + tokenType.ToString());
            }
        }

        internal static Func<Expression, UnaryExpression> GetUnaryExpressionCreator(TokenType tokenType)
        {
            switch (tokenType)
            {
                case TokenType.NotExpression: return Expression.Not;
                case TokenType.Negation: return Expression.Negate;
                default: throw new ArgumentOutOfRangeException("tokenType", "Token type is not a unary operator " + tokenType.ToString());
            }
        }
        #endregion
    }
}
// ReSharper restore PossibleNullReferenceException
// ReSharper restore AssignNullToNotNullAttribute