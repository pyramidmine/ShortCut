using System;
using System.Threading;

namespace ThreadPoolExceptionTest
{
	public static class Program
	{
		public static void Main()
		{
			Console.WriteLine("Main thread: Queuing an asynchronous operation");
			ThreadPool.QueueUserWorkItem(ComputeBoundOp, 5);
			Console.WriteLine("Main thread: Doing other work here...");
			Thread.Sleep(5000);
			Console.WriteLine("Hit <ENTER> key to end this program...");
			Console.ReadLine();
		}

		private static void ComputeBoundOp(object state)
		{
			Console.WriteLine($"In ComputeBoundOp: state={state}");
			Thread.Sleep(1000);

			//
			// 쓰레드풀 콜백메서드에서 예외가 발생하면
			// 메인 프로세스가 죽습니다!
			//
			throw new InvalidOperationException();
		}
	}
}
