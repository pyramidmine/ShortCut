using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniqueRandomNumberGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			// Generate unique random numbers
			List<int> numbers = Enumerable.Range(0, 1_000_000).OrderBy(x => Guid.NewGuid()).ToList();

			// Check duplication
			HashSet<int> uniqueNumbers = new HashSet<int>();
			foreach (var n in numbers)
			{
				if (uniqueNumbers.Contains(n))
				{
					Console.WriteLine($"{n} is duplicated.");
					break;
				}
				else
				{
					uniqueNumbers.Add(n);
				}
			}
			Console.WriteLine("All numbers are unique.");
		}
	}
}
