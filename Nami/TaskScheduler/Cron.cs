using jIAnSoft.Framework.Nami.Fibers;
using System;

namespace jIAnSoft.Framework.Nami.TaskScheduler
{
    public class Cron
    {
        private static Cron _instance;
        private readonly IFiber _fiber;

        private static Cron Instance => _instance ?? (_instance = new Cron());

        private Cron()
        {
            _fiber = new PoolFiber();
            _fiber.Start();
        }

        public static Job Every(int interval)
        {
            return new Job(interval, Instance._fiber);
        }

        public static Job Delay(int interval)
        {
            return new Job(interval, Unit.Delay, Instance._fiber);
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
    }
}
