using StackExchange.Redis;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace RedisPerformanceTest
{
	internal static class Program
	{
		private static readonly (float South, float North) LatitudesOfSeoul = (37.4133F, 37.7151F);
		private static readonly (float West, float East) LongitudeOfSeoul = (126.7341F, 127.2693F);
		private const int NumOfWorkers = 1000;
		private const int NumOfRepeats = 1;
		private static readonly List<WorkerInfo> wis = new List<WorkerInfo>();
		private static readonly List<Worker> ws = new List<Worker>();
		private static readonly Random rnd = new Random();

		static Program()
		{
			Random rnd = new Random();

			for (int i = 0; i < NumOfWorkers; i++)
			{
				wis.Add(new WorkerInfo
				{
					Name = $"W{i:D5}",
					Longitude = ToLocationString(GetRandomNumber(rnd, LongitudeOfSeoul.West, LongitudeOfSeoul.East)),
					Latitude = ToLocationString(GetRandomNumber(rnd, LatitudesOfSeoul.South, LatitudesOfSeoul.North)),
					UpdateTime = ToUpdateTimeString(DateTime.Now),
					IsOnline = "Y"
				});

				ws.Add(new Worker
				{
					Name = wis[i].Name,
					Info = $"{wis[i].Longitude}:{wis[i].Latitude}:{wis[i].UpdateTime}"
				});
			}
		}

		private static void Main()
		{
			ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(RedisServer.GetConnectionString());
			Debug.Assert(redis != null);

			IDatabase db = redis.GetDatabase();
			Debug.Assert(db != null);

			const string listKeyPrefix = "pf-list-test:worker:info:";

			DeleteKeys(true, db, listKeyPrefix);
			TestPerformanceDriver(true, db, "ListSetByIndexAsync,Parallel,Wait", listKeyPrefix, ListSetByIndexAsyncWithEach, parallel: true, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, listKeyPrefix);
			TestPerformanceDriver(false, db, "ListSetByIndexAsync,Parallel,NoWait", listKeyPrefix, ListSetByIndexAsyncWithEach, parallel: true, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(true, db, listKeyPrefix);
			TestPerformanceDriver(true, db, "ListSetByIndexAsync,Sequential,Wait", listKeyPrefix, ListSetByIndexAsyncWithEach, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, listKeyPrefix);
			TestPerformanceDriver(false, db, "ListSetByIndexAsync,Sequential,NoWait", listKeyPrefix, ListSetByIndexAsyncWithEach, parallel: false, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			DeleteKeys(true, db, listKeyPrefix);
			TestPerformanceDriver(true, db, "ListTrimAndRightPush,Wait", listKeyPrefix, ListTrimAndRightPush, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, listKeyPrefix);
			TestPerformanceDriver(false, db, "ListTrimAndRightPush,NoWait", listKeyPrefix, ListTrimAndRightPush, parallel: false, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			DeleteKeys(true, db, listKeyPrefix);
			TestPerformanceDriver(true, db, "ListDeleteKeyAndRightPush,Wait", listKeyPrefix, ListDeleteKeyAndRightPush, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, listKeyPrefix);
			TestPerformanceDriver(false, db, "ListDeleteKeyAndRightPush,NoWait", listKeyPrefix, ListDeleteKeyAndRightPush, parallel: false, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			const string hashKeyPrefix = "pf-hash-test:worker:info:";

			DeleteKeys(false, db, hashKeyPrefix);
			TestPerformanceDriver(false, db, "HashSet,Wait", hashKeyPrefix, HashSet, parallel: true, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, hashKeyPrefix);
			TestPerformanceDriver(false, db, "HashSet,NoWait", hashKeyPrefix, HashSet, parallel: true, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			DeleteKeys(true, db, hashKeyPrefix);
			TestPerformanceDriver(true, db, "HashSetAsync,Wait", hashKeyPrefix, HashSetAsync, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
			DeleteKeys(false, db, hashKeyPrefix);
			TestPerformanceDriver(false, db, "HashSetAsync,NoWait", hashKeyPrefix, HashSetAsync, parallel: false, waitAndPostset: false, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			TestPerformanceDriver(false, db, "HashGet,Wait", hashKeyPrefix, HashGet, parallel: true, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			TestPerformanceDriver(true, db, "HashGetAsync,Wait", hashKeyPrefix, HashGetAsync, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			RedisKey sortedsetKey = "sortedset-test";
			db.KeyDelete(sortedsetKey);
			TestPerformanceDriver(true, db, "SortedSetAddAsync,Wait", sortedsetKey, SortedSetAddAsync, parallel: false, waitAndPostset: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");

			RedisKey stringKeyPrefix = "string-test:";
			DeleteKeys(true, db, stringKeyPrefix);
			TestPerformanceDriver(true, db, "StringSet,Parallel,Wait,Async", stringKeyPrefix, StringSet, parallel: true, waitAndPostset: true, asynchronous: true, NumOfRepeats);
			Console.WriteLine("-----------------------------------------------------------------------------------");
		}

		private static void DeleteKeys(bool enabled, IDatabase db, string keyPrefix)
		{
			if (!enabled)
			{
				return;
			}

			for (int i = 0; i < wis.Count; i++)
			{
				RedisKey key = string.Format($"{keyPrefix}{wis[i].Name}");
				db.KeyDelete(key);
			}
		}

		/// <summary>
		/// Redis List, Hash 기능을 테스트하는 테스트 드라이버
		/// </summary>
		/// <param name="enabled">테스트를 수행할 지 결정. false면 바로 리턴</param>
		/// <param name="db">IDatabase 객체</param>
		/// <param name="title">수행시간을 출력할 때 표시할 타이틀</param>
		/// <param name="keyPrefix">키 생성할 때 앞에 붙일 접두사. 접두사 뒤에는 WorkerInfo.Name이 붙음</param>
		/// <param name="actor">수행할 테스트</param>
		/// <param name="parallel">병렬 실행 여부. true이면 Parallel.ForEach를 사용하고, false이면 for 루프 사용. actor 메서드가 동기식이면 true, 비동기식이면 false 설정 추천</param>
		/// <param name="waitAndPostset">태스크 종료를 기다릴 지 설정</param>
		/// <param name="repeat">몇 번 반복할 지 설정. 실제 수행 횟수는 repeat * wis.Count</param>
		private static void TestPerformanceDriver(
			bool enabled,
			IDatabase db,
			string title,
			string keyPrefix,
			Action<WorkerInfo, IDatabase, RedisKey, RedisValue[], bool> actor,
			bool parallel,
			bool waitAndPostset,
			int repeat)
		{
			if (!enabled)
			{
				return;
			}

			// 시간 측정은 Stopwatch 활용
			Stopwatch stopwatch = new Stopwatch();

			// Actor 시간 측정
			if (actor != null)
			{
				stopwatch.Restart();
				{
					if (parallel)
					{
						for (int i = 0; i < repeat; i++)
						{
							Parallel.ForEach(wis, item => actor(
								item,
								db,
								$"{keyPrefix}{item.Name}",
								new RedisValue[] {
									item.Name,
									IncreaseLocationString(item.Longitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
									IncreaseLocationString(item.Latitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
									ToUpdateTimeString(DateTime.Now)
								},
								waitAndPostset));
						}
					}
					else
					{
						for (int i = 0; i < repeat; i++)
						{
							for (int j = 0; j < wis.Count; j++)
							{
								actor(
									wis[j],
									db,
									$"{keyPrefix}{wis[j].Name}",
									new RedisValue[]
									{
										wis[j].Name,
										IncreaseLocationString(wis[j].Longitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
										IncreaseLocationString(wis[j].Latitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
										ToUpdateTimeString(DateTime.Now)
									},
									waitAndPostset);
							}
						}
					}
				}
				stopwatch.Stop();
			}

			// 시간 측정 결과 출력
			PrintStatistics(title, repeat * wis.Count, stopwatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// SortedSet 전용 성능 측정 드라이버
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="db"></param>
		/// <param name="title"></param>
		/// <param name="keyPrefix"></param>
		/// <param name="actor"></param>
		/// <param name="parallel"></param>
		/// <param name="waitAndPostset"></param>
		/// <param name="repeat"></param>
		private static void TestPerformanceDriver(
			bool enabled,
			IDatabase db,
			string title,
			RedisKey key,
			Action<WorkerInfo, IDatabase, RedisKey, double, bool> actor,
			bool parallel,
			bool waitAndPostset,
			int repeat)
		{
			if (!enabled)
			{
				return;
			}

			// 시간 측정은 Stopwatch 활용
			Stopwatch stopwatch = new Stopwatch();

			// Actor 시간 측정
			if (actor != null)
			{
				stopwatch.Restart();
				{
					long score = 1;
					if (parallel)
					{
						for (int i = 0; i < repeat; i++)
						{
							Parallel.ForEach(wis, item => actor(
								item,
								db,
								key,
								(double)Interlocked.Increment(ref score),
								waitAndPostset));
						}
					}
					else
					{
						for (int i = 0; i < repeat; i++)
						{
							for (int j = 0; j < wis.Count; j++)
							{
								actor(
									wis[j],
									db,
									key,
									(double)Interlocked.Increment(ref score),
									waitAndPostset);
							}
						}
					}
				}
				stopwatch.Stop();
			}

			// 시간 측정 결과 출력
			PrintStatistics(title, repeat * wis.Count, stopwatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// String 성능 측정 드라이버
		/// </summary>
		/// <param name="enabled"></param>
		/// <param name="db"></param>
		/// <param name="title"></param>
		/// <param name="keyPrefix"></param>
		/// <param name="actor"></param>
		/// <param name="parallel"></param>
		/// <param name="waitAndPostset"></param>
		/// <param name="asynchronous"></param>
		/// <param name="repeat"></param>
		private static void TestPerformanceDriver(
			bool enabled,
			IDatabase db,
			string title,
			RedisKey keyPrefix,
			Action<Worker, IDatabase, RedisKey, RedisValue, bool, bool> actor,
			bool parallel,
			bool waitAndPostset,
			bool asynchronous,
			int repeat)
		{
			if (!enabled)
			{
				return;
			}

			// 시간 측정은 Stopwatch 활용
			Stopwatch stopwatch = new Stopwatch();

			// Actor 시간 측정
			if (actor != null)
			{
				stopwatch.Restart();
				{
					if (parallel)
					{
						for (int i = 0; i < repeat; i++)
						{
							Parallel.ForEach(ws, item => actor(
								item,
								db,
								$"{keyPrefix}{item.Name}",
								$"{item.Name}:{item.Info}",
								waitAndPostset,
								asynchronous));
						}
					}
					else
					{
						for (int i = 0; i < repeat; i++)
						{
							for (int j = 0; j < ws.Count; j++)
							{
								actor(
									ws[j],
									db,
									$"{keyPrefix}{ws[j].Name}",
									$"{ws[j].Name}:{ws[j].Info}",
									waitAndPostset,
									asynchronous);
							}
						}
					}
				}
				stopwatch.Stop();
			}

			// 시간 측정 결과 출력
			PrintStatistics(title, repeat * wis.Count, stopwatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// ListSetByIndexAsync 메서드를 사용해서 List 값 설정
		/// </summary>
		/// <param name="wi"></param>
		/// <param name="db"></param>
		/// <param name="key"></param>
		/// <param name="values"></param>
		/// <param name="waitAndPostset">
		///		<para>비동기 메서드의 실행이 끝날때까지 기다렸다가 로컬 변수에 새 값을 설정할 지 여부</para>
		///		<para>true이면 기다리고, false이면 로컬 변수의 값을 미리 설정한 후 비동기 메서드의 완료를 기다리지 않음</para>
		///	</param>
		private static void ListSetByIndexAsyncWithEach(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			// 기존 키가 있는지 확인
			if (db.KeyExists(key))
			{
				//
				// 기존 키가 있으면, 기존 아이템 값을 각각 설정
				//
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				var tran = db.CreateTransaction();
				tran.ListSetByIndexAsync(key, 1, values[1]).ConfigureAwait(false);
				tran.ListSetByIndexAsync(key, 2, values[2]).ConfigureAwait(false);
				tran.ListSetByIndexAsync(key, 3, values[3]).ConfigureAwait(false);
				Task<bool> task = tran.ExecuteAsync();

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result);
					if (task.Result)
					{
						wi.Longitude = values[1];
						wi.Latitude = values[2];
						wi.UpdateTime = values[3];
					}
				}
			}
			else
			{
				//
				// 기존 키가 없으면 리스트 아이템을 인덱스로 접근할 수 없고, 푸시해서 넣어야 함
				//
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				Task<long> task = db.ListRightPushAsync(key, values);

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result == values.Length);
					if (task.Result == values.Length)
					{
						wi.Longitude = values[1];
						wi.Latitude = values[2];
						wi.UpdateTime = values[3];
					}
				}
			}
		}

		private static void ListTrimAndRightPush(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			// 기존 키가 있는지 확인
			if (db.KeyExists(key))
			{
				//
				// 기존 키가 있으면 아이템을 삭제한 후 삽입
				//
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				var tran = db.CreateTransaction();
				tran.ListTrimAsync(key, -1, 0);
				tran.ListRightPushAsync(key, values);
				Task<bool> task = tran.ExecuteAsync();

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result);
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}
			}
			else
			{
				//
				// 기존 키가 없으면 모든 아이템을 삽입
				//
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				Task<long> task = db.ListRightPushAsync(key, values);

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result == values.Length);
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}
			}
		}

		private static void ListDeleteKeyAndRightPush(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			// 기존 키가 있는지 확인
			if (db.KeyExists(key))
			{
				// 기존 키가 있으면 키를 날려버리고 아이템 전체를 푸시
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				var tran = db.CreateTransaction();
				tran.KeyDeleteAsync(key);
				tran.ListRightPushAsync(key, values);
				Task<bool> task = tran.ExecuteAsync();

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result);
					if (task.Result)
					{
						wi.Longitude = values[1];
						wi.Latitude = values[2];
						wi.UpdateTime = values[3];
					}
				}
			}
			else
			{
				// 키가 없으면 바로 푸시 시전
				if (!waitAndPostset)
				{
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}

				Task<long> task = db.ListRightPushAsync(key, values);

				if (waitAndPostset)
				{
					task.Wait();
					Debug.Assert(task.Result == values.Length);
					wi.Longitude = values[1];
					wi.Latitude = values[2];
					wi.UpdateTime = values[3];
				}
			}
		}

		private static void HashSet(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			if (!waitAndPostset)
			{
				SetWorkerInfo(wi, values);
			}

			HashEntry[] entries = CreateHashEntries(values);
			db.HashSet(key, entries);

			if (waitAndPostset)
			{
				SetWorkerInfo(wi, values);
			}
		}

		private static void HashSetAsync(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			if (!waitAndPostset)
			{
				SetWorkerInfo(wi, values);
			}

			HashEntry[] entries = CreateHashEntries(values);
			Task task = db.HashSetAsync(key, entries);

			if (waitAndPostset)
			{
				task.Wait();
				SetWorkerInfo(wi, values);
			}
		}

		private static HashEntry[] CreateHashEntries(RedisValue[] values)
		{
			return new HashEntry[]
			{
				new HashEntry("Name", values[0]),
				new HashEntry("Longitude", values[1]),
				new HashEntry("Latitude", values[2]),
				new HashEntry("UpdateTime", values[3])
			};
		}

		private static void SetWorkerInfo(WorkerInfo wi, RedisValue[] values)
		{
			wi.Name = values[0];
			wi.Longitude = values[1];
			wi.Latitude = values[2];
			wi.UpdateTime = values[3];
		}

		private static readonly RedisValue[] WorkerInfoFields = new RedisValue[]
		{
			"Name", "Longitude", "Latitude", "UpdateTime"
		};

		private static void HashGet(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			db.HashGet(key, WorkerInfoFields);
		}

		private static void HashGetAsync(WorkerInfo wi, IDatabase db, RedisKey key, RedisValue[] values, bool waitAndPostset)
		{
			Task<RedisValue[]> task = db.HashGetAsync(key, WorkerInfoFields);

			if (waitAndPostset)
			{
				task.Wait();

				/*
				Debug.Assert(
					task.Result?.Length == 4 &&
					string.Compare(wi.Name, task.Result[0]) == 0 &&
					string.Compare(wi.Longitude, task.Result[1]) == 0 &&
					string.Compare(wi.Latitude, task.Result[2]) == 0 &&
					string.Compare(wi.UpdateTime, task.Result[3]) == 0);
				*/
			}
		}

		private static void SortedSetAddAsync(WorkerInfo wi, IDatabase db, RedisKey key, double score, bool waitAndPostset)
		{
			Task<bool> task = db.SortedSetAddAsync(key, wi.Name, score);

			if (waitAndPostset)
			{
				task.Wait();
			}
		}

		private static void StringSet(Worker wi, IDatabase db, RedisKey key, RedisValue value, bool waitAndPostset, bool asynchronous)
		{
			if (!waitAndPostset)
			{
				wi.Info = value;
			}

			if (asynchronous)
			{
				Task<bool> task = db.StringSetAsync(key, value);
				if (waitAndPostset)
				{
					task.Wait();
					wi.Info = value;
				}
			}
			else
			{
				db.StringSet(key, value);
				if (waitAndPostset)
				{
					wi.Info = value;
				}
			}
		}

		private static float GetRandomNumber(Random rnd, float minimum, float maximum)
		{
			return (float)(rnd.NextDouble() * (maximum - minimum)) + minimum;
		}

		private static string ToLocationString(float location)
		{
			return $"{location:0.0000}";
		}

		private static string ToUpdateTimeString(DateTime updateTime)
		{
			return $"{updateTime:yyyyMMddHHmmss}";
		}

		private static string IncreaseLocationString(string locationByFloat, float diff)
		{
			if (float.TryParse(locationByFloat, out float location))
			{
				location += diff;
				return ToLocationString(location);
			}
			return locationByFloat;
		}

		private static void PrintStatistics(string title, int count, long elapsedMilliseconds)
		{
			Console.WriteLine($"{title,-36} Count:{count,8:N0}, ElapsedMilliseconds:{elapsedMilliseconds,8:N0}ms");
		}
	}

	public sealed class RedisServer
	{
		public static string RedisServerEndPoint { get; set; } = "192.168.0.46:16379";
		public static string RedisServerPassword { get; set; } = "Ekswnr59";
		public static string GetConnectionString()
		{
			return $"{RedisServerEndPoint},allowAdmin=true,password={RedisServerPassword}";
		}
	}

	public class WorkerInfo
	{
		public string Name { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string UpdateTime { get; set; }
		public string IsOnline { get; set; }
	}

	public class Worker
	{
		public string Name { get; set; }
		public string Info { get; set; }
	}
}
