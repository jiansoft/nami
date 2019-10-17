using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation that uses the .NET thread pool.
    /// </summary>
    public class DefaultThreadPool : IWorkThread
    {
        private bool _start;

        /// <inheritdoc />
        /// <summary>
        /// Run action.
        /// </summary>
        /// <param name="action"></param>
        public void Queue(Action action)
        {
            if (!_start)
            {
                return;
            }

            Task.Run(action);
        }

        public void Start()
        {
            _start = true;
        }
    }

    public class DefaultThread : IWorkThread
    {
        private Thread _thread;

        /// <inheritdoc />
        /// <summary>
        /// Run action.
        /// </summary>
        /// <param name="action"></param>
        public void Queue(Action action)
        {
            if (null != _thread && _thread.IsAlive)
            {
                _thread.Abort();
                _thread = null;
            }

            _thread = new Thread(() => { action(); })
            {
                Name = $"ThreadFiber-{DateTime.UtcNow.Ticks}",
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };

        }

        public void Start()
        {
            _thread.Start();
        }
    }
}