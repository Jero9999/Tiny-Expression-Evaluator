using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class BranchingExpressions
    {
        [Test]
        [TestCase("true ? 1 : 0", 1)]
        [TestCase("false ? 1 : 0", 0)]
        [TestCase(@"null ?: ""hello""", "hello")]
        [TestCase("100>10 ? Sum(1,2,3,-5) : Sum(-2,-3)", 1)]
        [TestCase(@"false 
                        ? 1 
                        : false 
                            ? 2 
                            : 3", 3)]
        public void Conditional(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        [Test]
        [TestCase(@"null ?: ""a string""", "a string")]
        [TestCase(@"null ?: true", true)]
        public void NullCoalescing(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}