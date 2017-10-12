using jIAnSoft.Framework.Nami.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace jIAnSoft.Framework.Nami.Fibers
{
    /// <inheritdoc />
    /// <summary>
    /// Fiber implementation backed by a dedicated thread.
    /// <see cref="T:jIAnSoft.Framework.Nami.Fibers.IFiber" />
    /// </summary>
    public class ThreadFiber : IFiber
    {
        private readonly object _lock = new object();
        private static int _threadCount;
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly Thread _thread;
        private readonly IQueue _queue;
        private readonly Scheduler _scheduler;
        private readonly IExecutor _executor;
        private ExecutionState _started = ExecutionState.Created;

        /// <inheritdoc />
        /// <summary>
        /// Create a thread fiber with the default queue.
        /// </summary>
        public ThreadFiber()
            : this(new DefaultQueue())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a thread fiber with a specified queue.
        /// </summary>
        /// <param name="queue"></param>
        public ThreadFiber(IQueue queue)
            : this(queue, $"ThreadFiber-{GetNextThreadId()}")
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a thread fiber with a specified name.
        /// </summary>
        /// /// <param name="threadName"></param>
        public ThreadFiber(string threadName)
            : this(new DefaultQueue(), threadName)
        {
        }


        /// <summary>
        /// Creates a thread fiber.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        /// <param name="priority"></param>
        public ThreadFiber(IQueue queue, string threadName, bool isBackground = true,
            ThreadPriority priority = ThreadPriority.Normal)
        {
            _queue = queue;
            _executor = new DefaultExecutor();
            _scheduler = new Scheduler(this);
            _thread = new Thread(RunThread)
            {
                Name = threadName,
                IsBackground = isBackground,
                Priority = priority
            };
        }

        private void RunThread()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref _threadCount);
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteNextBatch()
        {
            var toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }

        private List<Action> DequeueAll()
        {
            lock (_lock)
            {
                if (!ReadyToDequeue()) return null;
                var toPass = _queue.DequeueAll();
                return toPass;
            }
        }

        private bool ReadyToDequeue()
        {
            var running = _started == ExecutionState.Running;
            while (_queue.Count() == 0 && running)
            {
                Monitor.Wait(_lock);
            }
            return running;
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _queue.Enqueue(action);
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
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
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Framework.Nami.Fibers.IFiber.Start" />
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            _thread.Start();
        }

        public void Stop()
        {
            lock (_lock)
            {
                _started = ExecutionState.Stopped;
                _scheduler.Dispose();
                _subscriptions.Dispose();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Stops the thread.
        /// </summary>
        public void Dispose()
        {
            _scheduler.Dispose();
            _subscriptions.Dispose();
            _queue.Stop();
        }
    }
}