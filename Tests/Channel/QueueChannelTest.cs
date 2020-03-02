using System.Drawing;
using System.Threading;
using jIAnSoft.Nami.Channels;
using jIAnSoft.Nami.Fibers;
using NUnit.Framework;

namespace Tests.Channel
{
    [TestFixture]
    public class QueueChannelTest
    {
        [Test]
        public void TestQueueChannel()
        {
            var fiber = new PoolFiber();
            fiber.Start();
            var channel = new QueueChannel<string>();
            channel.Subscribe(fiber, OnPublish);
            channel.Subscribe(fiber, OnMessage);
            channel.Publish("Fire 1");
            channel.Publish("Fire 2");
            channel.Publish("Fire 3");
            channel.Publish("Fire 4");
            channel.Publish("Fire 5");
            channel.Publish("Fire 6");
        }

        private static void OnPublish(string message)
        {
            TestContext.WriteLine($"OnPublish:{message}");
        }
        private static void OnMessage(string message)
        {
            TestContext.WriteLine($"OnMessage:{message}");
        }
    }
}
