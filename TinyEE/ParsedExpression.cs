using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TinyEE.JavaScript;

namespace TinyEE
{
    public struct ParsedExpression<T>
    {
        internal ParsedExpression(string text)
        {
            if(text == null)
            {
                throw new ArgumentNullException("text");
            }
            _text = text;
            _parseTree = null;
            _variables = null;
        }

        #region Props
        private readonly string _text;
        public string Text
        {
            get { return _text ?? String.Empty; }
        }

        private IEnumerable<string> _variables;
        public IEnumerable<string> Variables
        {
            get { return _variables ?? (_variables = GetVariableNamesRecursive(ParseTree).Distinct()); }
        }

        private ParseTree _parseTree;

        internal ParseTree ParseTree
        {
            get { return _parseTree ?? (_parseTree = ParseInternal(Text)); }
        }

        internal Expression<Func<Func<string, object>, T>> SyntaxTree
        {
            get { return TransformInternal(ParseTree); }
        }
        #endregion

        #region Eval
        /// <summary>
        /// Compile
        /// </summary>
        public CompiledExpression<T> Compile()
        {
            return new CompiledExpression<T>(SyntaxTree.Compile());
        }

        /// <summary>
        /// Evaluates
        /// </summary>
        /// <returns></returns>
        public T Evaluate()
        {
            return Compile().Evaluate();
        }

        /// <summary>
        /// Evaluates the specified context.
        /// </summary>
        public T Evaluate(object context)
        {
            return Compile().Evaluate(context);
        }

        public T Evaluate(Func<string, object> contextFunctor)
        {
            return Compile().Evaluate(contextFunctor);
        }

        /// <summary>
        /// Translates the provided expression to Javascript.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public string TranslateToJs(JsTransformationOptions options = null)
        {
            return ParseTree.GetJsExpr(options ?? new JsTransformationOptions());
        }
        #endregion

        #region Private
        private static IEnumerable<string> GetVariableNamesRecursive(ParseNode node)
        {
            if (node.Token.Type == TokenType.Variable)
            {
                yield return node.Nodes[0].Token.Text;
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    foreach (var childNode in node.Nodes)
                    {
                        foreach (var varible in GetVariableNamesRecursive(childNode))
                        {
                            yield return varible;
                        }
                    }
                }
            }
        }

        private static ParseTree ParseInternal(string expression)
        {
            var parseTree = TEE.ParserInit.Value.Parse(expression);
            if (parseTree.Errors.Count > 0)
            {
                var error = new FormatException("Syntax error at" + parseTree.Errors.First().Position);
                error.Data["syntax_error_details"] = parseTree.Errors.ToArray();
                throw error;
            }
            return parseTree;
        }

        private static Expression<Func<Func<string, object>, T>> TransformInternal(ParseNode parseTree)
        {
            var contextExpr = Expression.Parameter(typeof(Func<string, object>), "context");
            var expressionTree = parseTree.GetAST(contextExpr);
            return Expression.Lambda<Func<Func<string, object>, T>>(Expression.Convert(expressionTree, typeof (T)), contextExpr);
        }
        #endregion

        #region Override Object
        public override string ToString()
        {
            return _text;
        }

        public string ToString(bool dumpAST)
        {
            return dumpAST ? SyntaxTree.ToString() : _text;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(ParsedExpression<T>)) return false;
            return Equals((ParsedExpression<T>)obj);
        }

        public bool Equals(ParsedExpression<T> other)
        {
            return Equals(other._text, _text);
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        }
        #endregion
    }
}