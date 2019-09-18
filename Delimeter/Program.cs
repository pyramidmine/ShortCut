using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delimeter
{
	class Program
	{
		static void Main(string[] args)
		{
			char verticalBar = (char)0x007C;
			char verticalBrokenBar = (char)0x00A6;

			StringBuilder sb = new StringBuilder();
			sb.Append(verticalBar);
			sb.Append(",");
			sb.Append(verticalBrokenBar);
			Console.WriteLine(sb.ToString());

			byte[] encodedVerticalBar = Encoding.UTF8.GetBytes(new char[] { verticalBar });
			byte[] encodedVerticalBrokenBar = Encoding.UTF8.GetBytes(new char[] { verticalBrokenBar });

			Console.WriteLine($"{verticalBar}, {encodedVerticalBar.Length}");
			Console.WriteLine($"{verticalBrokenBar}, {encodedVerticalBrokenBar.Length}");
		}
	}
}
