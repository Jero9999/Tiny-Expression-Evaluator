using System;
using System.Collections.Generic;
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
        [TestCase("4^2 + 2*4^1 + 1*4^0 >= 100/4 > 0")]
        [TestCase("1/2<1+1")]
        [TestCase("x + 1")]
        [TestCase("a.x + b.y")]
        public void CanParseExpression(string expression)
        {
            var p = TEE.Parse<object>(expression);
            Assert.IsNotNull(p);
            Assert.IsNotNull(p.Text);
            Assert.IsNotNull(p.AST);
            Assert.IsNotNull(p.Variables);
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        [TestCase("1a")]
        [TestCase("1 +* 2")]
        public void ParsingInvalidExpressionWillThrowFormatException(string expression)
        {
            TEE.Parse<object>(expression);
        }

        [Test]
        public void CanGetVariableList()
        {
            var p = TEE.Parse<bool>("z = (x + y)^3 = x^3 + 3*x^2*y + 3*x*y^2 + y^3");
            CollectionAssert.AreEquivalent(new[] { "x", "y", "z" }, p.Variables);

            var p2 = TEE.Parse<int>("12 + 23");
            CollectionAssert.IsEmpty(p2.Variables);
        }

        [Test]
        [TestCase("4^2 + 2*4^1 + 1*4^0 >= 100/4 > 0", true)]
        [TestCase("1/2<1+1", true)]
        [TestCase("", null)]
        public void CanCompileExpressions(string expression, object expected)
        {
            var ce = TEE.Compile<object>(expression);
            Assert.IsNotNull(ce);
            var result = ce.Evaluate(varName=> { throw new KeyNotFoundException(varName); });
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanReuseCompiledExpressionsWithDifferentValues()
        {
            var ce = TEE.Compile<bool>("x > y");
            Assert.AreEqual(true, ce.Evaluate(new { x = 2, y = 1.5 }));
            Assert.AreEqual(true, ce.Evaluate(new { x = 100.5, y = 1 }));
            Assert.AreEqual(false, ce.Evaluate(new { x = 2, y = 3 })); 
            Assert.AreEqual(true, ce.Evaluate(new Dictionary<string,object>(){ {"x", 3}, {"y",2} }));
            Assert.AreEqual(true, ce.Evaluate(v=>v=="x" ? 3 : v=="y" ? 2 : (object)null));
        }
    }
}