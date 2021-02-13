using System;
using C64Lib;

namespace Demo
{
    class Program
    {

        static void Recursive(uint i)
        {
            C64.SetChar(i, 0, i);

            if (i == 10)
                return;
            Recursive(i + 1);
        }

        static void Test(int d, int x)
        {
            int b = 1;
            int y = x;
            b = d;
            if (d == 2)
                C64.SetBorderColor(Colors.Cyan);
        }

        static void Main()
        {
            // Colors x = Colors.Black;

            // C64.Interrupt += (s, e) =>
            // {
            //     C64.SetBorderColor(x++);
            // };

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("HELLO");

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("HELLOWORLD");
            // GCTest.Start();
            Game g = new Game();
            g.Init();
            g.Run();
        }
    }
}
