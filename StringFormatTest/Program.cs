using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringFormatTest
{
	class Program
	{
		static void Main(string[] args)
		{
			StringBuilder sb = new StringBuilder(1024);
			{
				int i = 0;
				int readRequest = 1;
				int read = 2;
				int rowCount = 3;
				int bytes = 4;

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
		}
	}
}
