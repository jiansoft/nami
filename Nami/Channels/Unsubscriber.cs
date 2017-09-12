using System;
using jIAnSoft.Framework.Nami.Core;

namespace jIAnSoft.Framework.Nami.Channels
{
    internal class Unsubscriber<T> : IDisposable
    {
        private readonly Action<T> _receiver;
        private readonly Channel<T> _channel;
        private readonly ISubscriptionRegistry _subscriptions;

        public Unsubscriber(Action<T> receiver, Channel<T> channel, ISubscriptionRegistry subscriptions)
        {
            _receiver = receiver;
            _channel = channel;
            _subscriptions = subscriptions;
        }

        public void Dispose()
        {
            _channel.Unsubscribe(_receiver);
            _subscriptions.DeregisterSubscription(this);
        }
    }
}
