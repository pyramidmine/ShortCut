using MannaPlanet.MannaLog;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedisStressTest
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length != 4)
			{
				Console.WriteLine("Usage: RedisStressTest endpoint password user-count");
				return;
			}

			Log logger = new LogBuilder().Build();
			CancellationTokenSource cts = new CancellationTokenSource();

			try
			{
				logger.Info($"Redis Server: {args[0]}, connecting...");
				ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{args[0]},allowAdmin=true,password={args[1]},ConnectTimeout=1000");
				logger.Info($"Redis Server: {args[0]}, connected.");

				IDatabase db = redis.GetDatabase();

				// 스탯 출력 쓰레드 시작
				
			}
			catch (Exception ex)
			{
				logger.Error($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
			}

			Console.Read();
		}
	}

	public class OrderUpdateTime
	{
		public string OrderID { get; set; }
		public double UpdateTime { get; set; }
	}
}
