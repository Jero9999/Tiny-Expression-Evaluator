using System;

namespace TinyEE
{
    public struct CompiledExpression<T>
    {
        private readonly Func<Func<string, object>, T> _compiledExpr;

        internal CompiledExpression(Func<Func<string, object>, T> compiledExpression)
        {
            _compiledExpr = compiledExpression;
        }

        public T Evaluate()
        {
            return _compiledExpr.Invoke(ContextFunctor.ZeroVariable);
        }

        public T Evaluate(object context)
        {
            return _compiledExpr.Invoke(ContextFunctor.GetForObject(context));
        }

        public T Evaluate(Func<string, object> contextFunctor)
        {
            if(contextFunctor == null)
            {
                throw new ArgumentNullException("contextFunctor");
            }
            return _compiledExpr.Invoke(contextFunctor);
        }
    }
}