using System;
using jIAnSoft.Nami.Fibers;

namespace jIAnSoft.Nami.Clockwork
{
    public enum Unit
    {
        Delay = 1,
        Weeks,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    }

    public enum DelayUnit 
    {
        None = 1,
        Weeks,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    }

    public enum TimingAfterOrBeforeExecuteTask
    {
        BeforeExecuteTask = 1,
        AfterExecuteTask
    }

    public class Job : IDisposable
    {
        private readonly IFiber _fiber;
        private Action _task;
        private int _hour;
        private int _minute;
        private int _second;
        private Unit _unit;
        private readonly int _interval;
        private readonly DayOfWeek _weekday;
        private DateTime _nextRunTime;
        private IDisposable _taskDisposer;
        private DelayUnit _delayUnit;
        private TimingAfterOrBeforeExecuteTask _timingMode;

        private long _runTimes;

        private long _maximumTimes;

        public Job(int interval, IFiber fiber)
        {
            _runTimes = 0;
            _maximumTimes = -1;
            _hour = -1;
            _minute = -1;
            _second = -1;
            _interval = interval;
            _fiber = fiber;
            _timingMode = TimingAfterOrBeforeExecuteTask.BeforeExecuteTask;
            _delayUnit = DelayUnit.None;
        }

        public Job(int interval, Unit unit, IFiber fiber, DelayUnit delayMode) : this(interval, unit, fiber)
        {
            _delayUnit = delayMode;
        }

        public Job(int interval, Unit unit, IFiber fiber) : this(interval, fiber)
        {
            _unit = unit;
        }

        public Job(DayOfWeek weekday, IFiber fiber) : this(1, Unit.Weeks, fiber)
        {
            _weekday = weekday;
        }

        public Job Days()
        {
            if (_delayUnit == DelayUnit.None)
            {
                _unit = Unit.Days;
            }
            else
            {
                _delayUnit = DelayUnit.Days;
            }
            return this;
        }

        public Job Hours()
        {
            if (_delayUnit == DelayUnit.None)
            {
                _unit = Unit.Hours;
            }
            else
            {
                _delayUnit = DelayUnit.Hours;
            }
            return this;
        }

        public Job Minutes()
        {
            if (_delayUnit == DelayUnit.None)
            {
                _unit = Unit.Minutes;
            }
            else
            {
                _delayUnit = DelayUnit.Minutes;
            }
            return this;
        }

        public Job Seconds()
        {
            if (_delayUnit == DelayUnit.None)
            {
                _unit = Unit.Seconds;
            }
            else
            {
                _delayUnit = DelayUnit.Seconds;
            }
            return this;
        }

        public Job Milliseconds()
        {
            if (_delayUnit == DelayUnit.None)
            {
                _unit = Unit.Milliseconds;
            }
            else
            {
                _delayUnit = DelayUnit.Milliseconds;
            }
            return this;
        }

        public Job At(int hour, int minute, int second)
        {
            _hour = Math.Abs(hour);
            _minute = Math.Abs(minute);
            _second = Math.Abs(second);
            if (_unit != Unit.Hours)
            {
                _hour = _hour % 24;
            }
            _minute = minute % 60;
            _second = second % 60;
            return this;
        }

        // Start timing after the task is executed
        public Job AfterExecuteTask()
        {
            _timingMode = TimingAfterOrBeforeExecuteTask.AfterExecuteTask;
            return this;
        }

        //Start timing before the task is executed
        public Job BeforeExecuteTask()
        {
            _timingMode = TimingAfterOrBeforeExecuteTask.BeforeExecuteTask;
            return this;
        }

        public Job Times(long times)
        {
            _maximumTimes = times;
            return this;
        }

