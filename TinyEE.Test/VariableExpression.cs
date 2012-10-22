using System.Collections.Generic;
using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class VariableExpression
    {
        [Test]
        public void Valid(string expression, object expected)
        {
        }

        [Test]
        public void Invalid(string expression)
        {
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailWithKeyNotFoundExceptionWhenVariableNotFound()
        {
            var result = TEE.Evaluate<object>("a+b");
            Assert.Fail("Result:" + result);
        }
    }
}