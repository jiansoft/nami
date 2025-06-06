using jIAnSoft.Nami.Core;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Fibers;

/// <summary>
/// Async version of IFiber interface
/// </summary>
public interface IAsyncFiber : IAsyncExecutionContext, IAsyncScheduler
{
    Task StartAsync();
    Task StopAsync();
    
    /*Task<bool> EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default);
    void RegisterSubscription(IDisposable toAdd);
    bool DeregisterSubscription(IDisposable toRemove);
    int NumSubscriptions { get; }
    Task<int> GetQueueCountAsync();*/
}