using System;
using jIAnSoft.Nami.Fibers;

namespace jIAnSoft.Nami.Clockwork
{
    public class Nami
    {
        private static Nami _instance;
        private readonly IFiber _fiber;

        private static Nami Instance => _instance ?? (_instance = new Nami());

        private Nami()
        {
            _fiber = new PoolFiber();
            _fiber.Start();
        }

        public static Job RightNow()
        {
            return Delay(0);
        }

        public static Job Delay(int interval)
        {
            return new Job(Instance._fiber).Interval(interval).Times(1).Model(JobModel.Delay).Milliseconds();
        }

        public static Job Every(int interval)
        {
            return new Job(Instance._fiber).Interval(interval);
        }

        public static Job EverySunday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Sunday);
        }

        public static Job EveryMonday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Monday);
        }

        public static Job EveryTuesday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Tuesday);
        }

        public static Job EveryWednesday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Wednesday);
        }

        public static Job EveryThursday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Thursday);
        }

        public static Job EveryFriday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Friday);
        }

        public static Job EverySaturday()
        {
            return new Job(Instance._fiber).Interval(1).Week(DayOfWeek.Saturday);
        }

        public static Job Everyday()
        {
            return Every(1).Days();
        }
    }
}
