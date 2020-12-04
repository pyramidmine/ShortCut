using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
	internal static class Program
	{
		private static void Main()
		{
			//TestWait();
			//TestWaitWithException();
			//TestContinueWith();
			//TestContinueWithOption();
			TestTaskCancellation();
		}

		private static int Sum(int n)
		{
			Console.WriteLine("Sum method is called.");
			int sum = 0;
			for (; 0 < n; n--)
			{
				sum += n;
			}
			return sum;
		}

		private static int Sum(CancellationToken ct, int n)
		{
			int sum = 0;
			for (; 0 < n; n--)
			{
				// 취소됐다면, 리턴 값으로는 구분이 안되므로
				// 메서드가 작업을 완료하지 않았다는 걸 알려주기 위해 예외 발생
				ct.ThrowIfCancellationRequested();

				// 오버플로우 예외 발생
				// 체크하지 않으면 그냥 롤링 됨
				checked
				{
					sum += n;
				}
			}
			return sum;
		}
		private static void TestWait()
		{
			Task<int> t = new Task<int>(n => Sum((int)n), 100000);
			Console.WriteLine("Task is created.");

			t.Start();

			Console.WriteLine("Now, thread calls Wait");
			t.Wait();

			Console.WriteLine($"The Sum is: {t.Result}");
		}

		private static void TestWaitWithException()
		{
			Console.WriteLine();
			Console.WriteLine("---------- TestWaitWithException ----------");

			var cts = new CancellationTokenSource();
			Task<int> t = Task.Run(() => Sum(cts.Token, 100), cts.Token);

			Thread.Sleep(100);
			cts.Cancel();

			try
			{
				// 태스크 t가
				// 완료됐다면 정상적으로 리턴
				// 취소됐다면 예외 발생
				Console.WriteLine($"The sum is: {t.Result}");
			}
			catch (AggregateException ex)
			{
				// 취소돼서 발생한 예외만 처리 (계획된 취소)
				// 그 외 경우는 계획된 게 아니므로 다시 예외 전파
				ex.Handle(e => e is OperationCanceledException);

				// 여기까지 왔으면 취소된 예외만 있고, 모두 처리된 경우
				Console.WriteLine("Sum was canceled");
			}
		}

		private static void TestContinueWith()
		{
			Console.WriteLine("---------- TestContinueWith starts ----------");

			Task<int> t = Task.Run(() => Sum(CancellationToken.None, 100));

			try
			{
				// 이어서 실행할 태스크를 계속 연결할 수 있음
				// 이때, ContinueWith를 호출하는 주체가 중요함
				// 즉, 호출하는 태스크가 종료되면 다음 태스크가 실행되는 것임
				t.ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, The sum is={task.Result}"))
				 .ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, This is a third task"))
				 .ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, This is a fourth task"));

				// 태스크에 각각 붙이면?
				// t 태스크가 종료되면 이어서 실행할 태스크가 '순서에 상관없이' 실행 됨 (쓰레드 스케줄러 마음)
				/*
				t.ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, The sum is={task.Result}"));
				t.ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, This is a third task"));
				t.ContinueWith(task => Console.WriteLine($"Previous Task={task.Id}, This is a fourth task"));
				*/
			}
			catch (Exception ex)
			{
				Console.WriteLine($"TestContinueWith:EXCEPTION, {ex.GetType().Name}, {ex.Message}");
			}

			Thread.Sleep(1000);
			Console.WriteLine("---------- TestContinueWith ends ----------");
		}

		/// <summary>
		/// ContinueWith 옵션에 따라 선행 쓰레드의 성공/실패/취소 여부에 따라 후행 태스크를 선택 가능
		/// </summary>
		private static void TestContinueWithOption()
		{
			Console.WriteLine("---------- TestContinueWithOption starts ----------");

			// 선행 쓰레드가 성공하는 경우
			{
				Console.WriteLine("Case #1: Success");
				Task<int> t = Task.Run(() => Sum(CancellationToken.None, 100));
				t.ContinueWith(task => Console.WriteLine("Succeeded"), TaskContinuationOptions.OnlyOnRanToCompletion);
				t.ContinueWith(task => Console.WriteLine("Faulted"), TaskContinuationOptions.OnlyOnFaulted);
				t.ContinueWith(task => Console.WriteLine("Canceled"), TaskContinuationOptions.OnlyOnCanceled);
				Thread.Sleep(1000);
			}

			// 선행 쓰레드에서 예외가 발생한 경우 (여기서는 OverflowException)
			{
				Console.WriteLine("Case #2: Exception");
				Task<int> t = Task.Run(() => Sum(CancellationToken.None, 100000));
				t.ContinueWith(task => Console.WriteLine("Succeeded"), TaskContinuationOptions.OnlyOnRanToCompletion);
				t.ContinueWith(task => Console.WriteLine("Faulted"), TaskContinuationOptions.OnlyOnFaulted);
				t.ContinueWith(task => Console.WriteLine("Canceled"), TaskContinuationOptions.OnlyOnCanceled);
				Thread.Sleep(1000);
			}

			// 선행 쓰레드가 취소된 경우
			{
				Console.WriteLine("Case #2: Cancellation");
				var cts = new CancellationTokenSource();

				// NOTE:
				// Task 생성자에 Token을 두 번 전달해야 함
				// 익명 델리깃에 한 번, Task 생성자 파라미터로 한 번
				// 그래야, 이미 취소된 상태면 Task 생성을 건너뛸 수 있기 때문
				Task<int> t = new Task<int>(() => Sum(cts.Token, 50000), cts.Token);
				t.ContinueWith(task => Console.WriteLine("Succeeded"), TaskContinuationOptions.OnlyOnRanToCompletion);
				t.ContinueWith(task => Console.WriteLine("Faulted"), TaskContinuationOptions.OnlyOnFaulted);
				t.ContinueWith(task => Console.WriteLine("Canceled"), TaskContinuationOptions.OnlyOnCanceled);
				t.Start();
				cts.Cancel();
				Thread.Sleep(1000);
			}

			Console.WriteLine("---------- TestContinueWithOption ends ----------");
		}

		/// <summary>
		/// Task 만들 때 취소 토큰을 넣기/안넣기 차이 테스트
		/// </summary>
		/// <remarks>
		/// <para>
		/// 정리:
		/// 1. Task를 만들 때 취소토큰을 파라미터로 넣어주면
		///    태스크의 취소 여부를 확인 가능하므로 예외가 발생하든 아니든
		///    태스크의 상태는 Canceled가 됨
		/// 2. Task를 만들 때 취소토큰을 파라미터로 넣어주지 않으면
		///    태스크 시작 전에 취소된 경우는 RanToCompletion,
		///    태스크 시작 후 취소된 경우는 Faulted 가 돼서
		///    정확한 상태를 알 수 없게 됨
		/// 3. Task를 만들 때 사용하는 델리깃에 넣어주는 취소토큰이 있지 않느냐고 생각할 수 있지만,
		///    Task 입장에서는 멤버가 아닌 델리깃에 넣어주는 파라미터는 액세스가 안 되므로 사용할 수 없음
		///    (즉, Task가 볼 때는 델리깃에 넣어주는 파라미터는 모르는 파라미터임)
		/// </para>
		/// <para>
		/// 결론:
		///	1. Task 생성자에 취소토큰을 파라미터로 넘겨주면
		///	   취소됐을 때 태스크의 상태를 Canceled로 받을 수 있음!
		/// </para>
		/// </remarks>
		private static void TestTaskCancellation()
		{
			Console.WriteLine("---------- TestTaskCancellation starts ----------");

			/// <summary>
			/// 태스크 만들 때 취소토큰을 전달하지 않으므로
			/// 취소토큰 취소 여부와 상관없이 일단 태스크는 실행 됨
			/// 태스크 실행 중에 취소토큰이 취소되었다는 것이 확인되면
			/// (예외 발생을 안 하는 케이스이므로) 태스크는 정상 종료 됨
			/// 따라서, 태스크의 최종 상태는 RanToCompletion
			/// </summary>
			Console.WriteLine("****** Start canceled task, do not pass token to constructor ******");
			StartCanceledTaskTest(passTokenToConstructor: false);

			/// <summary>
			/// 태스크 만들 때 취소토큰을 전달한 상태에서
			/// 태스크를 실행하려고 하면 태스크 생성자에서 '어라? 이 취소토큰은 이미 취소됐잖아?' 하면서
			/// 예외를 발생시키고 태스크는 아예 실행하지 않음
			/// 이때, 태스크의 최종 상태는 Canceled
			/// </summary>
			Console.WriteLine("****** Start canceled task, pass token to constructor ******");
			StartCanceledTaskTest(passTokenToConstructor: true);

			/// <summary>
			/// 태스크 시작 후에 취소토큰을 취소하는 경우
			/// 태스크는 취소토큰이 취소되면 예외를 발생시키는데,
			/// 태스크 입장에서는 태스크의 취소를 모니터링하려고 만든 취소토큰이 취소된 건 지, 아닌 지 알 수 없으므로
			/// 태스크의 최종 상태는 (예외 발생으로 인한) Faulted
			/// </summary>
			Console.WriteLine("****** Throw if cancellation requested, do not pass token to constructor ******");
			ThrowIfCancellationRequestedTest(passTokenToConstructor: false);

			/// <summary>
			/// 태스크는 취소토큰이 생성자에서 받았던 취소토큰과 같은 지 비교할 수 있으므로
			/// 취소토큰이 취소됐다는 예외가 발생했어도 이게 태스크의 취소를 모니터링 하려는 목적으로 만든 취소토큰인 지 확인 가능
			/// 따라서, 태스크의 최종 상태는 Canceled
			/// </summary>
			Console.WriteLine("****** Throw if cancellation requested, pass token to constructor ******");
			ThrowIfCancellationRequestedTest(passTokenToConstructor: true);

			Console.WriteLine();
			Console.WriteLine("Test completed!");
		}

		private static void StartCanceledTaskTest(bool passTokenToConstructor)
		{
			// 태스크 생성
			Console.WriteLine("Creating task");
			var cts = new CancellationTokenSource();
			Task task = null;
			if (passTokenToConstructor)
			{
				task = new Task(() => TaskWork(cts.Token, throwException: false), cts.Token);
			}
			else
			{
				task = new Task(() => TaskWork(cts.Token, throwException: false));
			}

			// 취소
			Console.WriteLine("Canceling task");
			cts.Cancel();

			try
			{
				// 취소 토큰이 활성화 된 상태에서 태스크 시작
				Console.WriteLine("Starting task");
				task.Start();
				task.Wait();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				}
			}

			Console.WriteLine($"Task.Status: {task.Status}");
		}

		private static void ThrowIfCancellationRequestedTest(bool passTokenToConstructor)
		{
			// 태스크 생성
			Console.WriteLine("Creating task");
			var cts = new CancellationTokenSource();
			Task task = null;
			if (passTokenToConstructor)
			{
				task = new Task(() => TaskWork(cts.Token, throwException: true), cts.Token);
			}
			else
			{
				task = new Task(() => TaskWork(cts.Token, throwException: true));
			}

			try
			{
				Console.WriteLine("Starting task");
				task.Start();
				Thread.Sleep(100);

				Console.WriteLine("Canceling task");
				cts.Cancel();
				task.Wait();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				if (ex.InnerException != null)
				{
					Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
				}
			}

			Console.WriteLine($"Task.Status: {task.Status}");
		}

		private static void TaskWork(CancellationToken ct, bool throwException)
		{
			int loop = 0;

			while (true)
			{
				loop++;
				Console.WriteLine($"Task: loop={loop}");

				ct.WaitHandle.WaitOne(50);
				if (ct.IsCancellationRequested)
				{
					Console.WriteLine("Task: cancellation requested");
					if (throwException)
					{
						ct.ThrowIfCancellationRequested();
					}

					break;
				}
			}
		}
	}
}
