using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Context of asynchronous execution.
/// </summary>
public interface IAsyncExecutionContext
{
    /// <summary>
    /// Enqueue a single asynchronous action for execution.
    /// </summary>
    /// <param name="action">The asynchronous action to enqueue.</param>
    /// <returns>A Task representing the enqueue operation.</returns>
    Task EnqueueAsync(Func<Task> action);
}