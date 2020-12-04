using System;
using System.Linq;
using System.Text;

namespace BlockCopyTest
{
	class Program
	{
		static void Main(string[] args)
		{
			byte[] src = Encoding.UTF8.GetBytes("Hello");
			int srcOffset = 0;
			byte[] dst = new byte[src.Length];
			int dstOffset = 0;
			Buffer.BlockCopy(src, srcOffset, dst, dstOffset, 0);
			if (src.SequenceEqual(dst))
			{
				Console.WriteLine("Equal!");
			}
		}
	}
}
