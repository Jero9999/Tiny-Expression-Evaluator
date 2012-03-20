using System;
using System.Collections.Generic;

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
            return new ParsedExpression(expression).Evaluate();
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
            return new ParsedExpression(expression).Evaluate(context);
        }

        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="getVar">The get var.</param>
        /// <returns></returns>
        public static object Evaluate(string expression, Func<string, object> getVar)
        {
            return new ParsedExpression(expression).Evaluate(getVar);
        }

        /// <summary>
        /// Parse and an expression 
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static ParsedExpression Parse(string expression)
        {
            return new ParsedExpression(expression);
        }

        /// <summary>
        /// Compiles the specified expression so that it can be called multiple times, in different context without incurring the cost of scanning, parsing, and AST transforming
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static CompiledExpression Compile(string expression)
        {
            return new ParsedExpression(expression).Compile();
        }

        public static IEnumerable<string> GetVariables(string expression)
        {
            return new ParsedExpression(expression).Variables;
        }
        #endregion
    }
}