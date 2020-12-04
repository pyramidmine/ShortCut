using StackExchange.Redis;
using System;
using System.Text;
using System.Threading;

namespace RedisWithStackExchangeTest
{
	internal static class Program
	{
		private const string redisServerIP = "192.168.0.46:16379";
		private const string redisPassword = "Ekswnr59";

		private static void Main()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisServerIP},allowAdmin=true,password={redisPassword}");

			TestIndividualServer(redis);
			/*
						TestPubSub(redis);
						TestString(redis);
						TestHash(redis);
			*/

			Console.WriteLine("Press any key to exit: ");
			Console.ReadLine();
		}

		private static void TestIndividualServer(ConnectionMultiplexer redis)
		{
			foreach (var endPoint in redis.GetEndPoints())
			{
				Console.WriteLine(endPoint.ToString());

				IServer server = redis.GetServer(endPoint);
				Console.WriteLine($"Server Info: LastSave={server.LastSave()}, ClientCount={server.ClientList().Length}");
			}
		}

		private const string channelName = "messages";
		private static void TestPubSub(ConnectionMultiplexer redis)
		{
			Console.WriteLine($"Main.TestPubSub: Thread hash code: {Thread.CurrentThread.GetHashCode()}");

			ISubscriber sub = redis.GetSubscriber();
			sub.Subscribe(channelName).OnMessage(channelMessage =>
			{
				Console.WriteLine($"Thread hash code: {Thread.CurrentThread.GetHashCode()}");
				Console.WriteLine($"Redis Subscriber: channel={channelName}, message={channelMessage}");
			});
		}

		private const string stringKey = "worker:name:w12345";
		private const string stringValue = "이문환";
		private static readonly byte[] bytesKey = Encoding.UTF8.GetBytes("worker:name:w67890");
		private static readonly byte[] bytesValue = Encoding.UTF8.GetBytes("주정");
		private static void TestString(ConnectionMultiplexer redis)
		{
			IDatabase db = redis.GetDatabase();

			// string 타입
			{
				bool setResult = db.StringSet(stringKey, stringValue);
				var outputValue = db.StringGet(stringKey);
				Console.WriteLine($"StringTest: value: input='{stringValue}', output='{outputValue}'");
			}

			// byte[] 타입
			{
				bool setResult = db.StringSet(bytesKey, bytesValue);
				var outputValue = db.StringGet(bytesKey);
				Console.WriteLine($"StringTest: value: input='{bytesValue}', output='{outputValue}'");
			}
		}

		private static void TestHash(IDatabase redis)
		{
			throw new NotImplementedException();
		}
	}
}