        private void SetDelayNextTime()
        {
            var now = DateTime.Now;
            switch (_delayUnit)
            {
                case DelayUnit.Weeks:
                    _nextRunTime = now.AddDays(_interval * 7);
                    break;
                case DelayUnit.Days:
                    _nextRunTime = now.AddDays(_interval);
                    break;
                case DelayUnit.Hours:
                    _nextRunTime = now.AddHours(_interval);
                    break;
                case DelayUnit.Minutes:
                    _nextRunTime = now.AddMinutes(_interval);
                    break;
                case DelayUnit.Seconds:
                    _nextRunTime = now.AddSeconds(_interval);
                    break;
                case DelayUnit.Milliseconds:
                    _nextRunTime = now.AddMilliseconds(_interval);
                    break;
                case DelayUnit.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public IDisposable Do(Action action)
        {
            _task = action;
            var now = DateTime.Now;
            switch (_unit)
            {
                case Unit.Delay:
                    SetDelayNextTime();
                    break;
                case Unit.Weeks:
                    var i = (7 - (now.DayOfWeek - _weekday)) % 7;
                    _nextRunTime = new DateTime(now.Year, now.Month, now.Day , _hour, _minute, _second).AddDays(i);
                    if (_nextRunTime < now)
                    {
                        _nextRunTime = _nextRunTime.AddDays(7);
                    }
                    break;
                case Unit.Days:
                    if (_second < 0 || _minute < 0 || _hour < 0)
                    {
                        _nextRunTime = now.AddDays(1);
                        _second = _nextRunTime.Second;
                        _minute = _nextRunTime.Minute;
                        _hour = _nextRunTime.Hour;
                    }
                    else
                    {
                        _nextRunTime = new DateTime(now.Year, now.Month, now.Day, _hour, _minute, _second);
                        if (_interval > 1)
                        {
                            _nextRunTime = _nextRunTime.AddDays(_interval - 1);
                        }
                        if (_nextRunTime < now)
                        {
                            _nextRunTime = _nextRunTime.AddDays(_interval);
                        }
                    }
                    break;
                case Unit.Hours:
                    if (_minute < 0)
                    {
                        _minute = now.Minute;
                    }
                    if (_second < 0)
                    {
                        _second = now.Second;
                    }
                    _nextRunTime =
                        new DateTime(now.Year, now.Month, now.Day, now.Hour, _minute, _second).AddHours(_interval - 1);
                    if (_nextRunTime < now)
                    {
                        _nextRunTime = _nextRunTime.AddHours(_interval);
                    }
                    break;
                case Unit.Minutes:
                    if (_second < 0)
                    {
                        _second = now.Second;
                    }
                    _nextRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, _second).AddMinutes(_interval - 1);
                    if (_second < now.Second)
                    {
                        _nextRunTime = _nextRunTime.AddMinutes(_interval);
                    }
                    break;
                case Unit.Seconds:
                    _nextRunTime = now.AddSeconds(_interval);
                    break;
                case Unit.Milliseconds:
                    _nextRunTime = now.AddMilliseconds(_interval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var firstInMs = (_nextRunTime.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
            _taskDisposer = _fiber.Schedule(CanDo, firstInMs);
            return this;
        }

        private void CanDo()
        {
            if (DateTime.Now.Ticks >= _nextRunTime.Ticks)
            {
                if (_timingMode == TimingAfterOrBeforeExecuteTask.BeforeExecuteTask)
                {
                    _fiber.Enqueue(_task);
                }
                else
                {
                    var s = DateTime.Now.Ticks;
                    _task();
                    var f = DateTime.Now.Ticks - s;
                    _nextRunTime = _nextRunTime.AddTicks(f);
                }

                _runTimes++;

                if (_maximumTimes > 0 && _runTimes >= _maximumTimes)
                {
                    return;
                }

                switch (_unit)
                {
                    case Unit.Delay:
                        SetDelayNextTime();
                        break;
                    case Unit.Weeks:
                        _nextRunTime = _nextRunTime.AddDays(7);
                        break;
                    case Unit.Days:
                        _nextRunTime = _nextRunTime.AddDays(_interval);
                        break;
                    case Unit.Hours:
                        _nextRunTime = _nextRunTime.AddHours(_interval);
                        break;
                    case Unit.Minutes:
                        _nextRunTime = _nextRunTime.AddMinutes(_interval);
                        break;
                    case Unit.Seconds:
                        _nextRunTime = _nextRunTime.AddSeconds(_interval);
                        break;
                    case Unit.Milliseconds:
                        _nextRunTime = _nextRunTime.AddMilliseconds(_interval);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var adjustTime = (_nextRunTime.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
            _taskDisposer = _fiber.Schedule(CanDo, adjustTime);
        }

        public void Dispose()
        {
            _taskDisposer?.Dispose();
        }
    }
}