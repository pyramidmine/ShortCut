using MannaPlanet.MannaLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisStressTest
{
	public static class Program
	{
		private static readonly Log logger = new LogBuilder().Build();
		private static long redisTransactionCount = 0;

		public static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Usage: RedisStressTest endpoint password user-count");
				return;
			}

			if (!int.TryParse(args[2], out int userCount))
			{
				logger.Error($"ERROR: user-count is not a number: {args[2]}");
				return;
			}

			//
			// 배송원 정보 키
			// - 필드 이름이 배송원코드, 필드 값이 배송원 정보 스트링
			//
			RedisKey workerInfoKey = "worker-info";
			//
			// 배송원 정보 업데이트 키
			// - 멤버 이름이 배송원코드, 스코어가 업데이트시간
			//
			RedisKey workerUpdateKey = "worker-update";
			CancellationTokenSource cts = new CancellationTokenSource();
			Random rnd = new Random();

			//
			// 프로그램 실행 옵션에 넣어 준 user-count 만큼 만들어서 사용
			//
			List<WorkerInfo> workers = new List<WorkerInfo>(userCount);
			{
				var workerCodes = Enumerable.Range(1, userCount).ToArray();
				foreach (var workerCode in workerCodes)
				{
					workers.Add(new WorkerInfo(rnd, workerCode));
				}
			}

			try
			{
				logger.Info($"Redis Server: {args[0]}, connecting...");
				ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{args[0]},allowAdmin=true,password={args[1]},ConnectTimeout=1000");
				logger.Info($"Redis Server: {args[0]}, connected.");

				IDatabase db = redis.GetDatabase();

				//
				// 스탯 출력 쓰레드 시작
				//
				Timer statTimer = new Timer(PrintStatisticsCallback, null, 1000, 1000);

				//
				// 'c' 키를 눌러서 작업 종료할 수 있는 쓰레드 시작
				//
				Task.Run(() =>
				{
					if (Console.ReadKey().KeyChar == 'c')
					{
						cts.Cancel();
						statTimer.Dispose();
					}
				});

				//
				// SortedSet 업데이트 쓰레드 시작
				//
				ParallelOptions po = new ParallelOptions { CancellationToken = cts.Token };
				try
				{
					while (!cts.IsCancellationRequested)
					{
						Parallel.ForEach(workers, po, (worker) =>
						{
							// 위치와 시간을 랜덤하게 변경
							worker.Randomize(rnd);

							// 배송원 정보와 배송원 정보 업데이트 시간을 하나의 트랜잭션에서 처리
							RedisValue member = worker.Code;
							double score = worker.UpdateTime;
							var tran = db.CreateTransaction();
							tran.HashSetAsync(workerInfoKey, member, worker.ToString());
							tran.SortedSetAddAsync(workerUpdateKey, member, score);
							tran.Execute();

							// 모니터링 용도의 카운트 증가
							Interlocked.Increment(ref redisTransactionCount);

							// 쓰레드 취소 처리
							// : 예외 발생 -> catch
							po.CancellationToken.ThrowIfCancellationRequested();
						});
					}
				}
				catch (OperationCanceledException ex)
				{
					logger.Info($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
				}
				finally
				{
					cts.Dispose();
				}
			}
			catch (Exception ex)
			{
				logger.Error($"EXCEPTION: {ex.GetType().Name}, {ex.Message}");
			}

			Console.Read();
		}

		private static void PrintStatisticsCallback(object state)
		{
			logger.Info($"Transactions: Count={Interlocked.Exchange(ref redisTransactionCount, 0)}");
		}
	}

	/// <summary>
	/// 배송원 정보
	/// </summary>
	/// <remarks>
	/// - 배송원 정보를 레디스에 넣는 방법은 여러 가지지만, 배송원 정보 전체를 변경하는가 아니면 일부 필드만 변경하는가에 따라 크게 나눠짐
	/// - 필드를 따로 조회하면 최선의 경우에도 O(N) 시간이 걸리므로 시간적으로 손해
	/// - 필드를 합쳐서 조회하면 O(1) 시간에 처리 가능한 게 장점이지만, 필드를 파싱하는 건 다소 불편
	/// - 필드를 합쳐서 조회할 때, 해시 방식(단일 키를 사용하고 필드는 배송원코드)이 스트링 방식(배송원마다 키 부여)보다 사용에 유리할 듯
	/// </remarks>
	public class WorkerInfo
	{
		public string Code { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public double UpdateTime { get; set; }
		public bool IsOnline { get; set; }
		// 서울의 대략적인 위도/경도
		private static readonly (double West, double WestToEast) LongitudeOfSeoul = (126.7341, 127.2693 - 126.7341);
		private static readonly (double South, double SouthToNorth) LatitudesOfSeoul = (37.4133, 37.7151 - 37.4133);
		public WorkerInfo(Random rnd, int workerCode)
		{
			Code = $"W{workerCode:D5}";
			// 위치는 서울 위도/경도 범위 안에서 랜덤하게 설정
			Longitude = GetRandomDouble(rnd, LongitudeOfSeoul.West, LongitudeOfSeoul.WestToEast);
			Latitude = GetRandomDouble(rnd, LatitudesOfSeoul.South, LatitudesOfSeoul.SouthToNorth);
			UpdateTime = NowToDouble();
		}
		public void Randomize(Random rnd)
		{
			// 위치를 랜덤하게 변경: GPS 좌표의 소수점 이하 4번째 자리가 미터 단위이므로 이 부분을 변경
			Longitude += GetRandomDouble(rnd, -0.001, 0.002);
			Latitude += GetRandomDouble(rnd, -0.001, 0.002);
			UpdateTime = NowToDouble();
		}
		public override string ToString()
		{
			return new StringBuilder().AppendFormat($"{Longitude:0.0000}:{Latitude:0.0000}:{UpdateTime:0}:{(IsOnline?'Y':'N')}").ToString();
		}
		/// <summary>
		/// 특정 범위 안에서 임의의 실수값 리턴. 예를 들어, min = -0.001, diff = 0.002 경우, -0.001 ~ 0.001 사이의 임의의 값 생성
		/// </summary>
		/// <param name="rnd">Random 객체</param>
		/// <param name="min">최소 값</param>
		/// <param name="diff">범위</param>
		/// <returns></returns>
		private double GetRandomDouble(Random rnd, double min, double diff)
		{
			return (double)(rnd.NextDouble() * diff) + min;
		}
		/// <summary>
		/// 시간을 double 형태로 변환
		/// </summary>
		/// <returns>ddHHmmss 형태. 날짜+시간+분+초</returns>
		private double NowToDouble()
		{
			return double.Parse($"{DateTime.Now:ddHHmmss}");
		}
	}
}
