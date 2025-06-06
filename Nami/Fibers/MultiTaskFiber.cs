using jIAnSoft.Nami.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Fibers;
/// <summary>
/// 
/// </summary>
public class MultiTaskFiber : IAsyncFiber
{
    private readonly Subscriptions _subscriptions = new();
    private readonly IAsyncQueue _queue;
    private readonly IAsyncScheduler _scheduler;
    private readonly IAsyncExecutor _executor;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private volatile ExecutionState _state = ExecutionState.Created;
    private volatile bool _processingActive;
    private int _disposed; // 0=false, 1=true
    private Task _processingTask;

   
    public MultiTaskFiber()
    {
        _queue = new AsyncDefaultQueue();
        _scheduler = new AsyncScheduler(this);
        _executor = new AsyncExecutor();
    }
    
    public async Task EnqueueAsync(Func<Task> action)
    {
        if (_state != ExecutionState.Running || _disposed == 1)
        {
            return ;
        }
        
        var success = await _queue.EnqueueAsync(action);
            
        if (success)
        {
            // 確保處理任務正在運行
            EnsureProcessingActive();
        }
    }
    
    /// <summary>
    ///  Register subscription to be unsubscribed from when the fiber is disposed.
    /// </summary>
    /// <param name="toAdd"></param>
    public void RegisterSubscription(IDisposable toAdd)
    {
        _subscriptions.Add(toAdd);
    }
    
    /// <summary>
    ///  Deregister a subscription.
    /// </summary>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public bool DeregisterSubscription(IDisposable toRemove)
    {
        return _subscriptions.Remove(toRemove);
    }

    ///<summary>
    /// Number of subscriptions.
    ///</summary>
    public int NumSubscriptions => _subscriptions.Count;

    /// <summary>
    /// Get approximate queue count asynchronously.
    /// </summary>
    public async Task<int> GetQueueCountAsync()
    {
        return await _queue.CountAsync();
    }

    private void EnsureProcessingActive()
    {
        if (_processingActive || _state != ExecutionState.Running)
        {
            return;
        }

        lock (this)
        {
            if (_processingActive || _state != ExecutionState.Running)
            {
                return;
            }

            _processingActive = true;
            _processingTask = Task.Run(ProcessQueueAsync, _cancellationTokenSource.Token);
        }
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            while (_state == ExecutionState.Running && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var actions = await _queue.DequeueAllAsync(_cancellationTokenSource.Token);
                    
                if (actions.Length == 0)
                {
                    break;
                }
                
                foreach (var action in actions)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await action();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing action: {ex}");
                        }
                    }, _cancellationTokenSource.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常的取消操作，靜默處理
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ProcessQueueAsync: {ex}");
        }
        finally
        {
            _processingActive = false;
        }
    }
    
    public Task<IAsyncDisposable> ScheduleAsync(Func<Task> func, long firstInMs)
    {
        return _scheduler.ScheduleAsync(func, firstInMs);
    }

    public Task<IAsyncDisposable> ScheduleOnIntervalAsync(Func<Task> func, long firstInMs, long regularInMs)
    {
        return _scheduler.ScheduleOnIntervalAsync(func, firstInMs, regularInMs);
    }

    /// <inheritdoc />
    /// <summary>
    /// Start consuming actions asynchronously.
    /// </summary>
    public async Task StartAsync()
    {
        if (_disposed == 1 || _state == ExecutionState.Running)
        {
            return;
        }

        _state = ExecutionState.Running;
        await _queue.RunAsync();
            
        // 觸發處理開始
        await EnqueueAsync(() => Task.CompletedTask);
    }
    
    /// <summary>
    /// Stop consuming actions asynchronously.
    /// </summary>
    public async Task StopAsync()
    {
        if (_disposed == 1)
        {
            return;
        }

        _state = ExecutionState.Stopped;
        _cancellationTokenSource.Cancel();

        // 等待處理任務完成
        if (_processingTask != null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // 預期的取消異常
            }
        }
        
        _subscriptions.Dispose();
        await _queue.StopAsync();
    }

    private async Task DisposeAsync(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (!disposing)
        {
            return;
        }

        await StopAsync();
        await _queue.DisposeAsync();
        await _executor.DisposeAsync();
        _cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Stops the fiber and releases resources asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }
}