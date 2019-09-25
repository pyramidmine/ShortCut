using System;
using System.Collections.Generic;

namespace GenericCollections
{
	class Base { }

	class Derived : Base { }

	class Logic<T> where T : Base
	{
		List<T> items = new List<T>();

		public void Add(T item)
		{
			items.Add(item);
		}

		public void PrintAll()
		{
			foreach (var v in items)
			{
				Console.WriteLine(v.ToString());
			}
		}
	}
}