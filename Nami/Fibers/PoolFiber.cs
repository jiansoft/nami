using jIAnSoft.Nami.Core;
using System;
using System.Linq;

namespace jIAnSoft.Nami.Fibers
{
    /// <inheritdoc />
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public class PoolFiber : IFiber
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IWorkThread _thread;
        private readonly IQueue _queue;
        private readonly IScheduler _scheduler;
        private readonly IExecutor _executor;
        private ExecutionState _state = ExecutionState.Created;
        private bool _flushPending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        public PoolFiber()
        {
            _queue = new DefaultQueue();
            _scheduler = new Scheduler(this);
            _thread = new DefaultThreadPool();
            _executor = new DefaultExecutor();
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_state != ExecutionState.Running)
            {
                return;
            }

            _queue.Enqueue(action);
            lock (_lock)
            {
            
                if (_flushPending)
                {
                    return;
                }

                _thread.Queue(Flush);
                _flushPending = true;
            }
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

        private void Flush()
        {
            var toExecute = _queue.DequeueAll();
            
            if (toExecute == null || !toExecute.Any())
            {
                _flushPending = false;
                return;
            }

            var copy = toExecute.ToArray();
            _thread.Queue(() => { _executor.Execute(copy); });
            
            lock (_lock)
            {
                if (_queue.Count() > 0)
                {
                    // don't monopolize thread.
                    _thread.Queue(Flush);
                }
                else
                {
                    _flushPending = false;
                }
            }
        }

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
        /// <returns></returns>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// Start consuming actions.
        /// </summary>
        public void Start()
        {
            if (_state == ExecutionState.Running)
            {
               return;
            }
            _state = ExecutionState.Running;
            _thread.Start();
            //flush any pending events in queue
            Enqueue(() => { });
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            _scheduler.Dispose();
            _state = ExecutionState.Stopped;
            _subscriptions.Dispose();
            _queue.Stop();
          
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }
            Stop();
            _queue.Dispose();
            _executor.Dispose();
            _disposed = true;
        }

        /// <inheritdoc />
        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}