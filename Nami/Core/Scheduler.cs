using System;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc cref="ISchedulerRegistry" />
    /// <summary>
    ///  Enqueue actions on to context after schedule elapses.  
    /// </summary>
    internal class Scheduler : ISchedulerRegistry, IScheduler
    {
        private volatile bool _running = true;
        private readonly IExecutionContext _fiber;
        private readonly Subscriptions _pending = new Subscriptions();

        ///<summary>
        /// Constructs new instance.
        ///</summary>
        public Scheduler(IExecutionContext fiber)
        {
            _fiber = fiber;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueue action on to context after timer elapses.  
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (firstInMs > 0)
            {
                return ScheduleOnInterval(action, firstInMs, Timeout.Infinite);
            }

            var pending = new PendingAction(action);

            if (_running)
            {
                Enqueue(pending.Execute);
            }

            return pending;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueue actions on to context after schedule elapses.  
        /// </summary>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var pending = new TimerAction(this, action, firstInMs, regularInMs);
            if (!_running)
            {
                return pending;
            }

            _pending.Add(pending);
            pending.Schedule();
            return pending;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Removes a pending scheduled action.
        /// </summary>
        /// <param name="toRemove"></param>
        public bool Remove(IDisposable toRemove)
        {
            return _pending.Remove(toRemove);
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueue actions on to context immediately.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _fiber.Enqueue(action);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            _running = false;
            _pending.Dispose();
            _disposed = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Cancels all pending actions
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}