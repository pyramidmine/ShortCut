using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqTest
{
	class Program
	{
		static void Main(string[] args)
		{
			long[] readRequests = new long[5]{ 1, 2, 3, 4, 5};
			PrintCollection(readRequests);
			Array.Clear(readRequests, 0, readRequests.Length);
			PrintCollection(readRequests);

			List<Tuple<int, int>> weights = new List<Tuple<int, int>>();
			weights.Add(new Tuple<int, int>(0, 20));
			weights.Add(new Tuple<int, int>(1, 40));
			weights.Add(new Tuple<int, int>(2, 80));
			Random rand = new Random();
			for (int i = 0; i < 10; i++)
			{
				int randomNumber = rand.Next(weights.Last().Item2);
				int index = weights.Find(x => randomNumber < x.Item2).Item1;
				Console.WriteLine($"randomNumber:{randomNumber}, index:{index}");
			}
		}

		static void PrintCollection(long[] coll)
		{
			foreach (var v in coll)
			{
				Console.WriteLine(v);
			}
		}
	}
}
