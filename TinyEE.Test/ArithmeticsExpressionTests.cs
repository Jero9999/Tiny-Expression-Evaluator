using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class ArithmeticsExpressionTests
    {
        [Test]
        //Strings
        [TestCase(@"""Hello "" + ""world""", "Hello world")]
        [TestCase(@"""Hello "" + 1", "Hello 1")]
        //Negations
        [TestCase("--5", 5)]
        [TestCase("-5", -5)]
        [TestCase("-LONG(-255)", 255L)]
        [TestCase("-LONG(255)", -255L)]
        [TestCase("INT(-BIGINT(-255))", 255)]
        [TestCase("INT(-BIGINT(+255))", -255)]
        [TestCase("--1.1", 1.1)]
        [TestCase("-1.1", -1.1)]
        [TestCase("DOUBLE(-DECIMAL(-0.5))", 0.5)]
        [TestCase("DOUBLE(-DECIMAL(+0.5))", -0.5)]
        [TestCase("2^5", 32)]
        [TestCase("5*3", 15)]
        [TestCase("15/3", 5)]
        [TestCase("5+3", 8)]
        [TestCase("5-3", 2)]
        [TestCase("2.1+1", 3.1)]
        [TestCase("2.1-1", 1.1)]
        [TestCase("2 ^ -1", 0.5)]
        public void Valid(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        [Test]
        [TestCase("8/(4/2)", 4)]
        [TestCase("2*3+1", 7)]
        [TestCase("2*(3+1)", 8)]
        public void Chaining(string expression, object expected)
        {
        }

        [Test]
        [ExpectedException]
        [TestCase(@"""Hello "" - 1")]
        [TestCase(@"""Hello "" * 1")]
        [TestCase(@"""Hello "" / 1")]
        [TestCase("1 / 0")]
        public void Invalid(string expression)
        {
            TEE.Evaluate<object>(expression);
        }
    }
}