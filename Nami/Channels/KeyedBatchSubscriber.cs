using System;
using System.Collections.Generic;
using jIAnSoft.Framework.Nami.Core;
using jIAnSoft.Framework.Nami.Fibers;

namespace jIAnSoft.Framework.Nami.Channels
{
    /// <inheritdoc />
    /// <summary>
    /// Channel subscription that drops duplicates based upon a key.
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class KeyedBatchSubscriber<TK, T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly Action<IDictionary<TK, T>> _target;
        private readonly Converter<T, TK> _keyResolver;
        private readonly IFiber _fiber;
        private readonly long _intervalInMs;

        private Dictionary<TK, T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="intervalInMs"></param>
        public KeyedBatchSubscriber(Converter<T, TK> keyResolver, Action<IDictionary<TK, T>> target, IFiber fiber, long intervalInMs)
        {
            _keyResolver = keyResolver;
            _fiber = fiber;
            _target = target;
            _intervalInMs = intervalInMs;
        }

        ///<summary>
        /// Allows for the registration and deregistration of subscriptions
        ///</summary>
        public override ISubscriptionRegistry Subscriptions
        {
            get { return _fiber; }
        }

        /// <summary>
        /// received on delivery thread
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_batchLock)
            {
                var key = _keyResolver(msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<TK, T>();
                    _fiber.Schedule(Flush, _intervalInMs);
                }
                _pending[key] = msg;
            }
        }

        private void Flush()
        {
            var toReturn = ClearPending();
            if (toReturn != null)
            {
                _target(toReturn);
            }
        }

        private IDictionary<TK, T> ClearPending()
        {
            lock (_batchLock)
            {
                if (_pending == null || _pending.Count == 0)
                {
                    _pending = null;
                    return null;
                }
                IDictionary<TK, T> toReturn = _pending;
                _pending = null;
                return toReturn;
            }
        }
    }
}