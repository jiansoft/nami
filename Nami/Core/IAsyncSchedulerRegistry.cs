using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Registry for scheduling asynchronous actions.
/// </summary>
public interface IAsyncSchedulerRegistry
{
    /// <summary>
    /// Enqueue an asynchronous action to the target fiber or scheduler.
    /// </summary>
    /// <param name="func">The asynchronous action to enqueue.</param>
    /// <returns>A task representing the asynchronous enqueue operation.</returns>
    Task EnqueueAsync(Func<Task> func);

    /// <summary>
    /// Remove a scheduled timer or task.
    /// </summary>
    /// <param name="timer">The disposable timer to remove.</param>
    /// <returns>
    /// A value task representing the asynchronous remove operation.
    /// Return true if the timer was successfully removed; otherwise, false.
    /// </returns>
    ValueTask<bool> RemoveAsync(IAsyncDisposable timer);
}