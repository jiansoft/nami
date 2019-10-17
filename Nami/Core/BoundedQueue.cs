using System;
using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Queue with bounded capacity.  Will throw exception if capacity does not recede prior to wait time.
    /// </summary>
    public class BoundedQueue : IQueue
    {
        private readonly object _lock = new object();

        private List<Action> _actions = new List<Action>();


        private bool _disposed;
        //private readonly IExecutor _executor;

        private bool _running = true;
        private List<Action> _toPass = new List<Action>();

        ///<summary>
        /// Creates a bounded queue with a custom executor.
        ///</summary>
        public BoundedQueue()
        {
            MaxDepth = -1;
        }

        /// <summary>
        /// Max number of actions to be queued.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Max time to wait for space in the queue.
        /// </summary>
        public int MaxEnqueueWaitTimeInMs { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (!_running)
            {
                return;
            }

            lock (_lock)
            {
                if (!SpaceAvailable(1))
                {
                    return;
                }

                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        public int Count()
        {
            lock (_lock)
            {
                return _actions.Count;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            lock (_lock)
            {
                _running = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }

        public Action[] DequeueAll()
        {
            lock (_lock)
            {
                if (!ReadyToDequeue())
                {
                    return null;
                }

                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();

                Monitor.PulseAll(_lock);
                var toPass = _toPass.ToArray();
                return toPass;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool SpaceAvailable(int toAdd)
        {
            if (!_running)
            {
                return false;
            }

            while (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
            {
                if (MaxEnqueueWaitTimeInMs <= 0)
                {
                    throw new QueueFullException(_actions.Count);
                }

                Monitor.Wait(_lock, MaxEnqueueWaitTimeInMs);
                if (!_running)
                {
                    return false;
                }

                if (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
                {
                    throw new QueueFullException(_actions.Count);
                }
            }

            return true;
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }

            return _running;
        }

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
    }
}