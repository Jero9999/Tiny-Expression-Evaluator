using System.Collections.Generic;
using System.Linq.Expressions;

namespace TinyEE
{
    internal class VariableEnumerator:ExpressionVisitor
    {
        internal readonly ISet<string> Variables;
        private readonly string _contextVarName;

        internal VariableEnumerator(string contextVariableName)
        {
            _contextVarName = contextVariableName;
            Variables = new HashSet<string>();
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            ConstantExpression varNameExpr;
            ParameterExpression contextExpr;
            if ((contextExpr = node.Expression as ParameterExpression) != null &&
                contextExpr.Name == _contextVarName &&
                node.Arguments.Count == 1 && 
                (varNameExpr = node.Arguments[0] as ConstantExpression) != null)
            {
                Variables.Add((string)varNameExpr.Value);
            }
            return base.VisitInvocation(node);
        }
    }
}