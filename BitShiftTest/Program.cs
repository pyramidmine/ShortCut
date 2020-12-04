using System;

namespace BitShiftTest
{
	class Program
	{
		static void Main(string[] args)
		{
			int bytes = 0;
			bytes = GetRequiredBytes(0x8000000000000000);
			Console.WriteLine($"bytes = {bytes}");
		}

		static int GetRequiredBytes(ulong value)
		{
			int bytes = 0;
			do
			{
				value >>= 7;
				bytes++;
			} while (value != 0);

			return bytes;
		}
	}
}
