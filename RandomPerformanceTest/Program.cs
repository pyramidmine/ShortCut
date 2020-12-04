using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RandomPerformanceTest
{
	public static class Program
	{
		private static readonly (float South, float North) LatitudesOfSeoul = (37.4133F, 37.7151F);
		private static readonly (float West, float East) LongitudeOfSeoul = (126.7341F, 127.2693F);
		private const int NumOfWorkers = 10000;
		private const int NumOfRepeats = 1;
		private static readonly List<WorkerInfo> wis = new List<WorkerInfo>();
		private static readonly Random rnd = new Random();

		public static void Main()
		{
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
			}

			Stopwatch stopwatch = Stopwatch.StartNew();
			int count = 0;
			for (int loop = 0; loop < NumOfRepeats; loop++)
			{
				for (int i = 0; i < wis.Count; i++)
				{
					string[] values = new string[]
					{
						wis[i].Name,
						IncreaseLocationString(wis[i].Longitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
						IncreaseLocationString(wis[i].Latitude, GetRandomNumber(rnd, -0.001F, 0.001F)),
						ToUpdateTimeString(DateTime.Now)
					};

					wis[i].Longitude = values[1];
					wis[i].Latitude = values[2];
					wis[i].UpdateTime = values[3];

					count++;
				}
			}
			stopwatch.Stop();

			PrintStatistics("WorkerInfo: random modification", count, stopwatch.ElapsedMilliseconds);
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
			Console.WriteLine($"{title,-35} Count:{count,9:N0}, ElapsedMilliseconds:{elapsedMilliseconds,8:N0}ms");
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
}
