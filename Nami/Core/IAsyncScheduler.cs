using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Async version of IScheduler interface
/// </summary>
public interface IAsyncScheduler : IAsyncDisposable
{
    Task<IAsyncDisposable> ScheduleAsync(Func<Task> func, long firstInMs);
    Task<IAsyncDisposable> ScheduleOnIntervalAsync(Func<Task> func, long firstInMs, long regularInMs);
}