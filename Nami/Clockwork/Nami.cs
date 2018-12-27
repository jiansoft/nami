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

        public static Job Every(int interval)
        {
            return new Job(interval, Instance._fiber);
        }

        public static Job RightNow()
        {
            return Delay(0);
        }

        public static Job Delay(int interval)
        {
            return new Job(interval, Unit.Delay, Instance._fiber, DelayUnit.Milliseconds);
        }

        public static Job EverySunday()
        {
            return new Job(DayOfWeek.Sunday, Instance._fiber);
        }

        public static Job EveryMonday()
        {
            return new Job(DayOfWeek.Monday, Instance._fiber);
        }

        public static Job EveryTuesday()
        {
            return new Job(DayOfWeek.Tuesday, Instance._fiber);
        }

        public static Job EveryWednesday()
        {
            return new Job(DayOfWeek.Wednesday, Instance._fiber);
        }
        public static Job EveryThursday()
        {
            return new Job(DayOfWeek.Thursday, Instance._fiber);
        }

        public static Job EveryFriday()
        {
            return new Job(DayOfWeek.Friday, Instance._fiber);
        }

        public static Job EverySaturday()
        {
            return new Job(DayOfWeek.Saturday, Instance._fiber);
        }

        public static Job Everyday()
        {
            return Every(1).Days();
        }
    }
}
