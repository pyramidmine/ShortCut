using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringFormatTest
{
	internal static class Program
	{
		private static void Main()
		{
			/*
			StringBuilder sb = new StringBuilder(1024);
			{
				const int i = 0;
				const int readRequest = 1;
				const int read = 2;
				const int rowCount = 3;
				const int bytes = 4;

				sb.AppendFormat("QR#{0}:{{{1},{2},{3:F0},{4}}}{5}",
					i,
					readRequest,
					read,
					rowCount,
					bytes / (double)Math.Max(1, rowCount),
					i < 5 ? "," : "");

				string formatted = sb.ToString();
				Console.WriteLine(formatted);
			}
			*/
			TestDateTimeFormat(false);
			TestByteOrder(true);
		}

		private static void TestDateTimeFormat(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			Console.WriteLine("====== TestDateTimeFormat ======");
			// 입력: 2021-01-30 17:38:51.890
			DateTime dt = new DateTime(year: 2021, month: 1, day: 30, hour: 17, minute: 38, second: 51, millisecond: 890);
			Console.WriteLine($"DateTime: {dt}");
			// 출력: HH는 24시간제. fff는 뒤 0을 표시
			Console.WriteLine($"String (HH, fff): {dt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
			// 출력: hh는 12시간제. FFF는 뒤 0을 표시하지 않음
			Console.WriteLine($"String (hh, FFF): {dt.ToString("yyyy-MM-dd hh:mm:ss.FFF")}");
		}

		private static void TestByteOrder(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			const int pageNum = 11;
			const int isLastPageTrue = 1;

			// 데이터를 바이트 배열로 변환
			byte[] pageNumBytes = BitConverter.GetBytes(pageNum);
			// 변환된 바이트 배열 출력
			Console.WriteLine($"Byte order of value {pageNum}: {BitConverter.ToString(pageNumBytes)}");
		}
	}
}
