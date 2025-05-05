using jIAnSoft.Nami.Core;
using System;
using System.Threading;

namespace jIAnSoft.Nami.Fibers
{
    /// <summary>
    /// Fiber implementation backed by a dedicated thread.
    /// <see cref="T:jIAnSoft.Nami.Fibers.IFiber" />
    /// </summary>
    public class ThreadFiber : IFiber
    {
        private readonly object _lock = new object();
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly IWorkThread _thread;
        private readonly IQueue _queue;
        private readonly IScheduler _scheduler;
        private readonly IExecutor _executor;
        private ExecutionState _state = ExecutionState.Created;
        private int _disposed; // 0=false, 1=true
        
        /// <summary>
        /// Create a thread fiber with the default queue.
        /// </summary>
        public ThreadFiber()
            : this(new DefaultQueue())
        {
        }

        /// <summary>
        /// Creates a thread fiber with a specified queue.
        /// </summary>
        /// <param name="queue"></param>
        public ThreadFiber(IQueue queue)
        {
            _queue = queue;
            _executor = new DefaultExecutor();
            _scheduler = new Scheduler(this);
            _thread = new DefaultThread();
            _thread.Queue(RunOnThread);
        }

        private void RunOnThread()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteNextBatch()
        {
            var toExecute = _queue.DequeueAll();
            if (toExecute == null || toExecute.Length == 0)
            {
                return false;
            }
            
            _executor.Execute(toExecute);
            return true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_state == ExecutionState.Stopped || _disposed == 1)
            {
                return;
            }

            _queue.Enqueue(action);
        }

        /// <inheritdoc />
        /// <summary>
        ///  Register subscription to be unsubcribed from when the fiber is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        /// <inheritdoc />
        /// <summary>
        ///  Deregister a subscription.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        ///<summary>
        /// Number of subscriptions.
        ///</summary>
        public int NumSubscriptions => _subscriptions.Count;

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Core.IScheduler.Schedule(System.Action,System.Int64)" />
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _scheduler.Schedule(action, firstInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Core.IScheduler.ScheduleOnInterval(System.Action,System.Int64,System.Int64)" />
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Fibers.IFiber.Start" />
        /// </summary>
        public void Start()
        {
            if (_disposed == 1 || _state == ExecutionState.Running)
            {
                return;
            }

            _state = ExecutionState.Running;
            _thread.Start();
        }

        public void Stop()
        {
            if (_disposed == 1)
            {
                return;
            }

            lock (_lock)
            {
                _state = ExecutionState.Stopped;
                _scheduler.Dispose();
                _subscriptions.Dispose();
                _queue.Stop();
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            if (!disposing)
            {
                return;
            }

            Stop();
            _queue.Dispose();
            _executor.Dispose();
        }

        /// <summary>
        /// Stops the thread and releases resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

    }
}