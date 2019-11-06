using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var task = Task.Run(() =>
			{
				Thread.Sleep(1000);
				Console.WriteLine($"Task.Run");
			});

			Console.WriteLine("Main: After Task.Run");
			task.Wait();
		}
	}
}
