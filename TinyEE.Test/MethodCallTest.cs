using System.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TinyEE.Test
{
    [TestFixture]
    public class MethodCallTest
    {
        [Test]
        [TestCase(@"""elizabeth"".ToUpper()", "ELIZABETH")]
        [TestCase(@"""elizabeth"".ToUpper().Length.ToString()", "9")]
        [TestCase(@"$str.Join(""-"", ""a"",""b"",""c"",""d"")", "a-b-c-d")]
        [TestCase(@"$str.Format(""{0} love {1}"", ""I"", ""you"")", "I love you")]
        [TestCase(@"$math.Max(10,12)", 12)]
        [TestCase(@"$math.Ceiling(4.5)", 5d)]
        [TestCase(@"$list.IndexOf([0,1,2,3,4,5,6], 3)", 3)]
        [TestCase(@"$list.IndexOf([0,1,2,3,4,5,6], 3, 4)", -1)]
        public void Valid(string expression, object expected)
        {
            var strFuncs = TEE.WrapFunctions(typeof(String));
            var mathFuncs = TEE.WrapFunctions(typeof(Math));
            var listFuncs = TEE.WrapFunctions(typeof (Array));
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression, new Dictionary<string,object>{ {"$str",strFuncs}, {"$math",mathFuncs}, {"$list",listFuncs} }));
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}