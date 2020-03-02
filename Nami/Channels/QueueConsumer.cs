using jIAnSoft.Nami.Core;
using System;

namespace jIAnSoft.Nami.Channels
{
    internal class QueueConsumer<T> : IDisposable
    {
        private readonly object _mutex = new object();
        private bool _flushPending;
        private readonly IExecutionContext _target;
        private readonly Action<T> _callback;
        private readonly QueueChannel<T> _channel;

        public QueueConsumer(IExecutionContext target, Action<T> callback, QueueChannel<T> channel)
        {
            _target = target;
            _callback = callback;
            _channel = channel;
        }

        private void Signal()
        {
            lock (_mutex)
            {
                if (_flushPending)
                {
                    return;
                }
                _target.Enqueue(ConsumeNext);
                _flushPending = true;
            }
        }

        private void ConsumeNext()
        {
            try
            {
                if (_channel.Pop(out var msg))
                {
                    _callback(msg);
                }
            }
            finally
            {
                lock (_mutex)
                {
                    if (_channel.Count == 0)
                    {
                        _flushPending = false;
                    }
                    else
                    {
                        _target.Enqueue(ConsumeNext);
                    }
                }
            }
        }

        public void Dispose()
        {
            _channel.SignalEvent -= Signal;
        }

        internal void Subscribe()
        {
            _channel.SignalEvent += Signal;
        }
    }
}
