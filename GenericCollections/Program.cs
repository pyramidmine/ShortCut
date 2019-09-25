using System;
using System.Collections.Generic;

namespace GenericCollections
{
	class Program
    {
        static void Main(string[] args)
        {
			Logic<Base> logic = new Logic<Base>();
			logic.Add(new Base());
			logic.Add(new Derived());
			logic.PrintAll();

			Zoo<Animal> zoo = new Zoo<Animal>();
			zoo.Add(new Animal() { Name = "Tiger" });
			zoo.Add(new Animal() { Name = "Lion" });
			foreach (var v in zoo)
			{
				Console.WriteLine(v.Name);
			}
        }
    }
}
