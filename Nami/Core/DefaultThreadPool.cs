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
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public void Queue(Action action)
        {
            // 先取消之前的任務（如果存在）
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            
            // 取消前一個執行緒
            if (_thread != null && _thread.IsAlive)
            {
                _thread.Join(100); // 等待 100ms 讓執行緒自然結束
                if (_thread.IsAlive)
                {
                    try { _thread.Interrupt(); }
                    catch
                    {
                        // ignored
                    }
                }
            }

            _cts = new CancellationTokenSource();

            _thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (ThreadInterruptedException)
                {
                    // ignored
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            })
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