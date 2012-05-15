using System;
using System.Dynamic;
using System.Reflection;

namespace TinyEE
{
    /// <summary>
    /// Wrap around a static type, convert instance method calls into static ones
    /// </summary>
    internal class FunctionsWrapper : DynamicObject
    {
        private readonly Type _type;

        internal FunctionsWrapper(Type type)
        {
            _type = type;
        }

        const BindingFlags StaticFlags = BindingFlags.InvokeMethod 
                                        | BindingFlags.Static 
                                        | BindingFlags.Public 
                                        | BindingFlags.IgnoreCase;
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = _type.InvokeMember(binder.Name, StaticFlags, null, null, args);
            return true;
        }
    }
}