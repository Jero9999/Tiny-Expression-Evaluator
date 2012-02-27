using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TinyEE
{
    internal static class ContextFunctor
    {
        internal static Func<string, object> GetForObject(object context)
        {
            if (context == null)
            {
                return ZeroVariable;
            }
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
                    var body = Expression.Dynamic(
                                DLRUtil.GetFieldPropertyBinder(varName),
                                typeof(object),
                                Expression.Constant(context));
                    var memGetter = Expression.Lambda<Func<string, object>>(body, Expression.Parameter(typeof(string))).Compile();
                    return memGetter(varName);
                };
            }
            return result;
        }

        internal static object ZeroVariable(string varName)
        {
            throw new KeyNotFoundException("Variable not found");
        } 
    }
}