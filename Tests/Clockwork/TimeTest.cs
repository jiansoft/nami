using jIAnSoft.Nami.Clockwork;
using NUnit.Framework;

namespace Tests.Clockwork
{
    [TestFixture]
    public class TimeTest
    {
        [Test]
        public void TestTime()
        {
            var from = new BetweenTime(11, 0, 0, 99);
            var to = new BetweenTime(11, 45, 1, 999);

            TestContext.WriteLine($"from:{from} to:{to}");
            Assert.AreEqual(true, from.Before(to));
            Assert.AreEqual(false, from.After(to));
            Assert.AreEqual(false, from.Equal(to));
        }
    }
}