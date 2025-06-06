using System;
using System.Threading.Tasks;

namespace jIAnSoft.Nami.Clockwork;

internal enum JobModel
{
    Delay = 1,
    Every
}

public enum IntervalUnit
{
    Millisecond = 1,
    Second = 1000 * Millisecond,
    Minute = 60 * Second,
    Hour = 60 * Minute,
    Day = 24 * Hour,
    Week = 7 * Day
}

public class Job : IDisposable
{
    private bool _calculateNextTimeAfterExecuted;
    private long _duration;
    private DateTime _fromTime;
    private int _hour;
    private int _interval;
    private IntervalUnit _intervalUnit;
    private long _maximumTimes;
    private int _minute;
    private JobModel _model;
    private DateTime _nextTime;
    private int _second;
    private Func<Task> _task;
    private IDisposable _taskDisposer;
    private DateTime _toTime;
    private DayOfWeek _weekday;
    private volatile bool _disposed;

    public Job()
    {
        _maximumTimes = -1;
        _hour = -1;
        _minute = -1;
        _second = -1;
        _model = JobModel.Every;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _taskDisposer?.Dispose();
        _task = null; // 釋放對委派的引用
    }

    internal Job Model(JobModel model)
    {
        _model = model;
        return this;
    }

    public Job Days()
    {
        _intervalUnit = IntervalUnit.Day;
        return this;
    }

    public Job Hours()
    {
        _intervalUnit = IntervalUnit.Hour;
        return this;
    }

    public Job Minutes()
    {
        _intervalUnit = IntervalUnit.Minute;
        return this;
    }

    public Job Seconds()
    {
        _intervalUnit = IntervalUnit.Second;
        return this;
    }

    public Job Milliseconds()
    {
        _intervalUnit = IntervalUnit.Millisecond;
        return this;
    }

    public Job At(int hour, int minute, int second)
    {
        _hour = Math.Abs(hour) % 24;
        _minute = Math.Abs(minute) % 60;
        _second = Math.Abs(second) % 60;
        return this;
    }

    public Job At(TimeOnly time)
    {
        _hour = time.Hour;
        _minute = time.Minute;
        _second = time.Second;
        return this;
    }

    /// <summary>
    /// Start timing after the task is executed
    /// just for delay model、every N second and every N millisecond
    /// If you want some job every N minute、hour or day do once and want to calculate next execution time by after the job executed.
    /// Please use interval unit that Seconds or Milliseconds
    /// </summary>
    /// <returns></returns>
    public Job AfterExecuteTask()
    {
        if (_model == JobModel.Delay ||
            _intervalUnit == IntervalUnit.Second ||
            _intervalUnit == IntervalUnit.Millisecond)
        {
            _calculateNextTimeAfterExecuted = true;
        }

        return this;
    }

    /// <summary>
    /// Start timing before the task is executed
    /// </summary>
    /// <returns></returns>
    public Job BeforeExecuteTask()
    {
        _calculateNextTimeAfterExecuted = false;
        return this;
    }

    internal Job Interval(int interval)
    {
        _interval = interval;
        return this;
    }

    public Job Times(long times)
    {
        _maximumTimes = times;
        return this;
    }

    internal Job Week(DayOfWeek weekday)
    {
        _intervalUnit = IntervalUnit.Week;
        _weekday = weekday;
        return this;
    }

    public Job Between(BetweenTime f, BetweenTime t)
    {
        if (_model == JobModel.Delay || f.IsZero() || t.IsZero())
        {
            return this;
        }

        var now = DateTime.Now;
        _fromTime = new DateTime(now.Year, now.Month, now.Day, f.Hour, f.Minute, f.Second, f.Millisecond);
        _toTime = new DateTime(now.Year, now.Month, now.Day, t.Hour, t.Minute, t.Second, t.Millisecond);

        // 處理跨午夜情況：如果結束時間早於或等於開始時間，表示跨越午夜
        if (_toTime <= _fromTime)
        {
            _toTime = _toTime.AddDays(1);
        }

        return this;
    }

    public Job Between(TimeOnly fromTime, TimeOnly toTime)
    {
        if (_model == JobModel.Delay)
        {
            return this;
        }

        var now = DateTime.Now;
        _fromTime = new DateTime(now.Year, now.Month, now.Day, fromTime.Hour, fromTime.Minute, fromTime.Second,
            fromTime.Millisecond);
        _toTime = new DateTime(now.Year, now.Month, now.Day, toTime.Hour, toTime.Minute, toTime.Second,
            toTime.Millisecond);

        // 處理跨日情況：如果 toTime 小於 fromTime，表示跨越午夜
        if (toTime < fromTime)
        {
            _toTime = _toTime.AddDays(1);
        }

        return this;
    }

