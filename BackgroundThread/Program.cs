using System;
using System.Diagnostics;
using System.Threading;

namespace BackgroundThread
{
	internal static class Program
	{
		private static CancellationTokenSource cts;

		public static void Main()
		{
			cts = new CancellationTokenSource();

			Thread thread = new Thread(PrintThread);
			thread.Start();

			// 300ms 지난 후에 쓰레드 멈춤 지시
			Thread.Sleep(300);
			cts.Cancel();

			Console.WriteLine($"End of Main(), ProcessID={Process.GetCurrentProcess().Id}, ThreadId={Thread.CurrentThread.ManagedThreadId}");
		}

		private static void PrintThread()
		{
			for (int i = 0; i < 5; i++)
			{
				if (cts.IsCancellationRequested)
				{
					Console.WriteLine($"PrintThread, IsCancellationRequested={cts.IsCancellationRequested}");
					break;
				}

				Console.WriteLine($"PrintThread: {i}");
				Thread.Sleep(200);
			}
			Console.WriteLine($"End of PrintThread(), ProcessID={Process.GetCurrentProcess().Id}, ThreadId={Thread.CurrentThread.ManagedThreadId}");
		}
	}
}
