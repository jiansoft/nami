using System;
using jIAnSoft.Nami.Fibers;

namespace jIAnSoft.Nami.Clockwork
{
    public class Nami
    {
        private static Nami _instance;
        internal readonly IFiber Fiber;

        internal static Nami Instance => _instance ?? (_instance = new Nami());

        private Nami()
        {
            Fiber = new PoolFiber();
            Fiber.Start();
        }

        public static Job RightNow()
        {
            return Delay(0);
        }

        public static Job Delay(int interval)
        {
            return new Job().Model(JobModel.Delay).Interval(interval).Milliseconds().Times(1);
        }

        public static Job Every(int interval)
        {
            return new Job().Interval(interval);
        }

        public static Job EverySunday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Sunday);
        }

        public static Job EveryMonday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Monday);
        }

        public static Job EveryTuesday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Tuesday);
        }

        public static Job EveryWednesday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Wednesday);
        }

        public static Job EveryThursday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Thursday);
        }

        public static Job EveryFriday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Friday);
        }

        public static Job EverySaturday()
        {
            return new Job().Interval(1).Week(DayOfWeek.Saturday);
        }

        public static Job Everyday()
        {
            return Every(1).Days();
        }
    }
}
