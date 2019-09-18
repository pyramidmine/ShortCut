using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyInitializationOnConstructor
{
    class Program
    {
        static void Main(string[] args)
        {
            PropertyTest pt = new PropertyTest() { Name = "MoonHwan Lee" };
            Console.WriteLine(pt.Name);
        }
    }

    class PropertyTest
    {
        public string Name { get; set; }

        public PropertyTest()
        {
        }
    }
}
