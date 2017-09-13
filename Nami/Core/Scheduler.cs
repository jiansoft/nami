using System;
using System.Threading;

namespace jIAnSoft.Framework.Nami.Core
{
    /// <summary>
    ///  Enqueues actions on to context after schedule elapses.  
    /// </summary>
    public class Scheduler : ISchedulerRegistry, IScheduler, IDisposable
    {
        private volatile bool _running = true;

        private readonly IExecutionContext _executionContext;

        //private List<IDisposable> _pending = new List<IDisposable>();
        private readonly Subscriptions _disposabler = new Subscriptions();

        ///<summary>
        /// Constructs new instance.
        ///</summary>
        public Scheduler(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueues action on to context after timer elapses.  
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (firstInMs > 0) return ScheduleOnInterval(action, firstInMs, Timeout.Infinite);
            var pending = new PendingAction(action);
            _executionContext.Enqueue(pending.Execute);
            return pending;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueues actions on to context after schedule elapses.  
        /// </summary>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var pending = new TimerAction(this, action, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Removes a pending scheduled action.
        /// </summary>
        /// <param name="toRemove"></param>
        public void Remove(IDisposable toRemove)
        {
            _executionContext.Enqueue(() => _disposabler.Remove(toRemove));
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueues actions on to context immediately.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _executionContext.Enqueue(action);
        }

        private void AddPending(TimerAction pending)
        {
            _executionContext.Enqueue(() =>
            {
                if (!_running) return;
                _disposabler.Add(pending);
                pending.Schedule();
            });
        }

        /// <inheritdoc />
        /// <summary>
        ///  Cancels all pending actions
        /// </summary>
        public void Dispose()
        {
            _running = false;
            _disposabler.Dispose();
        }
    }
}