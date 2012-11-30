using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace TinyEE
{
    internal static class DLRUtilx
    {
        internal static CallSiteBinder GetUnaryBinder(TokenType tokenType)
        {
            return Binder.UnaryOperation(CSharpBinderFlags.CheckedContext, GetExpressionType(tokenType), null, GetArgInfo(1));
        }

        internal static CallSiteBinder GetBinaryBinder(TokenType tokenType)
        {
            return Binder.BinaryOperation(CSharpBinderFlags.CheckedContext, GetExpressionType(tokenType), null, GetArgInfo(2));
        }

        public static CallSiteBinder GetFieldPropertyBinder(string name)
        {
            return Binder.GetMember(CSharpBinderFlags.None, name, null, GetArgInfo(1));
        }

        internal static CallSiteBinder GetIndexBinder()
        {
            return Binder.GetIndex(CSharpBinderFlags.None, null, GetArgInfo(2));
        }

        internal static CallSiteBinder GetFunctionCallBinder(string name, int argsCount = 0, bool isStatic = true)
        {
            return Binder.InvokeMember(CSharpBinderFlags.None, 
                                        name, 
                                        null, 
                                        typeof(object), 
                                        new[] {
                                            isStatic 
                                                ? CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType, null)
                                                : CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                                        }
                                        .Concat(GetArgInfo(argsCount)));
        }

        internal static IEnumerable<CSharpArgumentInfo> GetArgInfo(int count)
        {
            return Enumerable.Repeat(CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), count);
        }

        internal static ExpressionType GetExpressionType(this TokenType tokenType)
        {
            //too lazy to make a dictionary, use switch
            switch (tokenType)
            {
                case TokenType.PLUS: return ExpressionType.Add;
                case TokenType.MINUS: return ExpressionType.Subtract;
                case TokenType.STAR: return ExpressionType.Multiply;
                case TokenType.FSLASH: return ExpressionType.Divide;
                case TokenType.MODULO: return ExpressionType.Modulo;
                case TokenType.EQUAL: return ExpressionType.Equal;
                case TokenType.LT: return ExpressionType.LessThan;
                case TokenType.GT: return ExpressionType.GreaterThan;
                case TokenType.LTE: return ExpressionType.LessThanOrEqual;
                case TokenType.GTE: return ExpressionType.GreaterThanOrEqual;
                case TokenType.NOTEQUAL: return ExpressionType.NotEqual;
                case TokenType.AND: return ExpressionType.AndAlso;
                case TokenType.OR: return ExpressionType.OrElse;
                case TokenType.NotExpression: return ExpressionType.Not;
                case TokenType.Negation: return ExpressionType.Negate;
                default: throw new ArgumentOutOfRangeException();
            }
        }



        
    }
}