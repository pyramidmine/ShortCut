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
			TestByteOrder(false);
			TestKeyValueString(false);
			TestStringJoin(false);
			TestBitConverter(false);
			TestUtf8StringCodec(true);
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
			Console.WriteLine($"String (HH, fff): {dt:yyyy-MM-dd HH:mm:ss.fff}");
			// 출력: hh는 12시간제. FFF는 뒤 0을 표시하지 않음
			Console.WriteLine($"String (hh, FFF): {dt:yyyy-MM-dd hh:mm:ss.FFF}");
		}

		private static void TestByteOrder(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			const int pageNum = 11;

			// 데이터를 바이트 배열로 변환
			byte[] pageNumBytes = BitConverter.GetBytes(pageNum);
			// 변환된 바이트 배열 출력
			Console.WriteLine($"Byte order of value {pageNum}: {BitConverter.ToString(pageNumBytes)}");
		}

		private static void TestKeyValueString(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			string key;
			string value;
			StringBuilder sb = new StringBuilder();

			// 키/값이 모두 있을 때
			{
				key = "CALLTYPE";
				value = "SLG1";
				AppendKeyValue(ref sb, key, value);
				Console.WriteLine($"Key: {key}, Value: {value}, Result: {sb}");
			}

			// 키가 없을 때
			{
				sb = new StringBuilder();
				key = "";
				value = "DEF1";
				AppendKeyValue(ref sb, key, value);
				Console.WriteLine($"Key: {key}, Value: {value}, Result: {sb}");
			}

			// 값이 없을 때
			{
				sb = new StringBuilder();
				key = "CALLSEQ";
				value = "";
				AppendKeyValue(ref sb, key, value);
				Console.WriteLine($"Key: {key}, Value: {value}, Result: {sb}");
			}

			// 값이 없지만 강제 쓰기 모드일 때
			{
				sb = new StringBuilder();
				key = "CALLSEQ";
				value = "";
				const bool forceWrite = true;
				AppendKeyValue(ref sb, key, value, forceWrite);
				Console.WriteLine($"Key: {key}, Value: {value}, ForceWrite: {forceWrite}, Result: {sb}");
			}
		}
		private static void AppendKeyValue(ref StringBuilder sb, string key, string value, bool forceWrite = false)
		{
			// 키가 없거나,
			// 값이 없으면서 강제 쓰기 모드가 아니면 그냥 리턴
			if ((string.IsNullOrEmpty(key)) ||
				(string.IsNullOrEmpty(value) && !forceWrite))
			{
				return;
			}

			sb.Append("\"").Append(key).Append("\":\"").Append(value).Append("\"");
		}

		private static void TestStringJoin(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			List<string> values = new List<string>();

			// 빈 리스트에 대해서 Join 시도
			{
				// 아무 일도 없음
				Console.WriteLine($"Join for empty list: {string.Join(",", values)}");
			}

			// 리스트 아이템이 1개만 있을 경우
			{
				values.Add("One");
				Console.WriteLine($"Join for one item list: {string.Join(",", values)}");
			}

			// 리스트 아이템이 2개 있을 경우
			{
				values.Add("Two");
				Console.WriteLine($"Join for two items list: {string.Join(",", values)}");
			}
		}

		private static void TestBitConverter(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			// bool을 byte array로 변환
			{
				const bool isLastByte = true;
				byte[] boolBytes = BitConverter.GetBytes(isLastByte);
				Console.WriteLine($"Byte array for bool, Length:{boolBytes.Length}");
			}

			// int를 byte array로 변환해서 출력
			{
				const int pageNum = 1;
				byte[] pageNumBytes = BitConverter.GetBytes(pageNum);
				Console.WriteLine($"Byte array for int, Length:{pageNumBytes.Length}, Array:{Encoding.Default.GetString(pageNumBytes)}");
			}
		}

		private static void TestUtf8StringCodec(bool enabled)
		{
			if (!enabled)
			{
				return;
			}

			// 문자 1개인 경우
			{
				string isLastPage = "1";
				byte[] encodedIsLastPage = Encoding.UTF8.GetBytes(isLastPage);
				string decodedIsLastPage = Encoding.UTF8.GetString(encodedIsLastPage);
				Console.WriteLine("isLastPage string, Original:{0}, Encoded:{1}, Decoded:{2}",
					isLastPage,
					BitConverter.ToString(encodedIsLastPage),
					decodedIsLastPage);
			}

			// 정수를 자릿수에 맞춰서 포맷
			{
				int pageNum = 1;
				Console.WriteLine($"pageNum int, Original:{pageNum}, String:{pageNum.ToString("D4")}");
			}

			// 문자 4개인 경우
			{
				string pageNum = "0001";
				byte[] encodedIsLastPage = Encoding.UTF8.GetBytes(pageNum);
				string decodedIsLastPage = Encoding.UTF8.GetString(encodedIsLastPage);
				Console.WriteLine("pageNum string, Original:{0}, Encoded:{1}, Decoded:{2}",
					pageNum,
					BitConverter.ToString(encodedIsLastPage),
					decodedIsLastPage);
			}
		}
	}
}
