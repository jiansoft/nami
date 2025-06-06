using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// 表示一個可以排程與自動重複執行的非同步定時器任務，
/// 並可安全地註銷與釋放資源（支援 IAsyncDisposable）。
/// </summary>
public class AsyncTimerFunc : IAsyncDisposable
{
    private readonly IAsyncSchedulerRegistry _scheduler;
    private Func<Task> _action;
    private readonly long _firstIntervalInMs;
    private readonly long _intervalInMs;
    private Timer _timer;
    private bool _cancelled;

    /// <summary>
    /// 建立新的 <see cref="AsyncTimerFunc"/> 實例。
    /// </summary>
    /// <param name="scheduler">用於排程和註銷本定時器的非同步 scheduler registry。</param>
    /// <param name="func">每次定時器觸發時要執行的非同步動作。</param>
    /// <param name="firstIntervalInMs">第一次觸發的延遲（毫秒）。</param>
    /// <param name="intervalInMs">後續每次觸發的間隔（毫秒）。若為 <see cref="Timeout.Infinite"/> 則只執行一次。</param>
    /// <exception cref="ArgumentNullException">若 scheduler 或 action 為 null。</exception>
    public AsyncTimerFunc(
        IAsyncSchedulerRegistry scheduler,
        Func<Task> func,
        long firstIntervalInMs,
        long intervalInMs)
    {
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _action = func ?? throw new ArgumentNullException(nameof(func));
        _firstIntervalInMs = firstIntervalInMs;
        _intervalInMs = intervalInMs;
    }

    /// <summary>
    /// 啟動定時器，依指定延遲與間隔排程動作。
    /// </summary>
    public void Schedule()
    {
        _timer = new Timer(async void (_) =>
        {
            try
            {
                await ExecuteOnTimerThread();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing scheduled action: {ex}");
            }
        }, null, _firstIntervalInMs, _intervalInMs);
    }

    /// <summary>
    /// 由定時器執行緒觸發。若為單次或已取消則自動移除與釋放資源，否則派送到 fiber 執行非同步動作。
    /// </summary>
    private async Task ExecuteOnTimerThread()
    {
        if (_intervalInMs == Timeout.Infinite || _cancelled)
        {
            var timer = Interlocked.Exchange(ref _timer, null);

            if (timer != null)
            {
                await timer.DisposeAsync();
            }

            await _scheduler.RemoveAsync(this);
        }

        // 派送到目標 fiber 執行
        _ = _scheduler.EnqueueAsync(ExecuteOnFiberAsync);
    }

    /// <summary>
    /// 在目標 fiber 上執行非同步動作。若已取消則忽略。
    /// </summary>
    private async Task ExecuteOnFiberAsync()
    {
        if (_cancelled)
        {
            return;
        }

        if (_action != null)
        {
            await _action();
        }
    }

    /// <summary>
    /// 非同步釋放本定時器與相關資源。結束所有排程並移除自己。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _cancelled = true;
        _action = null;
        await _scheduler.RemoveAsync(this);
        var timer = Interlocked.Exchange(ref _timer, null);
        if (timer != null)
        {
            await timer.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}