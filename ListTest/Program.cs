using System;
using System.Collections.Generic;

namespace ListTest
{
	internal static class Program
	{
		private static void Main()
		{
			//
			// 리스트 A를 B에 Add 한 후 삭제하면 B에 Add 된 리스트는 어떻게 되나?
			//
			List<string> source = new List<string>
			{
				"Hello",
				"World"
			};
			List<List<string>> target = new List<List<string>>();

			target.Add(source);
			PrintStringLists(target);

			source.Add("!");
			PrintStringLists(target);

			source = new List<string>();
			source.Add("Welcome");
			PrintStringLists(target);

			AddPage(target, ref source);
			PrintStringLists(target);

			AddPage(target, ref source);
			PrintStringLists(target);
		}

		private static void PrintStringLists(List<List<string>> stringLists)
		{
			Console.WriteLine("====== PrintStringLists ======");
			foreach (var strs in stringLists)
			{
				foreach (var str in strs)
				{
					Console.WriteLine(str);
				}
			}
		}

		/// <summary>
		/// 파라미터로 넣어준 객체의 값을 바꿔주는 테스트
		/// </summary>
		/// <param name="pages"></param>
		/// <param name="records"></param>
		/// <remarks>
		/// - 일반 파라미터의 경우: records 값을 리셋해도 파라미터 변수 records가 리셋되는 거지 원본 객체가 리셋되는 게 아님
		/// - 레퍼런스 파라미터의 경우: records 파라미터가 원본을 가리키므로 records에 새 값을 할당하는 순간 원본도 새 값을 가리키게 됨
		/// </remarks>
		private static void AddPage(List<List<string>> pages, ref List<string> records)
		{
			pages.Add(records);
			records = new List<string>();
		}
	}
}
