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
            var now = DateTime.Now;
            Log.Info($"Start at {now:HH:mm:ss}");
            
            /*Nami.Delay(20000).Do(() =>
            {
                foreach (var s in new[] {1, 2, 3})
                {
                    var ss = s;
                    Nami.Delay(2000).Do(() =>
                    {
                        Loop(ss, 0);
                    });
                }
            });*/
            Nami.Every(450).Milliseconds().Times(2).Do(() => { PrintData("Every 450 Milliseconds", DateTime.Now); });
            Nami.Every(1).Seconds().Times(3).Do(() => { PrintData("Every 1 Seconds Times 3", DateTime.Now); });
            Nami.Every(10).Minutes().Do(() => { PrintData("Every 10 Minutes", DateTime.Now); });
            Nami.Every(10).Minutes().AfterExecuteTask().Do(() =>
            {
                PrintData("Every 10 Minutes and AfterExecuteTask(didn't work)", DateTime.Now);
                Thread.Sleep(4 * 60 * 1000);
            });
            Nami.Every(600).Seconds().AfterExecuteTask().Do(() =>
            {
                PrintData("Every 600 Seconds and sleep 4 Minutes", DateTime.Now);
                Thread.Sleep(4 * 60 * 1000);
            });
            Nami.Delay(4000).Times(4).Do(() => { PrintData("Delay 4000 ms Times 4", DateTime.Now); });

            now = now.AddSeconds(17).AddMilliseconds(100);
            Nami.EveryMonday().At(now.Hour, now.Minute, now.Second).Do(() => { PrintData("Monday", DateTime.Now); });
            Nami.EveryTuesday().At(now.Hour, now.Minute, now.Second).Do(() => { PrintData("Tuesday", DateTime.Now); });
            Nami.EveryWednesday().At(now.Hour, now.Minute, now.Second)
                .Do(() => { PrintData("Wednesday", DateTime.Now); });
            Nami.EveryThursday().At(now.Hour, now.Minute, now.Second)
                .Do(() => { PrintData("Thursday", DateTime.Now); });
            Nami.EveryFriday().At(now.Hour, now.Minute, now.Second).Do(() => { PrintData("Friday", DateTime.Now); });
            Nami.EverySaturday().At(now.Hour, now.Minute, now.Second)
                .Do(() => { PrintData("Saturday", DateTime.Now); });
            Nami.EverySunday().At(now.Hour, now.Minute, now.Second).Do(() => { PrintData("Sunday", DateTime.Now); });

            now = now.AddSeconds(1);
            Nami.Every(1).Hours().At(now.Hour, now.Minute, now.Second).Do(() =>
            {
                PrintData("Every 1 Hours", DateTime.Now);
            });

            now = now.AddSeconds(1);
            Nami.Every(1).Days().At(now.Hour, now.Minute, now.Second)
                .Do(() => { PrintData("Every 1 Days", DateTime.Now); });

            now = now.AddSeconds(1);
            Nami.Everyday().At(now.Hour, now.Minute, now.Second).Do(() => { PrintData("Everyday", DateTime.Now); });

            Console.ReadKey();

            Nami.EverySunday().At(6, 0, 0).Do(() => { PrintData("EverySunday().Days().At(6,0,0)   ", DateTime.Now); });
            Nami.EveryMonday().At(6, 0, 0).Do(() => { PrintData("EveryMonday().Days().At(6,0,0)   ", DateTime.Now); });
            Nami.EveryMonday().At(12, 0, 0).Do(() =>
            {
                PrintData("EveryMonday().Days().At(12,0,0)   ", DateTime.Now);
            });

            Nami.EveryTuesday().At(6, 0, 0).Do(() => { PrintData("EveryMonday().Days().At(6,0,0)   ", DateTime.Now); });
            Nami.EveryTuesday().At(12, 0, 0)
                .Do(() => { PrintData("EveryMonday().Days().At(12,0,0)   ", DateTime.Now); });

            Nami.Everyday().At(6, 0, 0).Do(() => { PrintData("Everyday().At(6,0,0)   ", DateTime.Now); });

            Nami.Every(1).Days().Do(() => { PrintData("Every(1).Days()   ", DateTime.Now); });
            Nami.Every(1).Days().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 2).Do(() =>
            {
                PrintData("Every(1).Days().At   ", DateTime.Now);
            });
            Nami.Every(2).Days().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 2).Do(() =>
            {
                PrintData("Every(2).Days().At   ", DateTime.Now);
            });

            Nami.Every(1).Hours().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 2).Do(() =>
            {
                PrintData("Every Hours   ", DateTime.Now);
            });
            Nami.Every(2).Hours().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 2).Do(() =>
            {
                PrintData("Every 2 Hours   ", DateTime.Now);
            });
            Nami.Every(1).Minutes().Do(() => { PrintData("Every(1).Minutes()", DateTime.Now); });
            Nami.Every(2).Minutes().Do(() => { PrintData("Every(2).Minutes()", DateTime.Now); });

            Nami.Delay(1).Seconds().Times(5).Do(() =>
            {
                Log.Info($"Nami.Delay(1).Seconds() {DateTime.Now:HH:mm:ss}");
            });
            var d = Nami.Delay(1).Seconds().Times(5).Do(() => { PrintData(" Dispose ", DateTime.Now); });
            d.Dispose();
            Nami.Delay(30).Seconds().Times(5).Do(() => { PrintData("Delay(30) 5 times", DateTime.Now); });
            Nami.RightNow().Times(3).Do(() => { PrintData("RightNow 3 times", DateTime.Now); });
            Nami.RightNow().Do(() => { PrintData("Just RightNow   ", DateTime.Now); });
            Nami.Delay(1000).Milliseconds().Do(() =>
            {
                PrintData($"Just Delay(1000) execute:{DateTime.Now:HH:mm:ss.fff}", DateTime.Now);
            });
            Nami.Every(60000).Milliseconds().Do(() =>
            {
                PrintData("Just Every(60000).Milliseconds()   ", DateTime.Now);
            });
            Nami.Everyday().At(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second + 5)
                .Do(() => { PrintData("Everyday   ", DateTime.Now); });

            Console.ReadKey();
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
            Log.Info($"{name} {date:yyyy-MM-dd HH:mm:ss.fff}");
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

        private static void Loop(int index, int count)
        {
            Log.Info($"{index} loop {count} now {DateTime.Now:HH:mm:ss.fff}");
           
            var ss = index;
            Nami.Delay(2000).Do(() => { Loop(ss, ++count); });
        }
    }
}
