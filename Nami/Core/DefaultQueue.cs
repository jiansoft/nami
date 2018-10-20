using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    public class DefaultQueue : IQueue
    {
        private object _lock = new object();
        private bool _running = true;
        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public List<Action> DequeueAll()
        {
            lock (_lock)
            {
                if (!ReadyToDequeue())
                {
//                    if (_disposed)
//                    {
//                        _actions = null;
//                        _toPass = null;
//                    }
                    return null;
                }

                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
        }

        private bool ReadyToDequeue()
        {
            while (!_actions.Any() && _running)
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
        }

        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
            }
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            Stop();
            Enqueue(() => { });
            _actions?.Clear();
            _toPass?.Clear();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}