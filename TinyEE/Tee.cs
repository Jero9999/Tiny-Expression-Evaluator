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
        /// Evaluates the supplied string as an expression, return the execution result
        /// </summary>
        /// <param name="expression">The expression string.</param>
        public static T Evaluate<T>(string expression)
        {
            return new ParsedExpression<T>(expression).Evaluate();
        }

        /// <summary>
        /// Evaluates the supplied string as an expression. 
        /// Variables within the expression shall be resolved using the context object's properties or fields.
        /// In case the object implements IDictionary of strings, the indexer will be used instead.
        /// </summary>
        /// <param name="expression">The expression string.</param>
        /// <param name="context">The context object.</param>
        public static T Evaluate<T>(string expression, object context)
        {
            return new ParsedExpression<T>(expression).Evaluate(context);
        }

        /// <summary>
        /// Evaluates the specified expression, using a delegate to resolve variables.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <param name="fnResolver">The variable resolver, it takes a name and returns the variable with that name.</param>
        public static T Evaluate<T>(string expression, Func<string, object> fnResolver)
        {
            return new ParsedExpression<T>(expression).Evaluate(fnResolver);
        }

        /// <summary>
        /// Parse and an expression 
        /// </summary>
        /// <param name="expression">The expression.</param>
        public static ParsedExpression<T> Parse<T>(string expression)
        {
            return new ParsedExpression<T>(expression);
        }

        /// <summary>
        /// Compiles the specified expression so that it can be called multiple times, in different context without incurring the cost of scanning, parsing, and transforming the AST
        /// </summary>
        /// <param name="expression">The expression.</param>
        public static CompiledExpression<T> Compile<T>(string expression)
        {
            return new ParsedExpression<T>(expression).Compile();
        }

        /// <summary>
        /// Returns a wrapper that convert instance method calls to static ones
        /// </summary>
        public static dynamic Functions<T>()
        {
            return new FunctionsWrapper(typeof(T));
        }

        /// <summary>
        /// Gets the list of variables used in an expression
        /// </summary>
        /// <param name="expression">The expression.</param>
        public static IEnumerable<string> GetVariables(string expression)
        {
            return new ParsedExpression<object>(expression).Variables;
        }
        #endregion

        /// <summary>
        /// Cache the parser
        /// </summary>
        internal static readonly Lazy<Parser> ParserInit = new Lazy<Parser>(() => new Parser(new Scanner()), true);
    }
}