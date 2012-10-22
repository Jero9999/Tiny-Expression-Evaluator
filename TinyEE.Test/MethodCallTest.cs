using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class MethodCallTest
    {
        [Test]
        [TestCase("elizabeth.ToString().ToUpper().Length.ToString()", "13")]
        public void Valid(string expression, object expected)
        {
        }

        [Test]
        public void Invalid(string expression)
        {
            
        }
    }
}