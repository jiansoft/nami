using System;
using System.Threading;
using jIAnSoft.Framework.Nami.Core;

namespace jIAnSoft.Framework.Nami.Fibers
{
    /// <inheritdoc />
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public class PoolFiber : IFiber
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IThreadPool _pool;
        private readonly Scheduler _scheduler;
        private readonly IExecutor _executor;
        private readonly IQueue _queue;

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolFiber(IThreadPool pool, IExecutor executor)
        {
            _queue = new NewDefaultQueue();
            _scheduler = new Scheduler(this);
            _pool = pool;
            _executor = executor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a pool fiber with the default thread pool.
        /// </summary>
        public PoolFiber(IExecutor executor) 
            : this(new DefaultThreadPool(), executor)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Create a pool fiber with the default thread pool and default executor.
        /// </summary>
        public PoolFiber() 
            : this(new DefaultThreadPool(), new DefaultExecutor())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (_started == ExecutionState.Stopped)
            {
                return;
            }
            _queue.Enqueue(action);

            lock (_lock)
            {
                if (_started == ExecutionState.Created)
                {
                    return;
                }
                if (_flushPending) return;
                _pool.Queue(Flush);
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
            if (toExecute == null) return;
            _executor.Execute(toExecute);
            lock (_lock)
            {
                if (_queue.Count() > 0)
                {
                    // don't monopolize thread.
                    _pool.Queue(Flush);
                }
                else
                {
                    _flushPending = false;
                }
            }
        }


        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Framework.Nami.Core.IScheduler.Schedule(System.Action,System.Int64)" />
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
        /// <see cref="M:jIAnSoft.Framework.Nami.Core.IScheduler.ScheduleOnInterval(System.Action,System.Int64,System.Int64)" />
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
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            //flush any pending events in queue
            Enqueue(() => { });
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            _scheduler.Dispose();
            _started = ExecutionState.Stopped;
            _subscriptions.Dispose();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}