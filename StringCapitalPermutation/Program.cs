using System;
using System.Collections.Generic;
using System.Linq;

namespace StringCapitalPermutation
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Input string to permutation: ");
			string input = Console.ReadLine();
			Console.WriteLine($"Input string: {input}");
			List<string> permutations = GetCapitalPermutations(input);
			permutations.ForEach(p => Console.WriteLine(p));
		}

		private static List<string> GetCapitalPermutations(string input)
		{
			List<string> permutations = new List<string>{ string.Empty };

			foreach (var ch in input)
			{
				List<string> nextGenerations = new List<string>();
				permutations.ForEach(p =>
				{
					if (char.IsLetter(ch))
					{
						nextGenerations.Add(p + char.ToLower(ch));
						nextGenerations.Add(p + char.ToUpper(ch));
					}
					else
					{
						nextGenerations.Add(p + ch);
					}
				});
				permutations = nextGenerations;
			}

			return permutations;
		}
	}
}
