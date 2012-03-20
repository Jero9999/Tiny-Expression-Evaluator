using System;

namespace TinyEE
{
    public struct CompiledExpression
    {
        private readonly Func<Func<string, object>, object> _compiledExpr;

        internal CompiledExpression(Func<Func<string, object>, object> compiledExpression)
        {
            _compiledExpr = compiledExpression;
        }

        public object Evaluate()
        {
            return _compiledExpr.Invoke(ContextFunctor.ZeroVariable);
        }

        public object Evaluate(object context)
        {
            return _compiledExpr.Invoke(ContextFunctor.GetForObject(context));
        }

        public object Evaluate(Func<string, object> contextFunctor)
        {
            return _compiledExpr.Invoke(contextFunctor);
        }
    }
}