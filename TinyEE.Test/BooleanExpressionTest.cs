using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class BooleanExpressionTest
    {
        [Test]
        [TestCase("NOT true", false)]
        [TestCase("NOT false", true)]
        [TestCase("true and false", false)]
        [TestCase("true or false", true)]
        public void Valid(string expression, bool result)
        {
            Assert.AreEqual(result, TEE.Evaluate<bool>(expression));
        }

        [Test]
        [TestCase("true and true and false", false)]
        [TestCase("false or false or true", true)]
        [TestCase("not (not true)", true)]
        public void Chaining(string expression, bool result)
        {
            Assert.AreEqual(result, TEE.Evaluate<bool>(expression));
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}