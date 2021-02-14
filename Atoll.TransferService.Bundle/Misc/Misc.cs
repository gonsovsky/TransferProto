using System;
using System.Diagnostics;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{
    public static class Misc
    {
        public static Task Delay(int milliseconds)
        {
#if NETSTANDARD
            return Task.Delay(milliseconds);
#else
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.TrySetResult(true);
            };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
#endif
        }

        public static void Log(string value)
        {
#if DEBUG
            Debug.WriteLine(value);
#else
            Console.WriteLine(value);
#endif
            Console.WriteLine(value);
        }

        public static void Log(Exception exception, bool expected = true)
        {
            var value = $"[{(expected ? "EXPECTED" : "UNEXPECTED")}] {exception}: {exception.Message}";
#if DEBUG
            Debug.WriteLine(value);
#else
            Console.WriteLine(value);
#endif
        }
    }
}
