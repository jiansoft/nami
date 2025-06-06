using jIAnSoft.Nami.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Fibers;

/// <summary>
/// Async Fiber that uses a thread pool for execution with async queue.
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

    /// <summary>
    /// Construct new instance.
    /// </summary>
    public MultiTaskFiber()
    {
        _queue = new AsyncDefaultQueue();
        _scheduler = new AsyncScheduler(this);
        _executor = new AsyncExecutor();
    }
    
    /// <inheritdoc />
    /// <summary>
    /// Enqueue a single action asynchronously.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    public async Task<bool> EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (_state != ExecutionState.Running || _disposed == 1)
        {
            return false;
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cancellationTokenSource.Token);

        var success = await _queue.EnqueueAsync(action, linkedCts.Token);
            
        if (success)
        {
            // 確保處理任務正在運行
            EnsureProcessingActive();
        }

        return success;
    }
    
    /// <inheritdoc />
    /// <summary>
    ///  Register subscription to be unsubscribed from when the fiber is disposed.
    /// </summary>
    /// <param name="toAdd"></param>
    public void RegisterSubscription(IDisposable toAdd)
    {
        _subscriptions.Add(toAdd);
    }

    /// <inheritdoc />
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

                // 為每個 action 創建獨立的 Task 並行執行
                var tasks = actions.Select(action => 
                    Task.Run(async () =>
                    {
                        try
                        {
                            await action();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error executing action: {ex}");
                        }
                    }, _cancellationTokenSource.Token)
                ).ToArray();

               // await Task.WhenAll(tasks);
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

    /// <inheritdoc />
    /// <summary>
    /// Schedule an action to be executed after a delay.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="firstInMs"></param>
    /// <returns></returns>
    public IDisposable Schedule(Func<Task>action, long firstInMs)
    {
        return _scheduler.Schedule(action, firstInMs);
    }

    /// <inheritdoc />
    /// <summary>
    /// Schedule an action to be executed repeatedly at intervals.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="firstInMs"></param>
    /// <param name="regularInMs"></param>
    /// <returns></returns>
    public IDisposable ScheduleOnInterval(Func<Task> action, long firstInMs, long regularInMs)
    {
        return _scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
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
    /// Start consuming actions synchronously (for backward compatibility).
    /// </summary>
    public void Start()
    {
        StartAsync().GetAwaiter().GetResult();
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

    /// <summary>
    /// Stop consuming actions synchronously (for backward compatibility).
    /// </summary>
    public void Stop()
    {
        StopAsync().GetAwaiter().GetResult();
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
    public async Task DisposeAsync()
    {
        await DisposeAsync(true);
    }

    /// <summary>
    /// Stops the fiber and releases resources synchronously (for backward compatibility).
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}

/// <summary>
/// Empty disposable for cases where scheduling fails
/// </summary>
public class EmptyDisposable : IDisposable
{
    public void Dispose()
    {
        // 什麼都不做
    }
}

/*// 使用範例
public class AsyncFiberUsageExample
{
public async Task ExampleUsage()
{
    var fiber = new AsyncPoolFiber();

    try
    {
        // 啟動 fiber
        await fiber.StartAsync();

        // 非同步加入動作
        await fiber.EnqueueAsync(async () =>
        {
            await Task.Delay(10);

            Console.WriteLine("Hello from async fiber!");
        });

        // 同步嘗試加入動作（不等待）
        fiber.TryEnqueue(() => Task.Run(() => Console.WriteLine("Quick action")));


        // 排程延遲動作（使用 Task.Run 包裝）
        var scheduledAction = fiber.Schedule(() =>
                Task.Run(() => Console.WriteLine("Scheduled action")),
            1000
        );
        var scheduledAction1 = fiber.Schedule(async () =>
            {
                await Task.Delay(10);
                Console.WriteLine("Scheduled action");
            },
            1000
        );
        // 排程重複動作（使用 Task.Run 包裝）
        var intervalAction = fiber.ScheduleOnInterval(
            () => Task.Run(() => Console.WriteLine("Repeated action")),
            2000,
            1000
        );


        // 等待一段時間讓動作執行
        await Task.Delay(5000);

        // 取消排程動作
        scheduledAction.Dispose();
        intervalAction.Dispose();
    }
    finally
    {
        // 停止並清理
        await fiber.DisposeAsync();
    }
}
}*/