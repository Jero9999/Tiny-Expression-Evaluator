namespace TinyEE.JavaScript
{
    public static class JsExtension
    {
        /// <summary>
        /// Translates the privided expression to Javascript.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static string TranslateToJs(this ParsedExpression expression, JsTransformationOptions options = null)
        {
            return expression.ParseTree.GetJsExpr(options ?? new JsTransformationOptions());
        }
    }
}