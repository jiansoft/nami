using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Core;

public class AsyncDefaultQueue : IAsyncQueue
{
    private readonly ChannelWriter<Func<Task>> _writer;
    private readonly ChannelReader<Func<Task>> _reader;
    private volatile bool _running = true;
    private volatile bool _disposed;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public AsyncDefaultQueue(int capacity = -1)
    {
        var options = new BoundedChannelOptions(capacity == -1 ? int.MaxValue : capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };

        var channel = Channel.CreateBounded<Func<Task>>(options);
        _writer = channel.Writer;
        _reader = channel.Reader;
    }

    public async Task<bool> EnqueueAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (!_running || _disposed)
        {
            return false;
        }

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            await _writer.WriteAsync(action, linkedCts.Token);
            return true;
        }
        catch (InvalidOperationException)
        {
            // Channel 已關閉
            return false;
        }
        catch (OperationCanceledException)
        {
            // 操作被取消
            return false;
        }
    }

    public async Task<Func<Task>[]> DequeueAllAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || !_running)
        {
            return Array.Empty<Func<Task>>();
        }

        var actions = new List<Func<Task>>();

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            // 等待至少一個項目
            if (await _reader.WaitToReadAsync(linkedCts.Token))
            {
                // 讀取所有可用的項目
                while (_reader.TryRead(out var action))
                {
                    actions.Add(action);
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Channel 已關閉
            return Array.Empty<Func<Task>>();
        }
        catch (OperationCanceledException)
        {
            // 操作被取消
            return Array.Empty<Func<Task>>();
        }

        return actions.ToArray();
    }

    public async Task<Func<Task>> DequeueAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return null;
        }

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _cancellationTokenSource.Token);

            return await _reader.ReadAsync(linkedCts.Token);
        }
        catch (InvalidOperationException)
        {
            // Channel 已關閉
            return null;
        }
        catch (OperationCanceledException)
        {
            // 操作被取消
            return null;
        }
    }

    public bool TryDequeue(out Func<Task> action)
    {
        action = null;

        return !_disposed && _reader.TryRead(out action);
    }

    public async Task<int> CountAsync()
    {
        // Channel 沒有直接的 Count 屬性，這是一個近似值
        // 在高並發情況下可能不完全準確
        var count = 0;
        await foreach (var _ in _reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            count++;
            // 為了不阻塞太久，我們設置一個合理的上限
            if (count > 10000) break;
        }

        return count;
    }

    public Task RunAsync()
    {
        _running = true;
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _running = false;
        _writer.TryComplete();

        // 等待所有等待中的操作完成
        await Task.Delay(100);
    }
    
    // 提供一個非同步枚舉器，可以持續監聽隊列
    public async IAsyncEnumerable<Func<Task>> ReadAllAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cancellationTokenSource.Token);

        await foreach (var action in _reader.ReadAllAsync(linkedCts.Token))
        {
            yield return action;
        }
    }

    /// <summary>
    /// Disposes the queue asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _running = false;

        try
        {
            // 完成 writer，不再接受新的項目
            _writer.TryComplete();

            // 取消所有等待中的操作
            _cancellationTokenSource.Cancel();

            // 等待一段時間讓正在進行的操作完成
            await Task.Delay(100);

            // 清理剩餘的項目（如果需要的話）
            while (_reader.TryRead(out var _))
            {
                // 清空隊列中剩餘的項目
            }
        }
        catch (Exception ex)
        {
            // 記錄但不重新拋出異常
            Console.WriteLine($"Error during AsyncDefaultQueue disposal: {ex}");
        }
        finally
        {
            // 釋放 CancellationTokenSource
            _cancellationTokenSource.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

// 對應的介面