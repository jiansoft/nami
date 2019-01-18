using jIAnSoft.Nami.Clockwork;
using jIAnSoft.Nami.Fibers;
using System;
using System.Threading;
using NLog;

namespace Example
{
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            Nami.RightNow().Times(3).Do(() => { PrintData("RightNow 3 times", DateTime.Now); });
            Nami.RightNow().Do(() => { PrintData("Just RightNow   ", DateTime.Now); });
            
            Nami.Everyday().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second+1).Do(() => { PrintData("Everyday   ", DateTime.Now); });
            IFiber pool = new PoolFiber();
            IFiber thread = new ThreadFiber();
            pool.Start();
            thread.Start();

            pool.ScheduleOnInterval(() => { PrintData("pool  ", DateTime.Now); }, 0, 150000);
            var td = thread.ScheduleOnInterval(() => { PrintData("thread", DateTime.Now); }, 0, 100000);
            thread.ScheduleOnInterval(() => { PrintData("thread ten second", DateTime.Now); }, 0, 100000);
            for (var i = 0; i < 100; i++)
            {
                var i1 = i;
                thread.Enqueue(() => { PrintData($"thread  {i1}", DateTime.Now); });
            }

            pool.Schedule(() =>
            {
                Console.WriteLine($"td Dispose");
                td.Dispose();
                for (var i = 0; i < 10; i++)
                {
                    thread.Enqueue(() => { PrintData("Schedule start thread  ", DateTime.Now); });
                }
            }, 20000);
            Nami.Every(10).Seconds().AfterExecuteTask().Do(() => { RunSleepCron("Af", 10, 600); });
            Nami.Every(10).Seconds().AfterExecuteTask().Do(() => { RunSleepCron("Af", 10, 600); });
            Nami.Every(10).Seconds().BeforeExecuteTask().Do(() => { RunSleepCron("Be", 10, 0); });
            Nami.Every(10).Seconds().BeforeExecuteTask().Do(() => { RunSleepCron("Be", 10, 0); });



            Nami.Every(100).Seconds().Do(() =>
            {
                thread.Enqueue(() => { PrintData(" Nami.Every(1).Seconds().Do  ", DateTime.Now); });
            });

            Nami.Every(150).Seconds().Do(() => { PrintData("Nami  ", DateTime.Now); });
            Nami.Every(1).Hours().At(0, 02, 0).Do(() => { PrintData("Hours  2", DateTime.Now); });
            Nami.Delay(1500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.EveryTuesday().At(14, 13, 40).Do(() =>
            {
                PrintData("Nami.EveryTuesday().At(n, n, n)  ", DateTime.Now);
            });

            Nami.Every(1).Minutes().Do(() => { PrintData("Nami.Every(1).Minutes()", DateTime.Now); });
            Nami.Every(2).Minutes().At(0, 0, 15).Do(() =>
            {
                PrintData("Nami.Every(2).Minutes().At(0,0,15)", DateTime.Now);
            });

            Nami.Delay(1000).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(2500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(3500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Nami.Delay(4500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            //thread.Dispose();
            Console.ReadKey();
        }

        private static void PrintData(string name, DateTime date)
        {
            Log.Info($"{name} PrintData => {date:yyyy-MM-dd HH:mm:ss.fff}");
        }

        private static void RunSleepCron(string s, int second, int sleep)
        {
            Log.Info($"{s} every {second} second and sleep {sleep}  now {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            if (sleep <= 0)
            {
                return;
            }

            Thread.Sleep(sleep * 1000);
        }
        private static void RunSleepCron(string s, int second)
        {
            Log.Info($"{s} every {second} second now {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        }
    }
}
