using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    public class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeBlockingWait;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        ///<summary>
        /// BusyWaitQueue with custom executor.
        ///</summary>
        ///<param name="executor"></param>
        ///<param name="spinsBeforeTimeCheck"></param>
        ///<param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
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

        public List<Action> DequeueAll()
        {
            var spins = 0;
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(_lock))
                    {
                    }

                    if (!_running)
                    {
                        break;
                    }

                    var toReturn = TryDequeue();
                    if (toReturn != null)
                    {
                        return toReturn;
                    }

                    if (TryBlockingWait(stopwatch, ref spins))
                    {
                        if (!_running)
                        {
                            break;
                        }

                        toReturn = TryDequeue();
                        if (toReturn != null)
                        {
                            return toReturn;
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }

                Thread.Yield();
            }

            return null;
        }

        private bool TryBlockingWait(Stopwatch stopwatch, ref int spins)
        {
            if (spins++ < _spinsBeforeTimeCheck)
            {
                return false;
            }

            spins = 0;
            if (stopwatch.ElapsedMilliseconds <= _msBeforeBlockingWait)
            {
                return false;
            }

            Monitor.Wait(_lock);
            stopwatch.Restart();
            return true;
        }

        private List<Action> TryDequeue()
        {
            if (_actions.Count <= 0)
            {
                return null;
            }

            Lists.Swap(ref _actions, ref _toPass);
            _actions.Clear();
            return _toPass;
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