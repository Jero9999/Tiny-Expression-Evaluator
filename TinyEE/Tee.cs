using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyEE
{
    public static class TEE
    {
        #region Public API
        public static object Evaluate(string expression)
        {
            return Evaluate(expression, ZeroVariableFunctor);
        }

        public static object Evaluate(string expression, object context)
        {
            var contextFunctor = GetContextFunctor(context);
            return Evaluate(expression, contextFunctor);
        }

        public static object Evaluate(string expression, Func<string, object> getVar)
        {
            return Parse(expression)
                    .Transform()
                    .Compile()
                    .Invoke(getVar);
        }

        public static IEnumerable<string> GetVariableNames(string expression)
        {
            var tree = new Parser(new Scanner()).Parse(expression);
            return GetVariableNamesRecursive(tree);
        }

        private static IEnumerable<string> GetVariableNamesRecursive(ParseNode node)
        {
            if (node.Token.Type == TokenType.Variable)
            {
                yield return node.Nodes[0].Token.Text;
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    foreach (var childNode in node.Nodes)
                    {
                        foreach (var varible in GetVariableNamesRecursive(childNode))
                        {
                            yield return varible;
                        }
                    }
                }
            }
        } 
        #endregion

        #region Private
        private static ParseTree Parse(string expression)
        {
            var parseTree = new Parser(new Scanner()).Parse(expression);
            if (parseTree.Errors.Count > 0)
            {
                var error = new ArgumentException("Syntax error");
                error.Data["details"] = parseTree.Errors.ToList();
                throw error;
            }
            return parseTree;
        }

        private static Expression<Func<Func<string, object>, object>> Transform(this ParseNode parseTree)
        {
            var contextExpr = Expression.Parameter(typeof(Func<string, object>), "context");
            var expressionTree = ParseTreeTransform.GetAST(parseTree.Nodes[0], contextExpr);
            return Expression.Lambda<Func<Func<string, object>, object>>(
                Expression.TypeAs(expressionTree, typeof(object)),
                contextExpr);
        }

        private static Func<string, object> GetContextFunctor(object context)
        {
            Func<string, object> result;
            IDictionary dict;

            if ((dict = context as IDictionary) != null)
            {
                result = varName => dict[varName];
            }
            else
            {
                result = varName =>
                {
                    var memGetter = Expression.Lambda<Func<string, object>>(
                        Expression.Dynamic(
                            DLRUtil.GetFieldPropertyBinder(varName), typeof(object)))
                        .Compile();
                    return memGetter(varName);
                };
            }
            return result;
        }

        private static object ZeroVariableFunctor(string varName)
        {
            throw new KeyNotFoundException("Variable not found");
        } 
        #endregion
    }
}