    // 支援同步 Action 的 Do 方法
    public IDisposable Do(Action action)
    {
        // 將同步 Action 包裝成 Func<Task>
        _task = () =>
        {
            action();
            return Task.CompletedTask;
        };

        return DoInternal();
    }

    // 支援非同步 Func<Task> 的 Do 方法
    public IDisposable Do(Func<Task> action)
    {
        _task = action;
        return DoInternal();
    }

    private Job DoInternal()
    {
        _duration = _interval * (int)_intervalUnit;
        var now = DateTime.Now;

        if (_model == JobModel.Delay)
        {
            _nextTime = now;
        }
        else
        {
            if (_hour < 0)
            {
                _hour = now.Hour;
            }

            if (_minute < 0)
            {
                _minute = now.Minute;
            }

            if (_second < 0)
            {
                _second = now.Second;
            }

            switch (_intervalUnit)
            {
                case IntervalUnit.Week:
                    _nextTime = new DateTime(now.Year, now.Month, now.Day, _hour, _minute, _second);
                    var daysToAdd = (7 - (now.DayOfWeek - _weekday)) % 7;
                    if (daysToAdd > 0)
                    {
                        _nextTime = _nextTime.AddDays(daysToAdd);
                    }

                    break;
                case IntervalUnit.Day:
                    _nextTime = new DateTime(now.Year, now.Month, now.Day, _hour, _minute, _second);
                    break;
                case IntervalUnit.Hour:
                    _nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, _minute, _second);
                    break;
                case IntervalUnit.Minute:
                    _nextTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, _second);
                    break;

                default: // Second, Millisecond
                    _nextTime = now;
                    if (_fromTime.Ticks != 0 && _nextTime < _fromTime)
                    {
                        _nextTime = _fromTime;
                        if (_toTime.Ticks != 0 && _nextTime > _toTime)
                        {
                            _fromTime = _fromTime.AddMilliseconds(_duration);
                            _toTime = _toTime.AddMilliseconds(_duration);
                            _nextTime = _fromTime;
                        }
                    }

                    break;
            }
        }

        if (_nextTime <= now)
        {
            _nextTime = _nextTime.AddMilliseconds(_duration);
        }

        Schedule();
        return this;
    }

    private async Task RunAsync()
    {
        if (_disposed) return;

        try
        {
            var adjustTime = RemainTime();

            if (adjustTime <= 0)
            {
                var shouldExecute = _toTime.Ticks != 0 && _toTime >= DateTime.Now ||
                                    _fromTime.Ticks == 0 ||
                                    _toTime.Ticks == 0;

                if (shouldExecute)
                {
                    if (_calculateNextTimeAfterExecuted)
                    {
                        var startTime = DateTime.Now;
                        await _task();
                        var executionTime = DateTime.Now - startTime;
                        _nextTime = _nextTime.Add(executionTime);
                    }
                    else
                    {
                        // 使用 TryEnqueue 避免等待，如果失敗就直接執行
                        var enqueued = await Nami.Instance.Fiber.EnqueueAsync(_task);
                        if (!enqueued)
                        {
                            // 如果入隊失敗，直接執行
                            await _task();
                        }
                    }
                }

                _maximumTimes--;

                if (_maximumTimes == 0)
                {
                    return; // 達到最大執行次數，停止排程
                }

                // 計算下次執行時間
                _nextTime = _nextTime.AddMilliseconds(_duration);
                if (_toTime.Ticks != 0 && _nextTime >= _toTime)
                {
                    _fromTime = _fromTime.AddDays(1);
                    _toTime = _toTime.AddDays(1);
                    _nextTime = _fromTime;
                }
            }

            // 繼續排程下次執行
            Schedule();
        }
        catch (Exception ex)
        {
            // 記錄錯誤但不中斷排程
            Console.WriteLine($"Job execution error: {ex}");

            // 發生錯誤時仍然繼續排程
            Schedule();
        }
    }

    private long RemainTime()
    {
        var diff = (_nextTime - DateTime.Now).TotalMilliseconds;
        return Math.Max(0, (long)Math.Ceiling(diff));
    }

    private void Schedule()
    {
        if (_disposed) return;

        var delay = RemainTime();

        _taskDisposer = Nami.Instance.Fiber.Schedule(RunAsync, delay);
    }
}