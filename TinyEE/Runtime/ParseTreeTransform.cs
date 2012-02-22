using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TinyEE
{
    /// <summary>
    /// Transform the parsed tree into an abstract syntax tree (AST)
    /// </summary>
    internal static class ParseTreeTransform
    {
        internal static Expression GetAST(this ParseNode node, Expression context)
        {
            Expression result;
            var childNodes = node.Nodes.ToArray();
            switch (node.Token.Type)
            {
                case TokenType.OrExpression:
                case TokenType.AndExpression:
                case TokenType.Addition:
                case TokenType.Multiplication:
                    result = childNodes.Length >= 3
                                 ? GetBinaryExpression(childNodes, childNodes.Length - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Power:
                    result = childNodes.Length >= 3
                                 ? GetPowerExpression(childNodes, childNodes.Length - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Compare:
                    result = childNodes.Length >= 3
                                 ? GetCompareExpression(childNodes, childNodes.Length - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Negation:
                case TokenType.NotExpression:
                    result = childNodes.Length == 2
                                 ? GetUnaryExpression(node, childNodes[1], context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Start:
                case TokenType.Expression:
                case TokenType.Base:
                case TokenType.Literal:
                case TokenType.IndexAccess://has 2 childs, but uses the first one only
                    result = GetInnerAST(childNodes, context);
                    break;
                case TokenType.Group:
                    Debug.Assert(childNodes.Length == 3);
                    result = childNodes[1].GetAST(context);
                    break;
                case TokenType.Variable:
                    result = GetVariableExpression(childNodes, context);
                    break;
                case TokenType.Member:
                    result = childNodes.Length >= 3
                                 ? GetMemberExpression(childNodes, childNodes.Length - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.FunctionCall:
                    result = GetFunctionExpression(childNodes, context);
                    break;
                case TokenType.INTEGER:
                    //TODO: big integer
                    result = Expression.Constant(Int64.Parse(node.Token.Text));
                    break;
                case TokenType.DECIMAL:
                    //TODO: big decimal
                    result = Expression.Constant(Double.Parse(node.Token.Text));
                    break;
                case TokenType.STRING:
                    var nodeText = node.Token.Text;
                    Debug.Assert(nodeText.Length >= 2 && nodeText.StartsWith("\"") && nodeText.EndsWith("\""));
                    nodeText = nodeText.Substring(1, nodeText.Length - 2);
                    result = Expression.Constant(nodeText);
                    break;
                case TokenType.TRUE:
                    result = Expression.Constant(true);
                    break;
                case TokenType.FALSE:
                    result = Expression.Constant(false);
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
        private static Expression GetInnerAST(ParseNode[] childNodes, Expression context)
        {
            if (childNodes.Length == 0)
            {
                throw new InvalidOperationException("Invalid syntax");
            }
            return childNodes[0].GetAST(context);
        }

        private static Expression GetBinaryExpression(ParseNode[] nodes, int start, Expression context)
        {
            //chain from left to right
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            return start == 0
                       ? nodes[start].GetAST(context)
                       : Expression.Dynamic(DLRUtil.GetBinaryBinder(nodes[start - 1].Token.Type),
                                            typeof(object),
                                            GetBinaryExpression(nodes, start - 2, context),
                                            nodes[start].GetAST(context));
        }

        private static Expression GetCompareExpression(ParseNode[] nodes, int start, Expression context, Expression chain = null)
        {
            //Rewrite chained compare expressions to chained AND expressions, e.g. 5>4>3> --> 5>4 AND 4>3
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
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
                result = GetCompareExpression(nodes, start - 2, context, chain);
            }
            return result;
        }

        private static Expression GetPowerExpression(ParseNode[] nodes, int start, Expression context)
        {
            //Have to rewrite power expressions to Math.Pow function calls because C# runtime does not support the ^ operator like VB
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            return start == 0
                       ? nodes[start].GetAST(context)
                       : Expression.Dynamic(DLRUtil.GetStaticFunctionCallBinder("POW", 2),
                                            typeof(object),
                                            FunctionsExpression,
                                            GetPowerExpression(nodes, start - 2, context),
                                            nodes[start].GetAST(context));
        }

        private static Expression GetUnaryExpression(ParseNode @operator, ParseNode target, Expression context)
        {
            return Expression.Dynamic(DLRUtil.GetUnaryBinder(@operator.Token.Type),
                                      typeof(object),
                                      target.GetAST(context));
        }

        private static Expression GetMemberExpression(ParseNode[] nodes, int start, Expression context)
        {
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            Expression result;
            if (start == 0)
            {
                result = GetInnerAST(nodes, context);
            }
            else
            {
                var @operator = nodes[start - 1].Token;
                if (@operator.Type == TokenType.DOT)
                {
                    var fieldName = nodes[start].Nodes[0].Token.Text;
                    result = Expression.Dynamic(DLRUtil.GetFieldPropertyBinder(fieldName),
                                                typeof(object),
                                                GetMemberExpression(nodes, start - 2, context));
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
                                                GetMemberExpression(nodes, start - 2, context),
                                                indexExpr);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        private static Expression GetFunctionExpression(ParseNode[] childNodes, Expression context)
        {
            //restrict function calls to static functions defined on class Functions only
            Debug.Assert(childNodes.Length == 3 || childNodes.Length == 2);
            var argExprs = childNodes.Length == 3
                               ? childNodes[1].Nodes
                                             .Where(node => node.Token.Type != TokenType.COMMA)
                                             .Select(n => GetAST(n, context))
                               : Enumerable.Empty<Expression>();
            
            var funcText = childNodes[0].Token.Text;
            Debug.Assert(funcText.Length >= 2 && funcText.EndsWith("("));
            var funcName = funcText.Substring(0, funcText.Length - 1)
                                   .ToUpperInvariant();

            return Expression.Dynamic(DLRUtil.GetStaticFunctionCallBinder(funcName, argExprs.Count()),
                                        typeof(object),
                                        new[] { FunctionsExpression }.Concat(argExprs));
        }

        private static Expression GetVariableExpression(ParseNode[] childNodes, Expression context)
        {
            //Rewrite variable expressions to function calls that invoke the context (getVar) functor
            Debug.Assert(childNodes.Length == 1);
            var variableName = childNodes[0].Token.Text;
            return Expression.Call(context, VariableResolverInfo, Expression.Constant(variableName));
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