using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace MultiplicationTable
{
	class Program
	{
		static void Main(string[] args)
		{
			Stopwatch stopwatch = new Stopwatch();

			stopwatch.Restart();
			for (int i = 0; i < 100; i++)
			{
				PrintMultiplicationTableWithDoubleLoop();
			}
			stopwatch.Stop();
			long doubleLoopTicks = stopwatch.ElapsedTicks;

			stopwatch.Restart();
			for (int i = 0; i < 100; i++)
			{
				PrintMultiplicationTableWithStaticData();
			}
			stopwatch.Stop();
			long staticDataTicks = stopwatch.ElapsedTicks;

			Console.WriteLine($"Double Loop Method: ElapsedTicks={doubleLoopTicks}");
			Console.WriteLine($"Static Data Method: ElapsedTicks={staticDataTicks}");
		}

		static void PrintMultiplicationTableWithDoubleLoop()
		{
			for (int i = 1; i < 10; i++)
			{
				for (int j = 1; j < 10; j++)
				{
					Console.WriteLine($"{i}*{j}={i * j}");
				}
			}
		}

		private static readonly List<string> MultiplicationTexts = new List<string>();

		static Program()
		{
			for (int i = 1; i < 10; i++)
			{
				for (int j = 1; j < 10; j++)
				{
					MultiplicationTexts.Add($"{i}*{j}={i * j}");
				}
			}
		}

		static void PrintMultiplicationTableWithStaticData()
		{
			foreach (var text in MultiplicationTexts)
			{
				Console.WriteLine(text);
			}
		}
	}
}
