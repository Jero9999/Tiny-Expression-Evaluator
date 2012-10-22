using NUnit.Framework;

namespace TinyEE.Test
{
    [TestFixture]
    public class ComparisionExpressionTest
    {
        [Test]
        [TestCase(@"""Hello"" = ""Hello""", true)]
        [TestCase(@"""Hello"" = ""Goodbye""", false)]
        [TestCase("-5>-1", false)]
        [TestCase("-5<-1", true)]
        [TestCase("5>=1", true)]
        [TestCase("5<=5", true)]
        [TestCase("0=0", true)]
        [TestCase("0<>0", false)]
        public void Valid(string expression, bool result)
        {
        }

        [Test]
        [TestCase("2>1>0", true)]
        [TestCase("2<0<1", false)]
        [TestCase("0>=0>=0", true)]
        [TestCase("-5<=-1<=0", true)]
        [TestCase("1<>2<>3", true)]
        [TestCase("1=1=1", true)]
        [TestCase("5>=1<>0", true)]
        [TestCase("5>=1=0", false)]
        public void Chaining(string expression, bool result)
        {
            
        }

        [Test]
        public void Invalid(string expression)
        {
        }
    }
}