using System;
using System.Linq;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Implementation of an async executor for running asynchronous tasks.
/// </summary>
public sealed class AsyncExecutor : IAsyncExecutor
{
    private bool _isDisposed;

    /// <summary>
    /// Executes a single asynchronous action.
    /// </summary>
    public async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing action: {ex}");
        }
    }

    /// <summary>
    /// Executes an array of asynchronous actions in parallel.
    /// </summary>
    public async Task ExecuteAsync(Func<Task>[] actions)
    {
        await Task
            .WhenAll(actions.Select(ExecuteAsync))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the executor asynchronously.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return ValueTask.CompletedTask;

        _isDisposed = true;

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 同步版本的 Dispose（實作 IDisposable 介面）
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
