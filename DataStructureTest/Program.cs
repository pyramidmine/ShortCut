using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataStructureTest
{
	internal static class Program
	{
		private static void Main()
		{
			TestHashSetDuplicatedInitialization(false);
			TestStringListExtraction(true);
		}

		/// <summary>
		/// HashSet 초기화할 때 중복된 값을 넣어주면 어떻게 되는 지 테스트
		/// </summary>
		private static void TestHashSetDuplicatedInitialization(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			try
			{
				// 중복 없는 HashSet 생성
				{
					HashSet<string> names = new HashSet<string> { "One", "Two" };
					Console.WriteLine("====== HashSet Test: Not duplicated ======");
					foreach (var name in names)
					{
						Console.WriteLine(name);
					}
				}

				// 중복 된 키를 갖는 HashSet 생성
				{
					// 중복되더라도 익셉션이 발생하지 않음!
					HashSet<string> names = new HashSet<string> { "One", "One" };
					Console.WriteLine("====== HashSet Test: Duplicated ======");
					foreach (var name in names)
					{
						Console.WriteLine(name);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"EXCEPTION, Location={MethodBase.GetCurrentMethod()}, Type={ex.GetType()}, Message={ex.Message}");
			}

		}

		private static void TestStringListExtraction(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			List<string> names = new List<string> { "ROW1", "ROW2", "ROW3", "ROW4" };

			// ROW1 찾는 경우 나머지는 ROW2, ROW3, ROW4
			{
				string nameToFind = "ROW1";
				List<string> remains = names.Where(name => string.Compare(name, nameToFind) != 0).ToList();
				Console.WriteLine($"Name to find: {nameToFind}");
				Console.WriteLine($"Remains: {string.Join(',', remains.ToArray())}");
			}

			// ROW2 찾는 경우 나머지는 ROW1, ROW3, ROW4
			{
				string nameToFind = "ROW2";
				List<string> remains = names.Where(name => string.Compare(name, nameToFind) != 0).ToList();
				Console.WriteLine($"Name to find: {nameToFind}");
				Console.WriteLine($"Remains: {string.Join(',', remains.ToArray())}");
			}

			// ROW3 찾는 경우 나머지는 ROW1, ROW2, ROW4
			{
				string nameToFind = "ROW3";
				List<string> remains = names.Where(name => string.Compare(name, nameToFind) != 0).ToList();
				Console.WriteLine($"Name to find: {nameToFind}");
				Console.WriteLine($"Remains: {string.Join(',', remains.ToArray())}");
			}

			// ROW4 찾는 경우 나머지는 ROW1, ROW2, ROW3
			{
				string nameToFind = "ROW4";
				List<string> remains = names.Where(name => string.Compare(name, nameToFind) != 0).ToList();
				Console.WriteLine($"Name to find: {nameToFind}");
				Console.WriteLine($"Remains: {string.Join(',', remains.ToArray())}");
			}
		}
	}
}
