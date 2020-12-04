using System;

namespace ExtensionMethodOfProperty
{
	internal static class Program
	{
		private static void Main()
		{
			Person person = new Person { Age = 50 };
			person.Age.Print();

			byte[] src = new byte[1];
			int srcOffset = 0;
			person.Age.Read(src, ref srcOffset);
			person.Age.Print();
		}
	}

	internal class Person
	{
		public int Age { get; set; }
	}

	internal static class PersonExtension
	{
		public static void Print(this int value)
		{
			Console.WriteLine(value);
		}

		public static int Read(this int value, byte[] src, ref int srcOffset)
		{
			return value;
		}
	}
}
