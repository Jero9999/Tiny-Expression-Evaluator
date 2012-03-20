namespace TinyEE.JavaScript
{
    public class JsTransformationOptions
    {
        public VariableMode VariableMode { get; set; }
        public string FunctionNamespace { get; set; }
        public string VariableCallbackName { get; set; }
        public bool ConvertFunctionNamesToLower { get; set; }
    }
}