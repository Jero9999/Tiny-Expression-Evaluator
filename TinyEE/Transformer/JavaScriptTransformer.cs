using System;
using System.Diagnostics;
using System.Linq;

namespace TinyEE
{
    internal static class JavaScriptTransformer
    {
        internal static string GetJsExpr(this ParseNode node)
        {
            string result;
            var childNodes = node.Nodes.ToArray();
            switch (node.Token.Type)
            {
                case TokenType.OrExpression:
                case TokenType.AndExpression:
                case TokenType.Addition:
                case TokenType.Multiplication:
                    result = childNodes.Length >= 3 
                                 ? GetBinaryJsExpr(childNodes, childNodes.Length - 1) 
                                 : GetInnerJsExpr(childNodes);
                    break;
                case TokenType.Power:
                    result = childNodes.Length >= 3
                                 ? GetPowerJsExpr(childNodes, childNodes.Length - 1)
                                 : GetInnerJsExpr(childNodes);
                    break;
                case TokenType.Compare:
                    result = childNodes.Length >= 3
                                 ? GetCompareJsExpr(childNodes, childNodes.Length - 1)
                                 : GetInnerJsExpr(childNodes);
                    break;
                case TokenType.Negation:
                case TokenType.NotExpression:
                    result = childNodes.Length == 2
                                 ? GetUnaryJsExpr(node, childNodes[1])
                                 : GetInnerJsExpr(childNodes);
                    break;
                case TokenType.Start:
                case TokenType.Expression:
                case TokenType.Base:
                case TokenType.Literal:
                case TokenType.IndexAccess://has 2 childs, but uses the first one only
                    result = GetInnerJsExpr(childNodes);
                    break;
                case TokenType.Group:
                    Debug.Assert(childNodes.Length == 3);
                    result = "(" + GetJsExpr(childNodes[1]) + ")";
                    break;
                case TokenType.Variable:
                    result = childNodes[0].Token.Text;
                    break;
                case TokenType.Member:
                    result = childNodes.Length >= 3
                                 ? GetMemberJsExpr(childNodes, childNodes.Length - 1)
                                 : GetInnerJsExpr(childNodes);
                    break;
                case TokenType.FunctionCall:
                    result = GetFunctionJsExpr(childNodes);
                    break;
                case TokenType.INTEGER:
                    result = node.Token.Text;
                    break;
                case TokenType.DECIMAL:
                    result = node.Token.Text;
                    break;
                case TokenType.STRING:
                    result = node.Token.Text;
                    break;
                case TokenType.TRUE:
                    result = "true";
                    break;
                case TokenType.FALSE:
                    result = "false";
                    break;
                case TokenType.NULL:
                    result = "null";
                    break;
                case TokenType.EOF://reached EOF means that expression is empty
                    result = string.Empty;
                    break;
                default:
                    throw new InvalidOperationException("Should never reached here");
            }
            return result;
        }

        private static string GetFunctionJsExpr(ParseNode[] childNodes)
        {
            Debug.Assert(childNodes.Length == 3 || childNodes.Length == 2);
            var argExprs = childNodes.Length == 3
                               ? childNodes[1].Nodes
                                     .Where(node => node.Token.Type != TokenType.COMMA)
                                     .Select(n => GetJsExpr(n))
                               : Enumerable.Empty<string>();
            var funcText = childNodes[0].Token.Text;
            Debug.Assert(funcText.Length >= 2 && funcText.EndsWith("("));
            var funcName = funcText.Substring(0, funcText.Length - 1)
                .ToLowerInvariant();
            return funcName + "(" + String.Join(",", argExprs) + ")";
        }

        private static string GetMemberJsExpr(ParseNode[] nodes, int start)
        {
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            string result;
            if (start == 0)
            {
                result = GetInnerJsExpr(nodes);
            }
            else
            {
                var @operator = nodes[start - 1].Token;
                if (@operator.Type == TokenType.DOT)
                {
                    var fieldName = nodes[start].Nodes[0].Token.Text;
                    result = GetMemberJsExpr(nodes, start - 2) + "." + fieldName;
                }
                else if (@operator.Type == TokenType.LBRACKET)
                {
                    result = GetMemberJsExpr(nodes, start - 2) + "[" + GetJsExpr(nodes[start]) + "]";
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return result;
        }

        private static string GetUnaryJsExpr(ParseNode @operator, ParseNode target)
        {
            return GetJsOperator(@operator.Token.Type) + GetJsExpr(target);
        }

        private static string GetCompareJsExpr(ParseNode[] nodes, int start, string chain = null)
        {
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            string result;
            if (start == 0)
            {
                result = chain;
            }
            else
            {
                var link = GetJsExpr(nodes[start - 2])
                           + GetJsOperator(nodes[start - 1].Token.Type)
                           + GetJsExpr(nodes[start]);
                chain = chain != null
                            ? link + "&&" + chain
                            : link;
                result = GetCompareJsExpr(nodes, start - 2, chain);
            }
            return result;
        }

        private static string GetPowerJsExpr(ParseNode[] nodes, int start)
        {
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            return start == 0
                       ? GetJsExpr(nodes[start])
                       : "Math.pow(" 
                         + GetPowerJsExpr(nodes, start - 2)
                         + ","
                         + GetJsExpr(nodes[start])
                         + ")";
        }

        private static string GetBinaryJsExpr(ParseNode[] nodes, int start)
        {
            Debug.Assert(nodes.Length >= 3 && nodes.Length % 2 == 1);
            return start == 0
                       ? GetJsExpr(nodes[start])
                       : GetBinaryJsExpr(nodes, start - 2)
                         + GetJsOperator(nodes[start - 1].Token.Type)
                         + GetJsExpr(nodes[start]);
        }

        private static string GetInnerJsExpr(ParseNode[] nodes)
        {
            if (nodes.Length == 0)
            {
                throw new InvalidOperationException("Invalid syntax");
            }
            return GetJsExpr(nodes[0]);
        }

        internal static string GetJsOperator(this TokenType tokenType)
        {
            //too lazy to make a dictionary, use switch
            switch (tokenType)
            {
                case TokenType.PLUS: return "+";
                case TokenType.MINUS: return "-";
                case TokenType.STAR: return "*";
                case TokenType.FSLASH: return "/";
                case TokenType.EQUAL: return "===";
                case TokenType.LT: return "<";
                case TokenType.GT: return ">";
                case TokenType.LTE: return "<=";
                case TokenType.GTE: return ">=";
                case TokenType.NOTEQUAL: return "!==";
                case TokenType.AND: return "&&";
                case TokenType.OR: return "||";
                case TokenType.NotExpression: return "!";
                case TokenType.Negation: return "-";
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}