using System;
using jIAnSoft.Framework.Nami.Fibers;
using jIAnSoft.Framework.Nami.TaskScheduler;

namespace Demo
{
    static class Program
    {
        static void Main(string[] args)
        {
            IFiber pool = new PoolFiber();
            IFiber thread = new ThreadFiber();
            pool.Start();
            thread.Start();
            pool.ScheduleOnInterval(() => { PrintData("pool  ", DateTime.Now); }, 0, 150000);
            var td = thread.ScheduleOnInterval(() => { PrintData("thread", DateTime.Now); }, 0, 150000);
            pool.Schedule(() =>
            {
                Console.WriteLine($"td Dispose");
                td.Dispose();
            }, 20000);
            //Cron.Every(15).Seconds().Do(() => { PrintData("Cron  ", DateTime.Now); });
            Cron.Every(1).Hours().At(0, 02, 0).Do(() => { PrintData("Hours  2", DateTime.Now); });
            //Cron.Delay(1500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Cron.EveryTuesday().At(14, 13, 40).Do(() => { PrintData("Cron.EveryTuesday().At(n, n, n)  ", DateTime.Now); });

           // Cron.Every(1).Minutes().Do(() => { PrintData("Cron.Every(1).Minutes()", DateTime.Now); });
            Cron.Every(2).Minutes().At(0,0,15).Do(() => { PrintData("Cron.Every(2).Minutes().At(0,0,15)", DateTime.Now); });

            Cron.Delay(1000).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Cron.Delay(2500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Cron.Delay(3500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Cron.Delay(4500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Console.ReadKey();
        }

        private static void PrintData(string name, DateTime date)
        {
            Console.WriteLine($"{name} PrintData => {date:yyyy-MM-dd HH:mm:ss.fff}");
        }
    }
}
