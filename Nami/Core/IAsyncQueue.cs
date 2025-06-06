using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

public interface IAsyncQueue : IAsyncDisposable
{
    Task<bool> EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default);
    Task<Func<Task>[]> DequeueAllAsync(CancellationToken cancellationToken = default);
    Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken = default);
    bool TryDequeue(out Func<Task> action);
    Task<int> CountAsync();
    Task RunAsync();
    Task StopAsync();
    IAsyncEnumerable<Func<Task>> ReadAllAsync(CancellationToken cancellationToken = default);
}