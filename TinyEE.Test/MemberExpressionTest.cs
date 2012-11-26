using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class MemberExpressionTest
    {
        [Test]
        [TestCase("elizabeth.Age", 27)]
        [TestCase("array01A.Length", 10)]
        [TestCase(@"lookup[""key1""]", 1)]
        [TestCase(@"echo[""anykey""]", "anykey")]
        [TestCase(@"echo[1048576]", "1048576")]
        [TestCase(@"array01A[9]", 0)]
        [TestCase(@"echo.Name.Length", 4)]
        [TestCase(@"$usr.ToString()", "Anonymous(31337)")]
        [TestCase(@"table01A.Rows[0][""col1""]", "one")]
        [TestCase(@"table01A.Rows[0][""col1""].ToUpper()", "ONE")]
        [TestCase(@"table01A.Rows[0][""col1""].ToUpper().ToLower().Substring(1,2)", "ne")]
        [TestCase(@"table01A.Rows[3][""col3""]", "12/12/2012 12:12:12 PM")]
        public void Valid(string expression, object expected)
        {
            var vars = GetTestObject();
            var result = TEE.Evaluate<object>(expression, vars);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Invalid(string expression)
        {
        }

        public static Dictionary<string,object> GetTestObject()
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
            dt.Rows.Add("four", 4, new DateTime(2012, 12, 12, 12, 12, 12), false, 0.125m);

            return new Dictionary<string, object>
                       {
                           {"x",42},
                           {"table01A", dt},
                           {"array01A", new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}},
                           {"elizabeth", new Person{ Name = "Elizabeth", Age=27, Married = true }},
                           {"lookup", new Dictionary<string, object> {{"key1", 1}, {"key2", null}}},
                           {"echo", new Person{Name = "echo"}},
                           {"$usr", new Person{Name = "Anonymous", Age=31337}},
                       };
        }

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

            public override string ToString()
            {
                return Name + "(" + Age + ")";
            }
        }
    }
}