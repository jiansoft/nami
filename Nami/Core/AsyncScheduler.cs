using jIAnSoft.Nami.Fibers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Implementation of async scheduler
/// </summary>
public class AsyncScheduler : IAsyncScheduler
{
    private readonly IAsyncFiber _fiber;
    private readonly Timer _timer;
    private volatile bool _disposed;

    public AsyncScheduler(IAsyncFiber fiber)
    {
        _fiber = fiber ?? throw new ArgumentNullException(nameof(fiber));
        _timer = new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task<IDisposable> Schedule(Func<Task> action, long firstInMs)
    {
        return ScheduleOnInterval(action,firstInMs, Timeout.Infinite);
    }


    public Task<IDisposable> ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs)
    {
        if (_disposed)
            return Task.FromResult<IDisposable>(new EmptyDisposable());
        
        var timer = new Timer(async void (_) =>
        {
            try
            {
                await _fiber.EnqueueAsync(action);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing scheduled action: {ex}");
            }
        }, null, firstInMs, Timeout.Infinite);

        return Task.FromResult<IDisposable>(Task.FromResult(timer));
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        _disposed = true;
        _timer?.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}