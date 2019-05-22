﻿using System;

namespace jIAnSoft.Nami.Clockwork
{
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
        private Action _task;
        private IDisposable _taskDisposer;
        private DateTime _toTime;
        private DayOfWeek _weekday;

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
            _taskDisposer?.Dispose();
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
            return this;
        }

        public IDisposable Do(Action action)
        {
            _task = action;
            _duration = _interval * (int) _intervalUnit;
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
                        var i = (7 - (now.DayOfWeek - _weekday)) % 7;
                        if (i > 0)
                        {
                            _nextTime = _nextTime.AddDays(i);
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
                    //case IntervalUnit.Second:
                    //case IntervalUnit.Millisecond:
                    default:
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

        private void Run()
        {
            var adjustTime = RemainTime();

            if (adjustTime <= 0)
            {
                if (_toTime.Ticks != 0 && _toTime >= DateTime.Now ||
                    _fromTime.Ticks == 0 || 
                    _toTime.Ticks == 0)
                {
                    if (_calculateNextTimeAfterExecuted)
                    {
                        var s = DateTime.Now.Ticks;
                        _task();
                        var f = DateTime.Now.Ticks - s;
                        _nextTime = _nextTime.AddTicks(f);
                    }
                    else
                    {
                        Nami.Instance.Fiber.Enqueue(_task);
                    }
                }

                _maximumTimes += -1;
                if (_maximumTimes == 0)
                {
                    return;
                }

                _nextTime = _nextTime.AddMilliseconds(_duration);
                if (_toTime.Ticks != 0 && _nextTime >= _toTime)
                {
                    _fromTime = _fromTime.AddDays(1);
                    _toTime = _toTime.AddDays(1);
                    _nextTime = _fromTime;
                }
            }

            Schedule();
        }

        private long RemainTime()
        {
            var adjustTime = (_nextTime.Ticks - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
            return adjustTime;
        }
        
        private void Schedule()
        {
            var diff = RemainTime();
            _taskDisposer = Nami.Instance.Fiber.Schedule(Run, diff);
        }
    }
}