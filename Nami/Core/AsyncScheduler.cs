using jIAnSoft.Nami.Fibers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// 支援排程一次性或週期性非同步任務，
/// 提供 fiber 排程、註銷、與安全釋放等功能的非同步 scheduler。
/// </summary>
public class AsyncScheduler : IAsyncSchedulerRegistry, IAsyncScheduler
{
    private readonly IAsyncFiber _fiber;
    private readonly AsyncSubscriptions _pending = new();
    private volatile bool _disposed;

    /// <summary>
    /// 建立新的 <see cref="AsyncScheduler"/> 實例。
    /// </summary>
    /// <param name="fiber">目標非同步 fiber。</param>
    public AsyncScheduler(IAsyncFiber fiber)
    {
        _fiber = fiber ?? throw new ArgumentNullException(nameof(fiber));
    }

    /// <summary>
    /// 排程一個延遲後（或立即）執行的非同步動作。若 firstInMs &gt; 0，實際會呼叫 <see cref="ScheduleOnIntervalAsync"/>。
    /// </summary>
    /// <param name="func">要執行的非同步任務。</param>
    /// <param name="firstInMs">延遲毫秒數。</param>
    /// <returns>可註銷的 async disposable。</returns>
    public async Task<IAsyncDisposable> ScheduleAsync(Func<Task> func, long firstInMs)
    {
        if (firstInMs > 0)
        {
            return await ScheduleOnIntervalAsync(func, firstInMs, Timeout.Infinite);
        }

        var pending = new AsyncPendingFunc(func);

        await EnqueueAsync(pending.ExecuteAsync);

        return pending;
    }

    /// <summary>
    /// 排程一個週期性非同步任務。
    /// </summary>
    /// <param name="func">欲重複執行的非同步任務。</param>
    /// <param name="firstInMs">首次執行延遲。</param>
    /// <param name="regularInMs">後續重複間隔。</param>
    /// <returns>可註銷的 async disposable。</returns>
    public Task<IAsyncDisposable> ScheduleOnIntervalAsync(Func<Task> func, long firstInMs, long regularInMs)
    {
        var pending = new AsyncTimerFunc(this, func, firstInMs, regularInMs);

        _pending.Add(pending);
        pending.Schedule();

        return Task.FromResult<IAsyncDisposable>(pending);
    }

    /// <summary>
    /// 送入指定的非同步任務到 fiber 排程隊列。
    /// </summary>
    /// <param name="func">非同步任務。</param>
    public Task EnqueueAsync(Func<Task> func)
    {
        return _fiber.EnqueueAsync(func);
    }

    /// <summary>
    /// 移除已註冊的 async timer 或 pending 任務。
    /// </summary>
    /// <param name="timer">要移除的 async disposable。</param>
    /// <returns>移除成功與否。</returns>
    public ValueTask<bool> RemoveAsync(IAsyncDisposable timer)
    {
        return ValueTask.FromResult(_pending.Remove(timer));
    }

    /// <summary>
    /// 釋放 scheduler，本體不會釋放 fiber 及已註冊的 pending 任務。
    /// </summary>
    /// <returns>非同步完成任務。</returns>
    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return ValueTask.CompletedTask;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}