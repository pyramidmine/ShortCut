using GenericReadWriteLibrary;
using System;
using System.Collections.Generic;
using Xunit;

namespace GenericReadWriteLibraryUnitTests
{
	public class DataConverterUnitTest
	{
		private static readonly List<(byte Value, int RequiredBytes)> ByteTestDatas = new List<(byte, int)>
		{
			(0, 1),
			(1, 1),
			(0b_01011001, 1),
			(0b_10000000, 2),
			(0b_11111111, 2),
		};

		private delegate void WriteAction<T>(byte[] dst, ref int dstOffset, T value);
		private delegate T ReadAction<T>(byte[] src, ref int srcOffset);

		[Fact]
		public void TestReadWrite()
		{
			TestTypedReadWrite<byte>(
				ByteTestDatas,
				DataConverter.GetRequiredBytes,
				DataConverter.WriteByte,
				DataConverter.ReadByte);
		}

		private void TestTypedReadWrite<T>(
			List<(T, int)> TestDatas,
			Func<T, int> calculator,
			WriteAction<T> writer,
			ReadAction<T> reader)
		{
			// 필요한 전체 바이트 계산
			int length = 0;
			foreach (var (Value, RequiredBytes) in TestDatas)
			{
				int expected = RequiredBytes;
				int actual = calculator((T)Value);
				Assert.Equal(expected, actual);
				length += actual;
			}

			// 쓰기
			byte[] dst = new byte[length];
			int dstOffset = 0;
			foreach (var (Value, _) in TestDatas)
			{
				writer(dst, ref dstOffset, (T)Value);
			}

			// 읽기
			dstOffset = 0;
			foreach (var (Value, _) in TestDatas)
			{
				T expected = reader(dst, ref dstOffset);
				T actual = (T)Value;
				Assert.Equal(expected, actual);
			}
		}
	}
}
