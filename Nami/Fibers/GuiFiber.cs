using jIAnSoft.Nami.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace jIAnSoft.Nami.Fibers
{
    /// <inheritdoc />
    /// <summary>
    ///  Allows interaction with Windows Forms.  Transparently moves actions onto the Form's thread.
    /// </summary>
    public class GuiFiber : IFiber
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IExecutionContext _executionContext;
        private readonly Scheduler _timer;
        private readonly IExecutor _executor;
        private readonly List<Action> _queue = new List<Action>();

        private volatile ExecutionState _started = ExecutionState.Created;

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public GuiFiber(IExecutionContext executionContext, IExecutor executor)
        {
            _timer = new Scheduler(this);
            _executionContext = executionContext;
            _executor = executor;
        }

        /// <inheritdoc />
        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            switch (_started)
            {
                case ExecutionState.Stopped:
                    return;
                case ExecutionState.Created:
                {
                    lock (_lock)
                    {
                        if (_started == ExecutionState.Created)
                        {
                            _queue.Add(action);
                            return;
                        }
                    }

                    break;
                }
                case ExecutionState.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _executionContext.Enqueue(() => _executor.Execute(action));
        }

        ///<summary>
        /// Register subscription to be unsubcribed from when the fiber is disposed.
        ///</summary>
        ///<param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        ///<summary>
        /// Deregister a subscription.
        ///</summary>
        ///<param name="toRemove"></param>
        ///<returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        ///<summary>
        /// Number of subscriptions.
        ///</summary>
        public int NumSubscriptions
        {
            get { return _subscriptions.Count; }
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Core.IScheduler.Schedule(System.Action,System.Int64)" />
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _timer.Schedule(action, firstInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Core.IScheduler.ScheduleOnInterval(System.Action,System.Int64,System.Int64)" />
        /// </summary>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:jIAnSoft.Nami.Fibers.IFiber.Start" />
        /// </summary>
        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }

            lock (_lock)
            {
                var actions = _queue.ToList();
                _queue.Clear();
                if (actions.Count > 0)
                {
                    _executionContext.Enqueue(() => _executor.Execute(actions));
                }
                _started = ExecutionState.Running;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose" />
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Stop()
        {
            _timer.Dispose();
            _started = ExecutionState.Stopped;
            _subscriptions.Dispose();
        }
    }
}