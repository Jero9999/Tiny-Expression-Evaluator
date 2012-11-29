using System;
using System.Collections.Generic;
using System.Net.Mail;
using NUnit.Framework;

namespace TinyEE.Test
{
    public class LiteralExpressionTest
    {
        [Test]
        [TestCase("null", null)]
        [TestCase("false", false)]
        [TestCase("FalSe", false)]
        [TestCase("true", true)]
        [TestCase("TruE", true)]
        [TestCase("-2147483648", -2147483648)]
        [TestCase("2147483647", 2147483647)]
        [TestCase("-0", 0)]
        [TestCase("+0", 0)]
        [TestCase("-0.1234", -0.1234)]
        [TestCase("+0.1234", 0.1234)]
        [TestCase(@"""Hello""", "Hello")]
        [TestCase(@"""A Review of \""A Tale of Two Cities\"" and \""Moby Dick\""""", "A Review of \"A Tale of Two Cities\" and \"Moby Dick\"")]
        [TestCase(@"""사랑해""", "사랑해")]
        public void PrimitiveLiteral(string expr, object expected)
        {
            var actual = TEE.Evaluate<object>(expr);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("0..12", 0, 12, 13)]
        [TestCase("1..12", 1, 12, 12)]
        public void IntRangeLiteral(string range, int left, int right, int count)
        {
            var range1 = TEE.Evaluate<Range<int>>(range);
            Assert.AreEqual(left, range1.Left);
            Assert.AreEqual(right, range1.Right);
            Assert.AreEqual(count, range1.Size);
            var range2 = Range<int>.Numeric(left, right);
            Assert.AreEqual(range1, range2);
        }

        [Test]
        public void ListLiteral()
        {
            var list1 = TEE.Evaluate<object[]>("[1,2,3,4,5,6,7]");
            Assert.IsNotNull(list1);
            Assert.AreEqual(7, list1.Length);

            var list2 = TEE.Evaluate<object[]>(@"[""adam"",""eve"",""cain"",""abel""]");
            Assert.IsNotNull(list2);
            Assert.AreEqual(4, list2.Length);

            var list3 = TEE.Evaluate<object[]>("[true, false, true]");
            Assert.IsNotNull(list3);
            Assert.AreEqual(3, list3.Length);

            var list4 = TEE.Evaluate<object[]>(@"[1, false, """"]");
            Assert.IsNotNull(list4);
            Assert.AreEqual(3, list4.Length);

            var list5 = TEE.Evaluate<object[]>(@"[1, [1,2,3], { a:1,b:2 }]");
            Assert.IsNotNull(list5);
            Assert.AreEqual(3, list5.Length);
            Assert.AreEqual(typeof(int), list5[0].GetType());
            Assert.AreEqual(typeof(object[]), list5[1].GetType());
            Assert.AreEqual(typeof(Dictionary<string,object>), list5[2].GetType());
        }

        [Test]
        public void HashLiteral()
        {
            var dict0 = TEE.Evaluate<IDictionary<string, object>>("{}");
            Assert.IsNotNull(dict0);
            Assert.AreEqual(0, dict0.Count);

            var dict1 = TEE.Evaluate<IDictionary<string, object>>(@"{ a:1,b:2,c:3,d:usr, e:array }", new{ usr=new SmtpClient(), array=new[]{1,2,3,4,5} });
            Assert.IsNotNull(dict1);
            Assert.AreEqual(5, dict1.Count);
            CollectionAssert.AreEquivalent(new[] { "a", "b", "c", "d", "e" }, dict1.Keys);
            Assert.AreEqual(typeof(int), dict1["a"].GetType());
            Assert.AreEqual(typeof(int[]), dict1["e"].GetType());
            Assert.AreEqual(typeof(SmtpClient), dict1["d"].GetType());

            var dict2 = TEE.Evaluate<IDictionary<string, object>>(@"{ a:{ b:{ c:{ d:{} } } } }");
            Assert.IsNotNull(dict2);
            Assert.IsTrue(dict2.ContainsKey("a"));

            var dict2_a = dict2["a"] as IDictionary<string, object>;
            Assert.IsNotNull(dict2_a);
            Assert.IsTrue(dict2_a.ContainsKey("b"));

            var dict2_a_b = dict2_a["b"] as IDictionary<string, object>;
            Assert.IsNotNull(dict2_a_b);
            Assert.IsTrue(dict2_a_b.ContainsKey("c"));

            var dict2_a_b_c = dict2_a_b["c"] as IDictionary<string, object>;
            Assert.IsNotNull(dict2_a_b_c);
            Assert.IsTrue(dict2_a_b_c.ContainsKey("d"));

            var dict2_a_b_c_d = dict2_a_b_c["d"] as IDictionary<string, object>;
            Assert.IsNotNull(dict2_a_b_c_d);
            Assert.AreEqual(0, dict2_a_b_c_d.Count);
        }

        [Test]
        [ExpectedException(typeof(OverflowException))]
        [TestCase("-2147483649")]
        [TestCase("2147483648")]
        [TestCase("2147483647 + 1")]
        [TestCase("-2147483648 - 1")]
        public void IntLiteralOverflow(string expr)
        {
            var result = TEE.Evaluate<object>(expr);
            Assert.Fail("Result:" + result);
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}