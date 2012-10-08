using System;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using TinyEE;

namespace Formy.Evaluation.Test
{
    [TestFixture]
    public class EvaluationTest
    {
        //TODO:test for integer overflow
        //TODO:test each predefined functions
        [Test]
        [TestCase("null", null)]
        [TestCase("TRUE", true)]
        [TestCase("FalSe", false)]
        [TestCase("\"string\"", "string")]
        [TestCase("-1234567890", -1234567890)]
        [TestCase("+1234567890", 1234567890)]
        [TestCase("-1234567890.555", -1234567890.555)]
        [TestCase("+1234567890.555", 1234567890.555)]
        [TestCase("(123)", 123)]
        //[TestCase("Max(1,2)", 2)]
        [TestCase("x", 42)]
        [TestCase("elizabeth.Age", 27)]
        [TestCase("array01A.Length", 10)]
        [TestCase(@"lookup[""key1""]", 1)]
        [TestCase(@"echo[""anykey""]", "anykey")]
        [TestCase(@"echo[1048576]", "1048576")]
        [TestCase(@"array01A[9]", 0)]
        [TestCase("-5", -5)]
        [TestCase("--5", 5)]
        [TestCase("2^5", 32)]
        [TestCase("5*3", 15)]
        [TestCase("15/3", 5)]
        [TestCase("5+3", 8)]
        [TestCase("5-3", 2)]
        [TestCase("-5>-1", false)]
        [TestCase("-5<-1", true)]
        [TestCase("5>=1", true)]
        [TestCase("5<=5", true)]
        [TestCase("0=0", true)]
        [TestCase("0<>0", false)]
        [TestCase("NOT true", false)]
        [TestCase("NOT false", true)]
        [TestCase("true and false", false)]
        [TestCase("true or false", true)]
        [TestCase("", null)]
        [TestCase("true ? 1 : 0", 1)]
        [TestCase("false ? 1 : 0", 0)]
        [TestCase(@"null ?: ""hello""", "hello")]
        public void ShouldRunSingleExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr, TestUtils.GetTestContext());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase(@"echo.Name.Length", 4)]
        [TestCase(@"$usr.ToString()", "Anonymous(31337)")]
        [TestCase(@"table01A.Rows[0][""col1""]", "one")]
        [TestCase(@"table01A.Rows[0][""col1""].ToUpper()", "ONE")]
        [TestCase(@"table01A.Rows[0][""col1""].ToUpper().ToLower().Substring(1,2)", "ne")]
        [TestCase(@"table01A.Rows[3][""col3""]", "12/12/2012 12:12:12 PM")]
        [TestCase("elizabeth.ToString().ToUpper().Length.ToString()", "13")]
        [TestCase("1 + 2 + 3", 6)]
        [TestCase("8 - 5 - 3", 0)]
        [TestCase("1*2*3*4*5", 120)]
        [TestCase("8/4/2", 1)]
        [TestCase("2^2^2^2", 256)]
        [TestCase("-(--5)", -5)]
        [TestCase("2>1>0", true)]
        [TestCase("2<0<1", false)]
        [TestCase("0>=0>=0", true)]
        [TestCase("-5<=-1<=0", true)]
        [TestCase("1<>2<>3", true)]
        [TestCase("1=1=1", true)]
        [TestCase("5>=1<>0", true)]
        [TestCase("5>=1=0", false)]
        [TestCase("true and true and false", false)]
        [TestCase("false or false or true", true)]
        [TestCase("not (not true)", true)]
        [TestCase("100>10 ? Sum(1,2,3,-5) : Sum(-2,-3)", 1)]
        public void ShouldRunChainedExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr, TestUtils.GetTestContext());
            Assert.AreEqual(expected, actual);
        }
        
        [TestCase(@"""Hello "" + ""world""", "Hello world")]
        [TestCase(@"""Hello "" + 1", "Hello 1")]
        [TestCase(@"""Hello"" = ""Hello""", true)]
        [TestCase(@"""Hello"" = ""Goodbye""", false)]
        public void ShouldHandleStrings(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr, TestUtils.GetTestContext());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("2.1+1", 3.1)]
        [TestCase("2.1-1", 1.1)]
        [TestCase("8/(4/2)", 4)]
        [TestCase("2*3+1", 7)]
        [TestCase("2*(3+1)", 8)]
        [TestCase("4^2 + 2*4^1 + 1*4^0 >= 100/4 > 0", true)]
        [TestCase("1/2<1+1", true)]
        [TestCase("table01A.Rows[1+2][\"col\" + 2] = \"4\"", true)]
        public void ShouldRunComplexExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr, TestUtils.GetTestContext());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void ShouldFailWithKeyNotFoundExceptionWhenVariableNotFound()
        {
            var result = TEE.Evaluate<object>("a+b");
            Assert.Fail("Result:" + result);
        }

        [Test]
        public void ShouldRunFuncCallExpression()
        {
            var r1 = TEE.Evaluate<DateTime>("TODAY()");
            Assert.AreEqual(r1, DateTime.Today);

            var r2 = TEE.Evaluate<int>("SUM(1,2,3,4)");
            Assert.AreEqual(10, r2);
        }

        [Test]
        [ExpectedException]
        public void ShouldFailWhenCallingMethodWithIncorrectParam()
        {
            var r1 = TEE.Evaluate<BigInteger>("BIGINT(true)");
            Assert.Fail("R1: " + r1);
        }
    }
}