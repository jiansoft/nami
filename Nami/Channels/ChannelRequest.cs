using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Nami.Channels
{
    internal class ChannelRequest<TR, TM> : IRequest<TR, TM>, IReply<TM>
    {
        private readonly object _lock = new object();
        private readonly TR _req;
        private readonly Queue<TM> _resp = new Queue<TM>();
        private bool _disposed;

        public ChannelRequest(TR req)
        {
            _req = req;
        }

        public TR Request
        {
            get { return _req; }
        }

        public bool SendReply(TM response)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return false;
                }
                _resp.Enqueue(response);
                Monitor.PulseAll(_lock);
                return true;
            }
        }

        public bool Receive(int timeoutInMs, out TM result)
        {
            lock (_lock)
            {
                if (_resp.Count > 0)
                {
                    result = _resp.Dequeue();
                    return true;
                }
                if (_disposed)
                {
                    result = default(TM);
                    return false;
                }
                Monitor.Wait(_lock, timeoutInMs);
                if (_resp.Count > 0)
                {
                    result = _resp.Dequeue();
                    return true;
                }
            }
            result = default(TM);
            return false;
        }

        /// <summary>
        /// Stop receiving replies.
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
