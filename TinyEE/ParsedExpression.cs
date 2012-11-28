using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TinyEE.JavaScript;

namespace TinyEE
{
    /// <summary>
    /// A wrapper around an expression's parse tree that expose the transformed abstract syntax tree.
    /// </summary>
    public struct ParsedExpression<T>
    {
        internal const string ContextVariableName = "_ctx";

        private readonly string _text;
        private IEnumerable<string> _variables;
        private ParseTree _parseTree;
        private Expression _ast;
        private ParameterExpression _ctxExpr;
        
        internal ParsedExpression(string text)
        {
            if(text == null)
            {
                throw new ArgumentNullException("text");
            }
            _text = text;
            _parseTree = null;
            _variables = null;
            _ast = null;
            _ctxExpr = null;
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

        internal ParseTree ParseTree
        {
            get { return _parseTree ?? (_parseTree = ParseInternal(Text)); }
        }

        internal ParameterExpression ContextExpression
        {
            //The parameter for the execution tree and the syntax tree needs to be the same object
            get { return _ctxExpr ?? (_ctxExpr = Expression.Parameter(typeof (Func<string, object>), ContextVariableName)); }
        }

        /// <summary>
        /// The abstract syntax tree for this expression
        /// </summary>
        public Expression AST
        {
            get { return _ast ?? (_ast = ParseTree.GetAST(ContextExpression)); }
        }
        #endregion

        #region Eval
        /// <summary>
        /// Return the compiled expression
        /// </summary>
        public CompiledExpression<T> Compile()
        {
            var execTree = Expression.Lambda<Func<Func<string, object>, T>>(Expression.Convert(AST, typeof(T)), ContextExpression);
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

        /// <summary>
        /// (NOT COMPLETED YET) Translates the provided expression to Javascript.
        /// </summary>
        internal string TranslateToJs(JsTransformationOptions options = null)
        {
            return ParseTree.GetJsExpr(options ?? new JsTransformationOptions());
        }

        #region Private
        private IEnumerable<string> GetVariables()
        {
            var visitor = new VariableEnumerator(ContextVariableName);
            visitor.Visit(AST);
            return visitor.Variables;
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