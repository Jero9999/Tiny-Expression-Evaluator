using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyEE
{
    /// <summary>
    /// A wrapper around an expression's parse tree that expose the transformed abstract syntax tree.
    /// </summary>
    public struct ParsedExpression<T>
    {
        internal const string ContextVariableName = "_ctx";

        private readonly string _text;
        private readonly ParameterExpression _ctxExpr;
        private readonly Expression _ast;
        
        private IEnumerable<string> _variables;
        
        internal ParsedExpression(string text)
        {
            if(text == null)
            {
                throw new ArgumentNullException("text");
            }
            
            var parseTree = ParseInternal(text);
            _ctxExpr = Expression.Parameter(typeof(Func<string, object>), ContextVariableName);
            _ast = parseTree.GetAST(_ctxExpr);
            _text = parseTree.Text;
            _variables = null;
        }

        #region Props
        /// <summary>
        /// The original textual content of the expression
        /// </summary>
        public string Text
        {
            get { return _text ?? String.Empty; }
        }

        /// <summary>
        /// List of variables used in the expression
        /// </summary>
        public IEnumerable<string> Variables
        {
            get { return _variables ?? (_variables = GetVariables()); }
        }

        ///// <summary>
        ///// The abstract syntax tree for this expression
        ///// </summary>
        //public Expression AST
        //{
        //    get { return _ast; }
        //}
        #endregion

        #region Eval
        /// <summary>
        /// Return the compiled expression
        /// </summary>
        public CompiledExpression<T> Compile()
        {
            var execTree = Expression.Lambda<Func<Func<string, object>, T>>(Expression.Convert(AST, typeof(T)), _ctxExpr);
            return new CompiledExpression<T>(execTree.Compile());
        }

        /// <summary>
        /// Shortcut for Compile().Evaluate()
        /// </summary>
        public T Evaluate()
        {
            return Compile().Evaluate();
        }

        /// <summary>
        /// Shortcut for Compile().Evaluate(context)
        /// </summary>
        /// <param name="context">The context data-object (or dictionary) whose properties (or keys) will be used as variable in the compiled expression.</param>
        public T Evaluate(object context)
        {
            return Compile().Evaluate(context);
        }

        /// <summary>
        /// Shortcut for Compile().Evaluate(resolver)
        /// </summary>
        /// <param name="resolver">The resolver delegate that accept the variable's name (string) and return its value. It should throw KeyNotFoundException if the variable name cannot be resolved.</param>
        public T Evaluate(Func<string, object> resolver)
        {
            return Compile().Evaluate(resolver);
        }
        #endregion

        #region Private
        private IEnumerable<string> GetVariables()
        {
            throw new NotImplementedException();
            //var visitor = new VariableEnumerator(ContextVariableName);
            //visitor.Visit(AST);
            //return visitor.Variables;
        }

        private static ParseTree ParseInternal(string expression)
        {
            var parseTree = Cache.Parser.Value.Parse(expression);
            if (parseTree.Errors.Count > 0)
            {
                var error = new FormatException("Syntax error at " + parseTree.Errors.First().Position);
                error.Data["syntax_error_details"] = parseTree.Errors.ToArray();
                throw error;
            }
            return parseTree;
        }
        #endregion
    }
}