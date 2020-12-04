using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManualResetEventSlimTest
{
	internal class Program
	{
		private static readonly ManualResetEventSlim mres = new ManualResetEventSlim();

		static void Main(string[] args)
		{
			mres.Reset();
			Task.Factory.StartNew(() =>
			{
				Console.WriteLine("First task...");
				Thread.Sleep(500);
				mres.Set();
			});
			bool signaled = mres.Wait(1000);
			Console.WriteLine($"First task is signaled = {signaled}");

			mres.Reset();
			Task.Factory.StartNew(() =>
			{
				Console.WriteLine("Second task...");
				Thread.Sleep(500);
				mres.Set();
			});
			signaled = mres.Wait(1000);
			Console.WriteLine($"Second task is signaled = {signaled}");
		}
	}
}
