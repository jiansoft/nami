using System;
using jIAnSoft.Nami.Fibers;

namespace jIAnSoft.Nami.Clockwork
{
    internal enum JobModel
    {
        Delay = 1,
        Every
    }

    public enum IntervalUnit
    {
        Weeks,
        Days,
        Hours,
        Minutes,
        Seconds,
        Milliseconds
    }

    internal enum TimingAfterOrBeforeExecuteTask
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
        private long _runTimes;
        private long _maximumTimes;
        private IntervalUnit _intervalUnit;
        private readonly int _interval;
        private readonly DayOfWeek _weekday;
        private DateTime _nextRunTime;
        private IDisposable _taskDisposer;
        private TimingAfterOrBeforeExecuteTask _timingMode;
        private JobModel _model;
       
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
            _model = JobModel.Every;
        }
        
        public Job(int interval, IntervalUnit intervalUnit, IFiber fiber) : this(interval, fiber)
        {
            _intervalUnit = intervalUnit;
        }

        public Job(DayOfWeek weekday, IFiber fiber) : this(1, IntervalUnit.Weeks, fiber)
        {
            _weekday = weekday;
        }

        internal Job Model(JobModel model)
        {
            _model = model;
            return this;
        }

        public Job Days()
        {
            _intervalUnit = IntervalUnit.Days;
            return this;
        }

        public Job Hours()
        {
            _intervalUnit = IntervalUnit.Hours;
            return this;
        }

        public Job Minutes()
        {
            _intervalUnit = IntervalUnit.Minutes;
            return this;
        }

        public Job Seconds()
        {
            _intervalUnit = IntervalUnit.Seconds;
            return this;
        }

        public Job Milliseconds()
        {
            _intervalUnit = IntervalUnit.Milliseconds;
            return this;
        }

        public Job At(int hour, int minute, int second)
        {
            _hour = Math.Abs(hour);
            _minute = Math.Abs(minute);
            _second = Math.Abs(second);
            if (_intervalUnit != IntervalUnit.Hours)
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
            switch (_intervalUnit)
            {
                case IntervalUnit.Weeks:
                    _nextRunTime = now.AddDays(_interval * 7);
                    break;
                case IntervalUnit.Days:
                    _nextRunTime = now.AddDays(_interval);
                    break;
                case IntervalUnit.Hours:
                    _nextRunTime = now.AddHours(_interval);
                    break;
                case IntervalUnit.Minutes:
                    _nextRunTime = now.AddMinutes(_interval);
                    break;
                case IntervalUnit.Seconds:
                    _nextRunTime = now.AddSeconds(_interval);
                    break;
                case IntervalUnit.Milliseconds:
                    _nextRunTime = now.AddMilliseconds(_interval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IDisposable Do(Action action)
        {
            _task = action;
            if (_model == JobModel.Delay)
            {
                SetDelayNextTime();
            }
            else
            {
                var now = DateTime.Now;
                switch (_intervalUnit)
                {
                    case IntervalUnit.Weeks:
                        var i = (7 - (now.DayOfWeek - _weekday)) % 7;
                        _nextRunTime = new DateTime(now.Year, now.Month, now.Day, _hour, _minute, _second).AddDays(i);
                        if (_nextRunTime < now)
                        {
                            _nextRunTime = _nextRunTime.AddDays(7);
                        }

                        break;
                    case IntervalUnit.Days:
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
                    case IntervalUnit.Hours:
                        if (_minute < 0)
                        {
                            _minute = now.Minute;
                        }

                        if (_second < 0)
                        {
                            _second = now.Second;
                        }

                        _nextRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, _minute, _second)
                            .AddHours(_interval - 1);

                        if (_nextRunTime < now)
                        {
                            _nextRunTime = _nextRunTime.AddHours(_interval);
                        }

                        break;
                    case IntervalUnit.Minutes:
                        if (_second < 0)
                        {
                            _second = now.Second;
                        }

                        _nextRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, _second)
                            .AddMinutes(_interval - 1);

                        if (_second < now.Second)
                        {
                            _nextRunTime = _nextRunTime.AddMinutes(_interval);
                        }

                        break;
                    case IntervalUnit.Seconds:
                        _nextRunTime = now.AddSeconds(_interval);
                        break;
                    case IntervalUnit.Milliseconds:
                        _nextRunTime = now.AddMilliseconds(_interval);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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

                if (JobModel.Delay == _model)
                {
                    SetDelayNextTime();
                }
                else
                {
                    switch (_intervalUnit)
                    {
                        case IntervalUnit.Weeks:
                            _nextRunTime = _nextRunTime.AddDays(7);
                            break;
                        case IntervalUnit.Days:
                            _nextRunTime = _nextRunTime.AddDays(_interval);
                            break;
                        case IntervalUnit.Hours:
                            _nextRunTime = _nextRunTime.AddHours(_interval);
                            break;
                        case IntervalUnit.Minutes:
                            _nextRunTime = _nextRunTime.AddMinutes(_interval);
                            break;
                        case IntervalUnit.Seconds:
                            _nextRunTime = _nextRunTime.AddSeconds(_interval);
                            break;
                        case IntervalUnit.Milliseconds:
                            _nextRunTime = _nextRunTime.AddMilliseconds(_interval);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
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