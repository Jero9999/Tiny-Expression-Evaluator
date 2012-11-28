using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TinyEE
{
    internal static class Resolver
    {
        internal static object ZeroVariable(string varName)
        {
            throw new KeyNotFoundException("Variable not found");
        }

        internal static Func<string, object> FromObject(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            IDictionary dict;
            return (dict = context as IDictionary) != null 
                        ? FromDictionary(dict)
                        : FromDataObject(context);
        }

        private static Func<string,object> FromDictionary(IDictionary dictionary)
        {
            return varName => dictionary[varName];
        }

        private static Func<string,object> FromDataObject(object context)
        {
            var cache = new Dictionary<string, object>();
            return varName =>
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
    }
}