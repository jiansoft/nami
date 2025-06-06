using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Async version of IScheduler interface
/// </summary>
public interface IAsyncScheduler : IAsyncDisposable
{
    Task<IDisposable> Schedule(Func<Task> action, long firstInMs);
    Task<IDisposable> ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs);
}