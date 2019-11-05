using System;
using System.Collections.Generic;
using System.Threading;

namespace MultiThreadTest
{
    class Program
    {
        static int TotalThreads { get; } = 5;
        static int totalCount = 0;

        static void Main(string[] args)
        {
            Thread t = new Thread(new ThreadStart(DoSomething));
            t.Start();
            Console.WriteLine($"t.Background = {t.IsBackground}");
            Thread.Sleep(30);
            t.Abort();
            t.Join();
            Console.WriteLine($"Main: t.Join");

            PrintThreadState(ThreadState.Aborted);
            PrintThreadState(ThreadState.Aborted | ThreadState.AbortRequested);

            Thread t2 = new Thread(new ThreadStart(DoSpin));
            t2.Start();
            Thread.Sleep(10);
            Console.WriteLine($"Main: t2.Start executed");
            t2.Interrupt();
            t2.Join();
            Console.WriteLine($"Main: t2.Join");

            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < TotalThreads; i++)
            {
				threads.Add(new Thread(DoAdd));
            }
            foreach (var thread in threads)
            {
                thread.Start((object)80000);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
            Console.WriteLine($"Program.totalCount = {totalCount}");
        }

        static void DoSomething()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"DoSomething: {i}");
                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException ex)
            {
                Console.WriteLine(ex);
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Thread.ResetAbort();
            }

            Console.WriteLine($"End of DoSomething.");
        }

        static void PrintThreadState(ThreadState ts)
        {
            Console.WriteLine($"{ts,-16}: {(int)ts, -4}");
        }

        static void DoSpin()
        {
            try
            {
                Thread.SpinWait(2000000000);
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"DoSpin: {i}");
                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine($"End of DoSpin");
        }

        static void DoAdd(object count)
        {
			int loop = (int)count;
            for (int i = 0; i < loop; i++)
            {
                Program.totalCount = Program.totalCount + 1;
            }
        }
    }
}
