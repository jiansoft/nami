using System;
using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    public class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();
        private volatile bool _running = true;
        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();
        private int _disposed;
        
        public void Enqueue(Action action)
        {

            if (!_running || _disposed == 1)
            {
                return;
            }
            
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public Action[] DequeueAll()
        {
            lock (_lock)
            {
                if (_disposed == 1 || !ReadyToDequeue())
                {
                    return null;
                }

                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                var toPass = _toPass.ToArray();
                return toPass;
            }
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }

            return _running;
        }

        public int Count()
        {
            lock (_lock)
            {
                return _actions.Count;
            }
        }

        public void Run()
        {
            lock (_lock)
            {
                _running = true;
                Monitor.PulseAll(_lock);
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }
        
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }
            
            Stop();
            lock (_lock)
            {
                _actions.Clear();
                _toPass.Clear();
                Monitor.PulseAll(_lock);
            }
        }
    }
}