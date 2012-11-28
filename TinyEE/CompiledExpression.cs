using System;

namespace TinyEE
{
    /// <summary>
    /// This is a wrapper around the compiled result of an experssion. Use this when the expression has to be evaluated multiple times with different parameters to avoid the cost of parsing and transforming.
    /// </summary>
    /// <typeparam name="T">The expression's return type</typeparam>
    public struct CompiledExpression<T>
    {
        private readonly Func<Func<string, object>, T> _compiledExpr;

        internal CompiledExpression(Func<Func<string, object>, T> compiledExpression)
        {
            _compiledExpr = compiledExpression;
        }

        internal T Evaluate()
        {
            return _compiledExpr.Invoke(ContextFunctor.ZeroVariable);
        }

        /// <summary>
        /// Evaluates the expression using the specified context data-object (or dictionary).
        /// </summary>
        /// <param name="context">The context data-object (or dictionary) whose properties (or keys) will be used as variable in the compiled expression.</param>
        public T Evaluate(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return _compiledExpr.Invoke(ContextFunctor.GetForObject(context));
        }

        /// <summary>
        /// Evaluates the expression using the specified resolver delegate.
        /// </summary>
        /// <param name="resolver">The resolver delegate that accept the variable's name (string) and return its value. It should throw KeyNotFoundException if the variable name cannot be resolved.</param>
        public T Evaluate(Func<string, object> resolver)
        {
            if(resolver == null)
            {
                throw new ArgumentNullException("resolver");
            }
            return _compiledExpr.Invoke(resolver);
        }
    }
}