using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyEE
{
    /// <summary>
    /// The point of service (fascade) for the Tiny Expression Evaluator
    /// </summary>
    public static class TEE
    {
        #region Public API
        /// <summary>
        /// Evaluates the supplied string as an expression
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <returns></returns>
        public static object Evaluate(string expression)
        {
            return Evaluate(expression, ContextFunctor.ZeroVariable);
        }


        /// <summary>
        /// Evaluates the supplied string as an expression. 
        /// Variables within the expression shall be resolved using the context object's properties or fields.
        /// In case the object implements IDictionary of strings, the indexer will be used instead.
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <param name="context">The context object.</param>
        /// <returns></returns>
        public static object Evaluate(string expression, object context)
        {
            var contextFunctor = ContextFunctor.GetForObject(context);
            return Evaluate(expression, contextFunctor);
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="getVar">The get var.</param>
        /// <returns></returns>
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

        public static string TranslateToJs(string expression)
        {
            return Parse(expression).GetJsExpr();
        }
        #endregion

        #region Private
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
            var expressionTree = ASTTransformer.GetAST(parseTree.Nodes[0], contextExpr);
            return Expression.Lambda<Func<Func<string, object>, object>>(
                Expression.TypeAs(expressionTree, typeof(object)),
                contextExpr);
        }
        #endregion
    }
}