using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelegateSample
{
	delegate int Compare(int a, int b);

	delegate int Operate(int a, int b);

	class Comparer
	{
		public static int AscendCompare(int a, int b)
		{
			if (a > b)
			{
				return 1;
			}
			else if (a == b)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}

		public static int DescendCompare(int a, int b)
		{
			if (a < b)
			{
				return 1;
			}
			else if (a == b)
			{
				return 0;
			}
			else
			{
				return -1;
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			List<int> values = new List<int> { 3, 6, 2, 9, 5, 4, 1, 8, 7 };
			BubbleSort(values, Comparer.AscendCompare);
			foreach (var v in values)
			{
				Console.Write(v);
				Console.Write(' ');
			}
			Console.WriteLine();

			Console.WriteLine($"1.CompareTo(2) = {1.CompareTo(2)}");

			Operate opPlus = (int a, int b) => { return a + b; };
			Console.WriteLine($"opPlus(1, 3) = {opPlus(1, 3)}");
		}

		static void BubbleSort(List<int> values, Compare comparer)
		{
			for (int i = 0; i < values.Count - 1; i++)
			{
				for (int j = 0; j < values.Count - 1 - i; j++)
				{
					if (values[j] > values[j + 1])
					{
						int temp = values[j];
						values[j] = values[j + 1];
						values[j + 1] = temp;
					}
				}
			}
		}
	}
}
