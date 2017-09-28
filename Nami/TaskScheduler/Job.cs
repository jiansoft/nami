using System;
using jIAnSoft.Framework.Nami.Fibers;

namespace jIAnSoft.Framework.Nami.TaskScheduler
{
    public enum Unit
    {
        Delay = 1,

        Weeks,

        Days,

        Hours,

        Minutes,

        Seconds
    }

    public class Job : IDisposable
    {
        private IFiber _fiber;
        private Action _task;
        private int _hour;
        private int _minute;
        private int _second;
        private Unit _unit;
        private readonly int _interval;
        private readonly DayOfWeek _weekday;
        private DateTime _nextRunTime;
        private IDisposable _taskDisposer;

        public Job(int intervel, IFiber fiber)
        {
            _hour = -1;
            _minute = -1;
            _second = -1;
            _interval = intervel;
            _fiber = fiber;
        }

        public Job(int intervel, Unit unit, IFiber fiber) : this(intervel, fiber)
        {
            _unit = unit;
        }

        public Job(DayOfWeek weekday, IFiber fiber) : this(1, Unit.Weeks, fiber)
        {
            _weekday = weekday;
        }

        public Job Days()
        {
            _unit = Unit.Days;
            return this;
        }

        public Job Hours()
        {
            _unit = Unit.Hours;
            return this;
        }

        public Job Minutes()
        {
            _unit = Unit.Minutes;
            return this;
        }

        public Job Seconds()
        {
            _unit = Unit.Seconds;
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

        public IDisposable Do(Action action)
        {
            _task = action;
            var now = DateTime.Now;
            switch (_unit)
            {
                case Unit.Delay:
                    _nextRunTime = now.AddMilliseconds(_interval);
                    break;
                case Unit.Weeks:
                    var i = (7 - (now.DayOfWeek - _weekday)) % 7;
                    _nextRunTime = new DateTime(now.Year, now.Month, now.Day + i, _hour, _minute, _second);
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
                        _nextRunTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, _minute, _second);
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
                    _nextRunTime =
                        new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, _second).AddMinutes(
                            _interval - 1);
                    if (_second < now.Second)
                    {
                        _nextRunTime = _nextRunTime.AddMinutes(_interval);
                    }
                    break;
                case Unit.Seconds:
                    _nextRunTime = now.AddSeconds(_interval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var firstInMs = Convert.ToInt32((_nextRunTime.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond);
            _taskDisposer = _fiber.Schedule(CanDo, firstInMs);
            return this;
        }

        private void CanDo()
        {
            if (DateTime.Now >= _nextRunTime)
            {
                _fiber.Enqueue(_task);
                switch (_unit)
                {
                    case Unit.Delay:
                        return;
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _taskDisposer.Dispose();
            }
            var adjustTime = Convert.ToInt32((_nextRunTime.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond);
            if (adjustTime < 1)
            {
                adjustTime = 1;
            }
            _taskDisposer = _fiber.Schedule(CanDo, adjustTime);
        }

        public void Dispose()
        {
            _fiber = null;
            _taskDisposer?.Dispose();
            _task = null;
        }
    }
}