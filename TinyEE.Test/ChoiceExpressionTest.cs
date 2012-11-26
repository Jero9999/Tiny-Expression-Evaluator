using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class ChoiceExpressionTest
    {
        [Test]
        [TestCase("true ? 1 : 0", 1)]
        [TestCase("false ? 1 : 0", 0)]
        [TestCase(@"null ?: ""hello""", "hello")]
        public void Valid(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        [Test]
        [TestCase("100>10 ? Sum(1,2,3,-5) : Sum(-2,-3)", 1)]
        public void Chaining(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}