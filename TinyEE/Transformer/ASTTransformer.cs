using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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
            var childNodes = node.Nodes;
            switch (node.Token.Type)
            {
                case TokenType.OrExpression:
                case TokenType.AndExpression:
                case TokenType.Addition:
                case TokenType.Multiplication:
                    result = childNodes.Count >= 3
                                 ? GetBinaryAST(childNodes, childNodes.Count - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Power:
                    result = childNodes.Count >= 3
                                 ? GetPowerAST(childNodes, childNodes.Count - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Compare:
                    result = childNodes.Count >= 3
                                 ? GetCompareAST(childNodes, childNodes.Count - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Negation:
                case TokenType.NotExpression:
                    result = childNodes.Count == 2
                                 ? GetUnaryAST(node, childNodes[1], context)
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
                    Debug.Assert(childNodes.Count == 3);
                    result = childNodes[1].GetAST(context);
                    break;
                case TokenType.Variable:
                    var variableName = childNodes[0].Token.Text;
                    result = GetVariableAST(variableName, context);
                    break;
                case TokenType.MethodCall:
                    result = childNodes.Count == 4
                                 ? GetMethodAST(childNodes, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.Member:
                    result = childNodes.Count >= 3
                                 ? GetMemberExpression(childNodes, childNodes.Count - 1, context)
                                 : GetInnerAST(childNodes, context);
                    break;
                case TokenType.FunctionCall:
                    result = GetFunctionAST(childNodes, context);
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
                    nodeText = nodeText.Substring(1, nodeText.Length - 2).Replace("\\\"", "\"");
                    result = Expression.Constant(nodeText);
                    break;
                case TokenType.TRUE:
                    result = Expression.Constant(true);
                    break;
                case TokenType.FALSE:
                    result = Expression.Constant(false);
                    break;
                case TokenType.NULL:
                case TokenType.EOF:
                    //reached EOF means that expression is empty    
                    result = Expression.Constant(null);
                    break;
                default:
                    throw new InvalidOperationException("Should never reached here");
            }

            return result;
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
            //2 + 3 + 4 is calculated as (2+3)+4
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
                       : Expression.Dynamic(DLRUtil.GetFunctionCallBinder("POW", 2),
                                            typeof(object),
                                            FunctionsExpression,
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

        private static Expression GetMethodAST(IList<ParseNode> nodes, Expression context)
        {
            //method calls are not chainable and can only be invoked on system variables (variables beginning with $)
            Debug.Assert(nodes.Count == 4);
            var varName = nodes[0].Token.Text + nodes[1].Nodes[0].Token.Text;
            var @var = GetVariableAST(varName, context);
            
            var fnNodes = nodes[3].Nodes;
            var argExprs = fnNodes.Count == 3
                               ? GetArgumentsAST(fnNodes[1].Nodes.ToArray(), context)
                               : Enumerable.Empty<Expression>();
            var binder = GetFunctionCallBinder(fnNodes, argExprs.Count(), false);
            return Expression.Dynamic(binder, typeof (object), new[] {@var}.Concat(argExprs));
        }

        private static Expression GetFunctionAST(IList<ParseNode> childNodes, Expression context)
        {
            //restrict function calls to static functions defined on class Functions only
            Debug.Assert(childNodes.Count == 3 || childNodes.Count == 2);
            var argExprs = childNodes.Count == 3
                            ? GetArgumentsAST(childNodes[1].Nodes.ToArray(), context)
                            : Enumerable.Empty<Expression>();
            return Expression.Dynamic(GetFunctionCallBinder(childNodes, argExprs.Count(), true),
                                        typeof(object),
                                        new[] { FunctionsExpression }.Concat(argExprs));
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

        private static IEnumerable<Expression> GetArgumentsAST(IList<ParseNode> nodes, Expression context)
        {
            return nodes.Where(node => node.Token.Type != TokenType.COMMA)
                        .Select(n => GetAST(n, context));
        }

        private static Expression GetVariableAST(string variableName, Expression context)
        {
            //Rewrite variable expressions to function calls that invoke the context (getVar) functor
            
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
