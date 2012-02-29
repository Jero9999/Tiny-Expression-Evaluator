using System;
using System.Collections.Generic;

namespace TinyEE
{
    public class ParsedExpression
    {
        private readonly Func<Func<string, object>, object> _compiledExpr;

        internal ParsedExpression(Func<Func<string,object>, object> compiledExpression)
        {
            _compiledExpr = compiledExpression;
        }

        public string Text { get; internal set; }

        public IEnumerable<string> Variables { get; internal set; }

        public string JsText { get; internal set; }

        public string CSharpText { get; internal set; }

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