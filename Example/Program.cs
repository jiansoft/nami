using jIAnSoft.Nami.Clockwork;
using jIAnSoft.Nami.Fibers;
using System;

namespace Example
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
            var td = thread.ScheduleOnInterval(() => { PrintData("thread", DateTime.Now); }, 0, 10000);
            thread.ScheduleOnInterval(() => { PrintData("thread ten second", DateTime.Now); }, 0, 10000);
            for (int i = 0; i < 100; i++)
            {
                thread.Enqueue(() => { PrintData("thread  ", DateTime.Now); });
            }

            pool.Schedule(() =>
            {
                Console.WriteLine($"td Dispose");
                td.Dispose();
                for (int i = 0; i < 10; i++)
                {
                    thread.Enqueue(() => { PrintData("Schedule start thread  ", DateTime.Now); });
                }
            }, 20000);

            Nami.Every(1).Seconds().Do(() =>
            {
                thread?.Enqueue(() => { PrintData(" Nami.Every(1).Seconds().Do  ", DateTime.Now); });

            });

            Nami.Every(15).Seconds().Do(() => { PrintData("Nami  ", DateTime.Now); });
            Nami.Every(1).Hours().At(0, 02, 0).Do(() => { PrintData("Hours  2", DateTime.Now); });
            //Nami.Delay(1500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.EveryTuesday().At(14, 13, 40).Do(() => { PrintData("Nami.EveryTuesday().At(n, n, n)  ", DateTime.Now); });

            // Nami.Every(1).Minutes().Do(() => { PrintData("Nami.Every(1).Minutes()", DateTime.Now); });
            Nami.Every(2).Minutes().At(0, 0, 15).Do(() => { PrintData("Nami.Every(2).Minutes().At(0,0,15)", DateTime.Now); });

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
