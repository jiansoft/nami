using System;
using jIAnSoft.Framework.Nami.Fibers;
using jIAnSoft.Framework.Nami.TaskScheduler;

namespace DemoNetFramework
{
    internal static class Program
    {
        private static void Main(string[] args)
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
            //Nami.Every(15).Seconds().Do(() => { PrintData("Nami  ", DateTime.Now); });
            Nami.Every(1).Hours().At(0, 02, 0).Do(() => { PrintData("Hours  2", DateTime.Now); });
            //Nami.Delay(1500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.EveryTuesday().At(14, 13, 40).Do(() => { PrintData("Nami.EveryTuesday().At(n, n, n)  ", DateTime.Now); });

           // Nami.Every(1).Minutes().Do(() => { PrintData("Nami.Every(1).Minutes()", DateTime.Now); });
            Nami.Every(2).Minutes().At(0,0,15).Do(() => { PrintData("Nami.Every(2).Minutes().At(0,0,15)", DateTime.Now); });

            Nami.Delay(1000).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(2500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(3500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(4500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Console.ReadKey();
        }

        private static void PrintData(string name, DateTime date)
        {
            Console.WriteLine($"{name} PrintData => {date:yyyy-MM-dd HH:mm:ss.fff}");
        }
    }
}
