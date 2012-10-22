using System;
using System.Numerics;
using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class BuiltInFunctionsTests
    {
        [Test]
        [TestCase(@"BOOLEAN(""false"")", typeof(bool))]
        [TestCase(@"BOOLEAN(false)", typeof(bool))]
        [TestCase(@"INT(1)", typeof(int))]
        [TestCase(@"INT(1.0)", typeof(int))]
        [TestCase(@"INT(LONG(1))", typeof(int))]
        [TestCase(@"INT(DECIMAL(1))", typeof(int))]
        [TestCase(@"INT(BIGINT(1))", typeof(int))]
        [TestCase(@"INT(""1"")", typeof(int))]
        [TestCase(@"DOUBLE(1)", typeof(double))]
        [TestCase(@"DOUBLE(1.0)", typeof(double))]
        [TestCase(@"DOUBLE(LONG(1))", typeof(double))]
        [TestCase(@"DOUBLE(DECIMAL(1))", typeof(double))]
        [TestCase(@"DOUBLE(BIGINT(1))", typeof(double))]
        [TestCase(@"DOUBLE(""1"")", typeof(double))]
        [TestCase(@"LONG(1)", typeof(long))]
        [TestCase(@"LONG(1.0)", typeof(long))]
        [TestCase(@"LONG(LONG(1))", typeof(long))]
        [TestCase(@"LONG(DECIMAL(1))", typeof(long))]
        [TestCase(@"LONG(BIGINT(1))", typeof(long))]
        [TestCase(@"LONG(""1"")", typeof(long))]
        [TestCase(@"DECIMAL(1)", typeof(decimal))]
        [TestCase(@"DECIMAL(1.0)", typeof(decimal))]
        [TestCase(@"DECIMAL(LONG(1))", typeof(decimal))]
        [TestCase(@"DECIMAL(DECIMAL(1))", typeof(decimal))]
        [TestCase(@"DECIMAL(BIGINT(1))", typeof(decimal))]
        [TestCase(@"DECIMAL(""1"")", typeof(decimal))]
        [TestCase(@"BIGINT(1)", typeof(BigInteger))]
        [TestCase(@"BIGINT(1.0)", typeof(BigInteger))]
        [TestCase(@"BIGINT(LONG(1))", typeof(BigInteger))]
        [TestCase(@"BIGINT(DECIMAL(1))", typeof(BigInteger))]
        [TestCase(@"BIGINT(BIGINT(1))", typeof(BigInteger))]
        [TestCase(@"BIGINT(""1"")", typeof(BigInteger))]
        [TestCase(@"STR(true)", typeof(string))]
        [TestCase(@"STR(1)", typeof(string))]
        [TestCase(@"STR(1.0)", typeof(string))]
        [TestCase(@"STR(LONG(1))", typeof(string))]
        [TestCase(@"STR(DECIMAL(1))", typeof(string))]
        [TestCase(@"STR(BIGINT(1))", typeof(string))]
        [TestCase(@"STR(TODAY())", typeof(string))]
        [TestCase(@"STR(DURATION(""12:12:00""))", typeof(string))]
        [TestCase(@"STR("""")", typeof(string))]
        [TestCase(@"STR(null)", typeof(string))]
        [TestCase(@"TIME(""2012-12-12"")", typeof(DateTime))]
        [TestCase(@"DURATION(""12:12:00"")", typeof(TimeSpan))]
        [TestCase(@"URI(""http://localhost/"")", typeof(Uri))]
        [TestCase(@"BASE64DECODE(""TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFuY2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxlIGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhlbWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4="")", typeof(byte[]))]
        [TestCase(@"BASE64ENCODE(BASE64DECODE(""TWFuIGlzIGRpc3Rpbmd1aXNoZWQsIG5vdCBvbmx5IGJ5IGhpcyByZWFzb24sIGJ1dCBieSB0aGlzIHNpbmd1bGFyIHBhc3Npb24gZnJvbSBvdGhlciBhbmltYWxzLCB3aGljaCBpcyBhIGx1c3Qgb2YgdGhlIG1pbmQsIHRoYXQgYnkgYSBwZXJzZXZlcmFuY2Ugb2YgZGVsaWdodCBpbiB0aGUgY29udGludWVkIGFuZCBpbmRlZmF0aWdhYmxlIGdlbmVyYXRpb24gb2Yga25vd2xlZGdlLCBleGNlZWRzIHRoZSBzaG9ydCB2ZWhlbWVuY2Ugb2YgYW55IGNhcm5hbCBwbGVhc3VyZS4=""))", typeof(string))]
        public void Conversion(string expression, Type expectedType)
        {
            var actual = TEE.Evaluate<object>(expression);
            Assert.AreEqual(expectedType, actual.GetType());
        }

        [Test]
        [TestCase("TIME().GetType()", typeof(DateTime))]
        [TestCase("TODAY().GetType()", typeof(DateTime))]
        [TestCase("RANDOM_INT().GetType()", typeof(int))]
        [TestCase("RANDOM_DOUBLE().GetType()", typeof(double))]
        [TestCase("GUID().GetType()", typeof(Guid))]
        [TestCase(@"SIZE(DATES(""2012-10-01"",""2012-11-01""))", 32)]
        [TestCase(@"SUM(1,2,3,4)", 10)]
        [TestCase(@"SUM([1,2,3,4])", 10)]
        public void Valid(string expression, object expected)
        {
            Assert.AreEqual(expected, TEE.Evaluate<object>(expression));
        }

        //TODO:test more failing cases
        [Test]
        [ExpectedException]
        [TestCase("BIGINT(true)")]
        public void Invalid(string expression)
        {
            TEE.Evaluate<object>(expression);
        }
    }
}