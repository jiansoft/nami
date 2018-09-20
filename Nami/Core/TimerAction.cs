using System;
using System.Threading;

namespace jIAnSoft.Nami.Core
{
    internal sealed class TimerAction : IDisposable
    {
        private readonly ISchedulerRegistry _fiber;
        private Action _action;
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;

        private Timer _timer;
        private bool _cancelled;

        public TimerAction(ISchedulerRegistry fiber, Action action, long firstIntervalInMs, long intervalInMs)
        {
            _fiber = fiber;
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public void Schedule()
        {
            _timer = new Timer(x => ExecuteOnTimerThread(), null, _firstIntervalInMs, _intervalInMs);
        }

        private void ExecuteOnTimerThread()
        {
            if (_intervalInMs == Timeout.Infinite || _cancelled)
            {
                _fiber.Remove(this);
                var timer = Interlocked.Exchange(ref _timer, null);
                timer?.Dispose();
            }
            _fiber.Enqueue(ExecuteOnFiberThread);
        }

        private void ExecuteOnFiberThread()
        {
            if (_cancelled)
                return;
            _action();
        }

        public void Dispose()
        {
            _cancelled = true;
            _action = null;
            _fiber.Remove(this);
            var timer = Interlocked.Exchange(ref _timer, null);
            timer?.Dispose();
        }
    }
}