using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

public class AsyncPendingFunc : IAsyncDisposable
{
    private readonly Func<Task> _func;
    private bool _cancelled;

    public AsyncPendingFunc(Func<Task> func)
    {
        _func = func;
    }

    public async ValueTask DisposeAsync()
    {
        _cancelled = true;
        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }

    public async Task ExecuteAsync()
    {
        if (!_cancelled)
        {
            await _func();
        }
    }
}