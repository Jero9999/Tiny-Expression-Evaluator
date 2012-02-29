using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TinyEE
{
    internal static class ContextFunctor
    {
        internal static object ZeroVariable(string varName)
        {
            throw new KeyNotFoundException("Variable not found");
        }

        internal static Func<string, object> GetForObject(object context, bool invasive = false)
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
                var cache = new Dictionary<string,object>();
                result = varName =>
                {
                    object value;
                    //TODO:query default (string) indexer
                    if (!cache.TryGetValue(varName, out value))
                    {
                        var body = Expression.Convert(
                                    Expression.PropertyOrField(
                                        Expression.Constant(context), varName), 
                                    typeof(object));
                        value = Expression.Lambda<Func<object>>(body).Compile().Invoke();
                        cache[varName] = value;
                    }
                    return value;
                };
            }
            return result;
        }
    }
}