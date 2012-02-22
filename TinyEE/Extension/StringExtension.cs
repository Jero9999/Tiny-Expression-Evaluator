namespace TinyEE.Extension
{
    public static class StringExtension
    {
        public static object Evaluate(this string expression)
        {
            return TEE.Evaluate(expression);
        }

        public static object Evaluate(this string expression, object context)
        {
            return TEE.Evaluate(expression, context);
        }
    }
}