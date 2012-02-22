using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using NUnit.Framework;
using TinyEE;

namespace Formy.Evaluation.Test
{
    [TestFixture]
    public class EvaluationTest
    {
        //TODO:test for integer overflow
        //TODO:test functions

        public struct Person
        {
            public string Name;
            public int Age;
            public bool Married;

            public string this[string key]
            {
                get { return key; }
            }

            public string this[long index]
            {
                get { return index.ToString(); }
            }
        }

        private object _context;

        [SetUp]
        public void Setup()
        {
            var dt = new DataTable("table01A");
            dt.Columns.AddRange(new[]
                                    {
                                        new DataColumn("col1"), 
                                        new DataColumn("col2"), 
                                        new DataColumn("col3"),
                                        new DataColumn("col4"),
                                        new DataColumn("col5")
                                    });
            dt.Rows.Add("one", 1, DateTime.Today.AddYears(1), true, 12.5m);
            dt.Rows.Add("two", 2, DateTime.Today.AddYears(2), false, 22.5m);
            dt.Rows.Add("three", 3, DateTime.Today.AddYears(3), true, 49.9999m);
            dt.Rows.Add("four", 4, new DateTime(2012,12,12,12,12,12), false, 0.125m);
            
            _context = new Dictionary<string,object>
                           {
                               {"x",42},
                               {"table01A", dt},
                               {"array01A", new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}},
                               {"elizabeth", new Person{ Name = "Elizabeth", Age=27, Married = true }},
                               {"lookup", new Dictionary<string, object> {{"key1", 1}, {"key2", null}}},
                               {"echo", new Person{Name = "echo"}},
                           };
        }

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
        [TestCase("Max(1,2)", 2)]
        [TestCase("x", 42)]
        [TestCase("elizabeth.Age", 27)]
        [TestCase("array01A.Length", 10)]
        [TestCase(@"lookup[""key1""]", 10)]
        [TestCase(@"echo[""anykey""]", "anykey")]
        [TestCase(@"echo[1048576]", "1048576")]
        [TestCase(@"array01A[9]", 9)]
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
        public void ShouldRunSingleExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate(expr, _context);
            actual.Should().Equals(expected);
        }

        [Test]
        [TestCase(@"echo.Name.Length", 4)]
        [TestCase(@"table01A.Rows[0][""col1""]", "one")]
        [TestCase(@"table01A.Rows[3][""col3""]", "12/12/2012 12:12:12 PM")]
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
        [TestCase("true and true and false", false)]
        [TestCase("false or false or true", true)]
        [TestCase("not (not true)", true)]
        [TestCase("If(100>10, Sum(Max(1,2),Max(0,1),Max(-5,3)), Sum(Min(-2,-3),Min(1,2)))", 6)]
        public void ShouldRunChainedExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate(expr, _context);
            actual.Should().Equals(expected);
        }

        [TestCase(@"""Hello""", "Hello")]
        [TestCase(@"""A Review of \""A Tale of Two Cities\"" and \""Moby Dick\""""", "A Review of \"A Tale of Two Cities\" and \"Moby Dick\"")]
        [TestCase(@"""Hello "" + ""world""", "Hello world")]
        [TestCase(@"""사랑해""", "사랑해")]
        [TestCase(@"""Hello "" + 1", "Hello 1")]
        [TestCase(@"""Hello"" = ""Hello""", true)]
        [TestCase(@"""Hello"" = ""Goodbye""", false)]
        public void ShouldHandleStrings(string expr, object expected)
        {
            var actual = TEE.Evaluate(expr, _context);
            actual.Should().Equals(expected);
        }

        [Test]
        [TestCase("2.1+1", 3.1)]
        [TestCase("2.1-1", 1.1)]
        [TestCase("8/(4/2)", 4)]
        [TestCase("2*3+1", 7)]
        [TestCase("2*(3+1)", 8)]
        [TestCase("4^2 + 2*4^1 + 1*4^0 >= 100/2 > 0", true)]
        [TestCase("1/2<1+1", true)]
        //[TestCase("table01A.Rows[1+2][\"col\" + 2] = 4", true)]
        public void ShouldRunComplexExpression(string expr, object expected)
        {
            var actual = TEE.Evaluate(expr, _context);
            actual.Should().Equals(expected);
        }
    }
}
