using System;
using C64Lib;

namespace Demo
{
    class Program
    {
        static void Foo(string s)
        {
            Console.WriteLine(s);
        }
        static void Main()
        {
            // Foo("X");
            // Colors x = Colors.Black;

            // C64.Interrupt += (s, e) =>
            // {
            //     C64.SetBorderColor(x++);
            // };

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("HELLO");

            //for (int i = 0; i < 10; i++)
            //    Console.WriteLine("HELLO WORLD!");
            // GCTest.Start();
            Game g = new Game();
            g.Init();
            g.Run();
        }
    }
}
