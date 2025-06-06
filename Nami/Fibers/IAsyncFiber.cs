using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Fibers;

/// <summary>
/// Async version of IFiber interface
/// </summary>
public interface IAsyncFiber : IDisposable
{
    Task<bool> EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default);
    void RegisterSubscription(IDisposable toAdd);
    bool DeregisterSubscription(IDisposable toRemove);
    int NumSubscriptions { get; }
    Task<int> GetQueueCountAsync();
    IDisposable Schedule(Func<Task> action, long firstInMs);
    IDisposable ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs);
    Task StartAsync();
    void Start(); // For backward compatibility
    Task StopAsync();
    void Stop(); // For backward compatibility
    Task DisposeAsync();
}