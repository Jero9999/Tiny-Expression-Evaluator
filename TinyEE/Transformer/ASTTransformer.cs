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
    /// <summary>
    /// Transform the parsed tree into an abstract syntax tree (AST)
    /// </summary>
    internal static class ASTTransformer
    {
        internal static Expression GetAST(this ParseNode node, Expression context)
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
                                 ? GetBinaryAST(children, children.Count - 1, context)
                                 : GetInnerAST(children, context);
                    break;
                case TokenType.Power:
                    result = children.Count >= 3
                                 ? GetPowerAST(children, children.Count - 1, context)
                                 : GetInnerAST(children, context);
                    break;
                case TokenType.Compare:
                    result = children.Count >= 3
                                 ? GetCompareAST(children, children.Count - 1, context)
                                 : GetInnerAST(children, context);
                    break;
                case TokenType.Negation:
                case TokenType.NotExpression:
                    result = children.Count == 2
                                 ? GetUnaryAST(node, children[1], context)
                                 : GetInnerAST(children, context);
                    break;
                case TokenType.Start:
                case TokenType.Expression:
                case TokenType.Base:
                case TokenType.Literal:
                case TokenType.IndexAccess://has 2 childs, but uses the first one only
                    result = GetInnerAST(children, context);
                    break;
                case TokenType.Group:
                    Debug.Assert(children.Count == 3);
                    result = children[1].GetAST(context);
                    break;
                case TokenType.Variable:
                    var variableName = children[0].Token.Text;
                    result = GetVariableAST(variableName, context);
                    break;
                case TokenType.Member:
                    result = children.Count >= 3
                                 ? GetMemberExpression(children, children.Count - 1, context)
                                 : GetInnerAST(children, context);
                    break;
                case TokenType.FunctionCall:
                    result = GetFunctionAST(children, FunctionsExpression, true, context);
                    break;
                case TokenType.ListLiteral:
                    result = GetListAST(children, context);
                    break;
                case TokenType.HashLiteral:
                    result = GetHashAST(children, context);
                    break;
                case TokenType.RANGE:
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
                case TokenType.NULL:
                case TokenType.EOF://reached EOF means that expression is empty    
                    result = Expression.Constant(null);
                    break;
                default:
                    throw new InvalidOperationException("Should never reached here");
            }
            return result;
        }

        private static Expression GetRangeAST(ParseNode node)
        {
            var intStrs = node.Token.Text.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            Debug.Assert(intStrs.Length == 2);
            var lower = Int32.Parse(intStrs[0]);
            var upper = Int32.Parse(intStrs[1]);
            if(lower > upper)
            {
                var tmp = lower;
                lower = upper;
                upper = tmp;
            }
            var range = Enumerable.Range(lower, upper - lower).ToArray();
            return Expression.Constant(range, typeof(int[]));
        }

        private static Expression GetHashAST(List<ParseNode> children, Expression context)
        {
            var pairExprs = children.Count == 3
                                ? GetPairsAST(children[1].Nodes, context)
                                : Enumerable.Empty<Expression>();
            var fnDictInfo = typeof (Functions).GetMethod("DICTIONARY");
            return Expression.Call(fnDictInfo, Expression.NewArrayInit(typeof(KeyValuePair<string, object>), pairExprs));
        }

        private static IEnumerable<Expression> GetPairsAST(IEnumerable<ParseNode> nodes, Expression context)
        {
            return nodes.Where(node => node.Token.Type != TokenType.COMMA)
                        .Select(n => GetPairAST(n.Nodes, context));
        }

        private static Expression GetPairAST(IList<ParseNode> nodes, Expression context)
        {
            Debug.Assert(nodes.Count == 3);
            var keyStrExpr = GetAST(nodes[0], context);

            var valueExpr = Expression.Convert(GetAST(nodes[2], context), typeof(object));

            var ctor = typeof (KeyValuePair<string, object>).GetConstructor(new[] {typeof (string), typeof (object)});
            return Expression.New(ctor, keyStrExpr, valueExpr);
        }

        private static Expression GetListAST(IList<ParseNode> children, Expression context)
        {
            var items = children.Count == 3
                        ? children[1].Nodes
                                    .Where(node => node.Token.Type != TokenType.COMMA)
                                    .Select(n =>Expression.Convert(GetAST(n, context), typeof(object)))
                        : Enumerable.Empty<Expression>();
            return Expression.NewArrayInit(typeof(object), items);
        }

        #region Get specific expressions
        private static Expression GetInnerAST(IList<ParseNode> childNodes, Expression context)
        {
            if (childNodes.Count == 0)
            {
                throw new InvalidOperationException("Invalid syntax");
            }
            return childNodes[0].GetAST(context);
        }

        private static Expression GetBinaryAST(IList<ParseNode> nodes, int start, Expression context)
        {
            //chain from left to right
            //2 + 3 + 4 is calculated as (2+3)+4, 15%12%5 as (15%12)%5
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            return start == 0
                       ? nodes[start].GetAST(context)
                       : Expression.Dynamic(DLRUtil.GetBinaryBinder(nodes[start - 1].Token.Type),
                                            typeof(object),
                                            GetBinaryAST(nodes, start - 2, context),
                                            nodes[start].GetAST(context));
        }

        private static Expression GetCompareAST(IList<ParseNode> nodes, int start, Expression context, Expression chain = null)
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
                var link = Expression.Dynamic(DLRUtil.GetBinaryBinder(nodes[start - 1].Token.Type),
                                              typeof(object),
                                              nodes[start - 2].GetAST(context),
                                              nodes[start].GetAST(context));
                chain = chain != null
                            ? Expression.Dynamic(DLRUtil.GetBinaryBinder(TokenType.AND),
                                                 typeof(object),
                                                 link,
                                                 chain)
                            : link;
                result = GetCompareAST(nodes, start - 2, context, chain);
            }
            return result;
        }

        private static Expression GetPowerAST(IList<ParseNode> nodes, int start, Expression context)
        {
            //Have to rewrite power expressions to Math.Pow function calls because C# runtime does not support the ^ operator like VB
            //a^b^c is rewritten as Math.Pow(Math.Pow(a, b), c) and calculated as (a^b)^c 
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            return start == 0
                       ? nodes[start].GetAST(context)
                       : Expression.Dynamic(DLRUtil.GetFunctionCallBinder("Pow", 2),
                                            typeof(object),
                                            Expression.Constant(typeof(Math)),
                                            GetPowerAST(nodes, start - 2, context),
                                            nodes[start].GetAST(context));
        }

        private static Expression GetUnaryAST(ParseNode @operator, ParseNode target, Expression context)
        {
            //NOTE:unary expressions are unchainable without grouping
            return Expression.Dynamic(DLRUtil.GetUnaryBinder(@operator.Token.Type),
                                      typeof(object),
                                      target.GetAST(context));
        }

        private static Expression GetMemberExpression(IList<ParseNode> nodes, int start, Expression context)
        {
            Debug.Assert(nodes.Count >= 3 && nodes.Count % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = GetInnerAST(nodes, context);
            }
            else
            {
                var @operator = nodes[start - 1].Token;
                var baseExpr = GetMemberExpression(nodes, start - 2, context);
                if (@operator.Type == TokenType.DOT)
                {
                    var child = nodes[start].Nodes[0];
                    if (child.Token.Type == TokenType.FunctionCall)
                    {
                        result = GetFunctionAST(child.Nodes, baseExpr, false, context);
                    }
                    else if (child.Token.Type == TokenType.IDENTIFIER)
                    {
                        var fieldName = nodes[start].Nodes[0].Token.Text;
                        result = Expression.Dynamic(DLRUtil.GetFieldPropertyBinder(fieldName), typeof(object), baseExpr);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid children, expect either Property or Method call at this point");
                    }
                }
                else if (@operator.Type == TokenType.LBRACKET)
                {
                    var indexExpr = nodes[start].GetAST(context);
                    //convert long to int32 as most indexer in .NET are by int32 (pragmatic decision)
                    if(indexExpr.Type == typeof(long))
                    {
                        indexExpr = Expression.Convert(indexExpr, typeof (int));
                    }

                    result = Expression.Dynamic(DLRUtil.GetIndexBinder(),
                                                typeof(object),
                                                baseExpr,
                                                indexExpr);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        private static Expression GetFunctionAST(IList<ParseNode> childNodes, Expression baseExpr, bool isStatic, Expression context)
        {
            //restrict function calls to static functions defined on class Functions only
            Debug.Assert(childNodes.Count == 3 || childNodes.Count == 2);
            var argExprs = childNodes.Count == 3
                            ? GetArgumentsAST(childNodes[1].Nodes, context)
                            : new Expression[0];
            var binder = GetFunctionCallBinder(childNodes, argExprs.Length, isStatic);
            return Expression.Dynamic(binder, typeof(object), new[] { baseExpr }.Concat(argExprs));
        }

        private static CallSiteBinder GetFunctionCallBinder(IList<ParseNode> nodes, int argCount, bool isStatic)
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

        private static Expression[] GetArgumentsAST(IEnumerable<ParseNode> nodes, Expression context)
        {
            return nodes.Where(node => node.Token.Type != TokenType.COMMA)
                        .Select(n => GetAST(n, context))
                        .ToArray();
        }

        private static Expression GetVariableAST(string variableName, Expression context)
        {
            //Rewrite variable expressions to function calls that invoke the context (getVar) functor
            return Expression.Call(context, VariableResolverInfo, new Expression[]{Expression.Constant(variableName)});
        }

        private static MethodInfo _varResolverInfo;
        public static MethodInfo VariableResolverInfo
        {
            get { return _varResolverInfo ?? (_varResolverInfo = typeof(Func<string,object>).GetMethod("Invoke")); }
        }

        private static ConstantExpression _functionsExpr;
        public static ConstantExpression FunctionsExpression
        {
            get { return _functionsExpr ?? (_functionsExpr = Expression.Constant(typeof (Functions))); }
        }
        #endregion
    }
}
// ReSharper restore PossibleNullReferenceException
// ReSharper restore AssignNullToNotNullAttribute