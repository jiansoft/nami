# nami

Nami is a high performance C# threading library.
This project fork from [retlang](<https://code.google.com/archive/p/retlang/>).

### Features

* Fiber
* Cron c# job scheduling for humans. It is inspired by [schedule](<https://github.com/dbader/schedule>).
  


Usage
================

### Quick Start
1. Install Nuget package. [here](https://www.nuget.org/packages/jIAnSoft.Nami/).

2.Use examples
``` csharp
using jIAnSoft.Framework.Nami.Fibers;
using jIAnSoft.Framework.Nami.TaskScheduler;

 internal static class Program
    {
        private static void Main(string[] args)
        {
            IFiber pool = new PoolFiber();
            IFiber thread = new ThreadFiber();
            pool.Start();
            thread.Start();
            //After 1 second execute first and every 15 second execute once. 
            pool.ScheduleOnInterval(() => { PrintData("pool  ", DateTime.Now); }, 1000, 15000);
            //After 0 second execute first and every 15 second execute once. 
            var td = thread.ScheduleOnInterval(() => { PrintData("thread", DateTime.Now); }, 0, 15000);
            //After 2 second cancel the td schedule
            pool.Schedule(() =>
            {
                Console.WriteLine($"td Dispose");
                td.Dispose();
            }, 20000);

            Nami.Every(1).Hours().At(0, 2, 0).Do(() => { PrintData("Hours  2", DateTime.Now); });
            Nami.Every(2).Minutes().At(0,0,15).Do(() => { PrintData("Nami.Every(2).Minutes().At(0,0,15)", DateTime.Now); });
            Nami.Every(1).Minutes().Do(() => { PrintData("Nami.Every(1).Minutes()", DateTime.Now); });
            Nami.Every(15).Seconds().Do(() => { PrintData("Nami  ", DateTime.Now); });
            Nami.EveryTuesday().At(14, 13, 40).Do(() => { PrintData("Nami.EveryTuesday().At(n, n, n)  ", DateTime.Now); });
            Nami.Delay(1500).Do(() => { PrintData("Delay  ", DateTime.Now); });
            Console.ReadKey();
        }

        private static void PrintData(string name, DateTime date)
        {
            Console.WriteLine($"{name} PrintData => {date:yyyy-MM-dd HH:mm:ss.fff}");
        }
    }

```
## License

Copyright (c) 2017

Released under the MIT license:

- [www.opensource.org/licenses/MIT](http://www.opensource.org/licenses/MIT)