using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class GeneralTest
    {
        [Test]
        public void EmptyExpressionEvaluateToNull()
        {
            Assert.AreEqual(null, TEE.Evaluate<object>(""));
        }

        [Test]
        [TestCase("4^2 + 2*4^1 + 1*4^0 >= 100/4 > 0", true)]
        [TestCase("1/2<1+1", true)]
        [TestCase("table01A.Rows[1+2][\"col\" + 2] = \"4\"", true)]
        public void Valid(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr);
            Assert.AreEqual(expected, actual);
        }
    }
}