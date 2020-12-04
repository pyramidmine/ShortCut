using System;
using System.Threading;

namespace CancellationTokenSourceTest
{
	internal static class Program
	{
		private static void Main()
		{
			/*
			TestRegister();
			TestRegisterWithException(false);
			TestRegisterWithException(true);
			TestRegisterWithCancaledSource();
			TestRegisterMultipleTimes();
			TestRegisterWithLink();
			*/
			TestRegisterWithCancelAfter();

			Console.ReadLine();
		}

		private static void TestRegister()
		{
			var cts = new CancellationTokenSource();
			cts.Token.Register(() => Console.WriteLine("Canceled 1"));
			cts.Token.Register(() => Console.WriteLine("Canceled 2"));
			cts.Cancel();

			// 출력은 입력의 역순인 것으로 봐선 스택 형태인 듯
			/*
			Canceled 2
			Canceled 1
			*/
		}

		/// <summary>
		/// 취소 콜백에 등록된 델리깃에 예외가 발생했을 때의 처리 테스트
		/// </summary>
		/// <param name="throwOnFirstException">
		/// <list type="bullet">
		///		<item>
		///			<term>false</term>
		///			<description>
		///				콜백 체인에서 예외가 발생하면 AggregateException으로 모아서 처리.<br/>
		///				콜백 체인에 포함된 모든 델리깃이 호출 됨.
		///			</description>
		///		</item>
		///		<item>
		///			<term>true</term>
		///			<description>
		///				콜백 체인에서 예외가 발생하면 바로 예외를 전파.<br/>
		///				콜백 체인 중간에 예외가 발생하면 나머지 델리깃은 호출되지 않음.
		///			</description>
		///		</item>
		/// </list>
		/// </param>
		private static void TestRegisterWithException(bool throwOnFirstException)
		{
			Console.WriteLine($"Called with throwOnFirstException={throwOnFirstException}");

			try
			{
				var cts = new CancellationTokenSource();
				cts.Token.Register(() => { Console.WriteLine("Canceled 1"); throw new ArgumentException("Canceled 1"); });
				cts.Token.Register(() => { Console.WriteLine("Canceled 2"); throw new ArgumentOutOfRangeException("Canceled 2"); });
				cts.Cancel(throwOnFirstException);
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType()}, {ex.Message}");
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType()}, {ex.Message}");
			}
			catch (AggregateException ex)
			{
				Console.WriteLine($"EXCEPTION: {ex.GetType()}, {ex.Message}");
				foreach (var innerException in ex.InnerExceptions)
				{
					Console.WriteLine($"EXCEPTION: {innerException.GetType()}, {innerException.Message}");
				}
			}
		}

		/// <summary>
		/// 이미 취소된 CancellationTokenSource에 콜백을 연결하면 어떻게 되는지 테스트
		/// </summary>
		/// <remarks>
		/// 콜백을 등록하는 순간 호출 됨
		/// </remarks>
		private static void TestRegisterWithCancaledSource()
		{
			var cts = new CancellationTokenSource();
			cts.Cancel();
			cts.Token.Register(() => Console.WriteLine("Canceled 1"));
			cts.Token.Register(() => Console.WriteLine("Canceled 2"));
		}

		/// <summary>
		/// 같은 콜백을 여러 번 등록하면 어떻게 되는지 테스트
		/// </summary>
		/// <remarks>
		/// 호출 됨
		/// </remarks>
		private static void TestRegisterMultipleTimes()
		{
			var cts = new CancellationTokenSource();
			cts.Token.Register(CanceledCallback);
			cts.Token.Register(CanceledCallback);
			cts.Cancel();
		}

		private static void CanceledCallback()
		{
			Console.WriteLine("CanceledCallback called.");
		}

		/// <summary>
		/// 여러 개의 CancellationTokenSource를 링크로 연결해서 취소 테스트
		/// </summary>
		/// <remarks>
		/// - 링크로 연결한 CTS는, 연결된 CTS 중에 하나만 Cancel() 해도 콜백 호출됨
		/// - 이와 함께 실제 Cancel() 호출 된 CTS도 콜백 호출 됨
		/// </remarks>
		private static void TestRegisterWithLink()
		{
			var cts1 = new CancellationTokenSource();
			cts1.Token.Register(() => Console.WriteLine("cts1 canceled."));

			var cts2 = new CancellationTokenSource();
			cts2.Token.Register(() => Console.WriteLine("cts2 canceled."));

			var cts3 = new CancellationTokenSource();
			cts3.Token.Register(() => Console.WriteLine("cts3 canceled."));

			var lcts = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[] { cts1.Token, cts2.Token, cts3.Token });
			lcts.Token.Register(() => Console.WriteLine("lcts canceled."));

			cts2.Cancel();

			Console.WriteLine("cts1 canceled={0}, cts2 canceled={1}, cts3 canceled={2}, lcts canceled={3}",
				cts1.IsCancellationRequested,
				cts2.IsCancellationRequested,
				cts3.IsCancellationRequested,
				lcts.IsCancellationRequested);
		}

		/// <summary>
		/// 일정 시간 후 자동 취소 테스트
		/// </summary>
		private static void TestRegisterWithCancelAfter()
		{
			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] Auto cancellation with constructor timeout milliseconds...");
			var cts1 = new CancellationTokenSource(2150);
			cts1.Token.Register(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] cts1 canceled"));
			Thread.Sleep(3000);

			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] Auto cancellation with constructor time span...");
			var cts2 = new CancellationTokenSource(new TimeSpan(0, 0, 0, 2, 150));
			cts2.Token.Register(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] cts2 canceled"));
			Thread.Sleep(3000);

			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] Auto cancellation with Cancel(milliseconds) method...");
			var cts3 = new CancellationTokenSource();
			cts3.Token.Register(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] cts3 canceled"));
			cts3.CancelAfter(2150);
			Thread.Sleep(3000);

			Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] Auto cancellation with Cancel(TimeSpan) method...");
			var cts4 = new CancellationTokenSource();
			cts4.Token.Register(() => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.FFF}] cts4 canceled"));
			cts4.CancelAfter(new TimeSpan(0, 0, 0, 2, 150));
			Thread.Sleep(3000);
		}
	}
}
