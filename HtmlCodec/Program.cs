using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlCodec
{
	class Program
	{
		static void Main(string[] args)
		{
			string encoded = "A &gt;= B";
			string decoded = System.Net.WebUtility.HtmlDecode(encoded);
			Console.WriteLine(decoded);
		}
	}
}
