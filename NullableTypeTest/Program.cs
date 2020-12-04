using System;

namespace NullableTypeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            int? a = null;
            if (a != null)
            {
                Console.WriteLine(a);
            }
            a = 100;
            Console.WriteLine(a);
            int b = a.Value;
            Console.WriteLine(b);
        }
    }
}
