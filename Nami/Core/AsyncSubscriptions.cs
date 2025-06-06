using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

public class AsyncSubscriptions : IAsyncDisposable
{
    private readonly object _lock = new();

    private List<IAsyncDisposable> _asyncDisposables = new();
    
    /// <summary>
    /// Add asynchronous disposable
    /// </summary>
    public void Add(IAsyncDisposable toAdd)
    {
        lock (_lock)
        {
            _asyncDisposables.Add(toAdd);
        }
    }
    
    /// <summary>
    /// Remove asynchronous disposable
    /// </summary>
    public bool Remove(IAsyncDisposable toRemove)
    {
        lock (_lock)
        {
            return _asyncDisposables.Remove(toRemove);
        }
    }

    /// <summary>
    /// Disposes all disposables registered in list.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        var disposables = Interlocked.Exchange(ref _asyncDisposables, new List<IAsyncDisposable>());

        foreach (var disposable in disposables)
        {
            await disposable.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Number of registered disposables.
    /// </summary>
    public int Count
    {
        get
        {
            int count;
            lock (_lock)
            {
                count = _asyncDisposables.Count;
            }

            return count;
        }
    }
}