using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTest
{
	internal static class Program
	{
		/// <summary>
		/// Parallel.ForEach 테스트
		/// 결과:
		/// 1. ForEach는 액션 델리깃 안에서 1차로 분기되는 모든 쓰레드를 기다림 (원래 목적이 그거니까)
		/// 2. 그러나, 1차 쓰레드에서 분기되는 2차 쓰레드는 기다리지 않음 (2차 쓰레드의 존재를 모름)
		/// 3. 이때 1차 쓰레드의 Attached/Detached 상태는 상관 없음
		/// 4. async/await 경우에는 1차 쓰레드도 기다리지 않음 (원래 목적이 그거니까)
		/// </summary>
		private static void Main()
		{
			int[] numbers = new int[5] { 100, 200, 300, 400, 500 };

			Stopwatch stopwatch = new Stopwatch();

			// 1. Wait + Attached
			if (true)
			{
				stopwatch.Restart();
				Parallel.ForEach(numbers, item => TestMethod(item, shouldWait: true, attached: true));
				stopwatch.Stop();
				Console.WriteLine($"Wait and Attached: {stopwatch.ElapsedMilliseconds:N0}ms");
			}

			// 2. Wait + Detached
			if (false)
			{
				stopwatch.Restart();
				Parallel.ForEach(numbers, item => TestMethod(item, shouldWait: true, attached: false));
				stopwatch.Stop();
				Console.WriteLine($"Wait and Detached: {stopwatch.ElapsedMilliseconds:N0}ms");
			}

			// 3. Not Wait + Attached
			if (false)
			{
				stopwatch.Restart();
				Parallel.ForEach(numbers, item => TestMethod(item, shouldWait: false, attached: true));
				stopwatch.Stop();
				Console.WriteLine($"Not Wait and Attached: {stopwatch.ElapsedMilliseconds:N0}ms");
			}

			// 4. Not Wait + Detached
			if (false)
			{
				stopwatch.Restart();
				Parallel.ForEach(numbers, item => TestMethod(item, shouldWait: false, attached: false));
				stopwatch.Stop();
				Console.WriteLine($"Not Wait and Detached: {stopwatch.ElapsedMilliseconds:N0}ms");
			}

			// 5. async/await + Detached
			if (false)
			{
				stopwatch.Restart();
				Parallel.ForEach(numbers, item => TestMethodAsync(item));
				stopwatch.Stop();
				Console.WriteLine($"async/await and Detached: {stopwatch.ElapsedMilliseconds:N0}ms");
			}

			// 6. numbers 출력
			// --> Parallel.ForEach 메서드가 Task 실행을 끝까지 책임지는지 확인
			Console.Write("numbers: ");
			foreach (var number in numbers)
			{
				Console.Write($"{number}, ");
			}
			Console.Write(Environment.NewLine);

			// 6. 종료 대기
			Console.Read();
		}

		private static void TestMethod(int number, bool shouldWait, bool attached)
		{
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: TestMethod, shouldWait={shouldWait}, attached={attached}, number={number}, start...");
			if (shouldWait)
			{
				if (attached)
				{
					Task.Factory.StartNew(() => DoSomething(number).Wait(), TaskCreationOptions.AttachedToParent);
				}
				else
				{
					Task.Factory.StartNew(() => DoSomething(number).Wait());
				}
			}
			else
			{
				if (attached)
				{
					Task.Factory.StartNew(() => DoSomething(number), TaskCreationOptions.AttachedToParent);
				}
				else
				{
					Task.Factory.StartNew(() => DoSomething(number));
				}
			}
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: TestMethod, shouldWait={shouldWait}, attached={attached}, number={number}, end.");
		}

		private static async void TestMethodAsync(int number)
		{
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: async/await={true}, number={number}, start...");
			await Task.Delay(number).ConfigureAwait(false);
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: async/await={true}, number={number}, end.");
		}

		private static Task DoSomething(int milliseconds)
		{
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: DoSomething, number={milliseconds}, start...");
			Task.Delay(milliseconds).Wait();
			Console.WriteLine($"{DateTime.Now:HHmmss:ffff}: DoSomething, number={milliseconds}, end.");
			return Task.CompletedTask;
		}
	}
}
