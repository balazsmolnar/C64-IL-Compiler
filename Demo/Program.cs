using System;

namespace Demo
{
    class Program
    {
        static void Hello()
        {
            Console.WriteLine("hello world from method");
        }
        static void Main(string[] args)
        {
            for (int i=0; i<10; i++) {
                for (int j=0; j<i; j++) {
                    Console.Write(" ");
                }
                
                Hello();
            }
        }
    }
}
