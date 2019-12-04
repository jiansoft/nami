
using jIAnSoft.Nami.Core;
using NUnit.Framework;

namespace Tests.Core
{
    [TestFixture]
    public class DefaultQueueTest
    {
        [Test]
        public void TestDefaultQueue()
        {
            var q = new DefaultQueue();
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
            Assert.AreEqual(0,  q.Count());
            Assert.AreEqual(1, r.Length);
        }
        
        [Test]
        public void TestEnqueue()
        {
            var q = new DefaultQueue();
            q.Enqueue(() => { });
            TestContext.WriteLine($"q.Count():{q.Count()}");
            Assert.AreEqual(1, q.Count());
        }

        [Test]
        public void TestDequeueAll()
        {
            var q = new DefaultQueue();
            q.Run();
           
            q.Enqueue(() => { });
            q.Enqueue(() => { });
            q.Enqueue(() => { });
            var r = q.DequeueAll();
            TestContext.WriteLine($"q.Count():{q.Count()} r.Count():{r.Length}");
            Assert.AreEqual(3, r.Length);
        }
    }
}