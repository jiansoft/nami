using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

/// <summary>
/// Async version of IExecutor interface
/// </summary>
public interface IAsyncExecutor : IAsyncDisposable 
{
    Task ExecuteAsync(Func<Task> action);
    Task ExecuteAsync(Func<Task>[] actions);
}