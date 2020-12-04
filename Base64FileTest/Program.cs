using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base64FileTest
{
	class Program
	{
		private static readonly string SAMPLE_DATA_FILENAME = "SampleData.dat";

		static void Main(string[] args)
		{
			string sampleText = "Sample Text";
			byte[] sampleData = Encoding.UTF8.GetBytes(sampleText);

			// 샘플 데이터를 Base64 인코딩으로 파일에 저장
			File.WriteAllBytes(SAMPLE_DATA_FILENAME, Convert.To)
		}
	}
}
