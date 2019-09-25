using System.Collections;
using System.Collections.Generic;

namespace GenericCollections
{
	class Animal
	{
		public string Name { get; set; }
	}

	class Zoo<T> : IEnumerable<T>
	{
		List<T> items = new List<T>();

		public void Add(T item)
		{
			items.Add(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return items.GetEnumerator();
		}
	}
}
