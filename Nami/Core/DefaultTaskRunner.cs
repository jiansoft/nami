using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core
{
    public class DefaultTaskRunner : IWorkThread
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _task;

        /// <summary>
        /// Queue a new action to run. If the previous task is still running, it will be cancelled and waited.
        /// </summary>
        /// <param name="action">The action to run.</param>
        public void Queue(Action action)
        {
            if (_task != null && !_task.IsCompleted)
            {
                _cts.Cancel();
                
                try
                {
                    _task.Wait(); // Optionally wait for clean exit
                }
                catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException))
                {
                    // Ignore cancellation exceptions
                }
            }

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _task = Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (OperationCanceledException)
                {
                    // Cancelled gracefully
                }
            }, token);
        }

        /// <summary>
        /// Not supported in task-based runner.
        /// </summary>
        public void Start()
        {
        }
    }
}