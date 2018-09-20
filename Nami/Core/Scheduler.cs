using System;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc cref="ISchedulerRegistry" />
    /// <summary>
    ///  Enqueues actions on to context after schedule elapses.  
    /// </summary>
    public class Scheduler : ISchedulerRegistry, IScheduler, IDisposable
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
        ///  Enqueues action on to context after timer elapses.  
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (firstInMs > 0)
            {
                return ScheduleOnInterval(action, firstInMs, Timeout.Infinite);
            }
            var pending = new PendingAction(action);
            _fiber.Enqueue(pending.Execute);
            return pending;
        }

        /// <inheritdoc />
        /// <summary>
        ///  Enqueue actions on to context after schedule elapses.  
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
            _fiber.Enqueue(() => _pending.Remove(toRemove));
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

        private void AddPending(TimerAction pending)
        {
            _fiber.Enqueue(() =>
            {
                if (!_running) return;
                _pending.Add(pending);
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
            _pending.Dispose();
        }
    }
}