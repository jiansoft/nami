using jIAnSoft.Nami.Core;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public class BusyWaitQueueTest
    {
        [Test]
        public void TestBusyWaitQueue()
        {
            var q = new BusyWaitQueue(10, 10);
            q.Stop();
            q.Enqueue(() => { });
            TestContext.WriteLine($"q.Count():{q.Count()}");
            Assert.AreEqual(0, q.Count());
            q.Run();
            q.Enqueue(() => { });
            TestContext.WriteLine($"q.Count():{q.Count()}");
            Assert.AreEqual(1, q.Count());
            var r = q.DequeueAll();
            TestContext.WriteLine($"q.Count():{q.Count()} r.Count():{r.Length}");
            Assert.AreEqual(0, q.Count());
            Assert.AreEqual(1, r.Length);
        }
    }
}