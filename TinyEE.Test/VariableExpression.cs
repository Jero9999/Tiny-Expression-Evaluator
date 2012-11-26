using System.Collections.Generic;
using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class VariableExpression
    {
        [Test]
        public void CanGetVariableFromAnonObjectProp()
        {
            var result1 = TEE.Evaluate<bool>("a", new {a = true});
            Assert.IsTrue(result1);
            var result2 = TEE.Evaluate<int>("x", new { x = 12 });
            Assert.AreEqual(12, result2);
        }

        private class DataContainer
        {
            public int A { get; set; }
            public string B { get; set; }
        }

        [Test]
        public void CanGetVariableFromObjectProp()
        {
            var result = TEE.Evaluate<string>("B + A", new DataContainer(){ B = "Hello ", A = 10 });
            Assert.AreEqual("Hello 10", result);
        }

        [Test]
        public void CanGetVariableFromDictionary()
        {
            var result = TEE.Evaluate<string>("B + A", new Dictionary<string, object> { {"A", 10}, {"B", "Hello "} });
            Assert.AreEqual("Hello 10", result);
        }

        [Test]
        public void CanGetVariableFromResolver()
        {
            var result = TEE.Evaluate<string>("B + A", var=> var == "A" 
                                                                    ? 10 
                                                                    : var == "B" 
                                                                            ? "Hello " 
                                                                            : (object)null );
            Assert.AreEqual("Hello 10", result);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailWithKeyNotFoundExceptionWhenVariableNotFound()
        {
            var result = TEE.Evaluate<object>("a+b");
            Assert.Fail("Result:" + result);
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}