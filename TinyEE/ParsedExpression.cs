using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TinyEE
{
    public struct ParsedExpression
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
            get { return _text ?? string.Empty; }
        }

        private ParseTree _parseTree;
        internal ParseTree ParseTree
        {
            get { return _parseTree ?? (_parseTree = ParseInternal(Text)); }
        }

        private IEnumerable<string> _variables;
        public IEnumerable<string> Variables
        {
            get { return _variables ?? (_variables = GetVariableNamesRecursive(ParseTree).Distinct()); }
        }

        internal Expression<Func<Func<string, object>, object>> SyntaxTree
        {
            get { return TransformInternal(ParseTree); }
        } 
        #endregion

        #region Eval
        public CompiledExpression Compile()
        {
            return new CompiledExpression(SyntaxTree.Compile());
        }

        public object Evaluate()
        {
            return Compile().Evaluate(ContextFunctor.ZeroVariable);
        }

        public object Evaluate(object context)
        {
            return Compile().Evaluate(ContextFunctor.GetForObject(context));
        }

        public object Evaluate(Func<string, object> contextFunctor)
        {
            return Compile().Evaluate(contextFunctor);
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

        private static readonly Lazy<Parser> ParserInit = new Lazy<Parser>(()=>new Parser(new Scanner()), true);

        private static ParseTree ParseInternal(string expression)
        {
            var parseTree = ParserInit.Value.Parse(expression);
            if (parseTree.Errors.Count > 0)
            {
                var error = new ArgumentException("Syntax error");
                error.Data["details"] = parseTree.Errors.ToList();
                throw error;
            }
            return parseTree;
        }

        private static Expression<Func<Func<string, object>, object>> TransformInternal(ParseNode parseTree)
        {
            var contextExpr = Expression.Parameter(typeof(Func<string, object>), "context");
            var expressionTree = parseTree.GetAST(contextExpr);
            return Expression.Lambda<Func<Func<string, object>, object>>(
                Expression.TypeAs(expressionTree, typeof(object)),
                contextExpr);
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
            if (obj.GetType() != typeof(ParsedExpression)) return false;
            return Equals((ParsedExpression)obj);
        }

        public bool Equals(ParsedExpression other)
        {
            return Equals(other.Text, Text);
        }

        public override int GetHashCode()
        {
            return _text.GetHashCode();
        } 
        #endregion
    }
}