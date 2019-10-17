using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    public class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();
        private bool _running = true;
        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        public void Enqueue(Action action)
        {
            if (!_running)
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
                if (!ReadyToDequeue() || _disposed)
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

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }
            _disposed = true;
            Stop();
            _actions?.Clear();
            _toPass?.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}