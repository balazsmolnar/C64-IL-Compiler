using System;
using C64Lib;
using Hunchback;

namespace Demo
{
    class Program
    {
        static void Foo(int y, ulong address, int x)
        {
            // Console.WriteLine(s);
        }

        static void Main()
        {
            var game = new Game();
            game.Start();

            // Ball.RunBalls();
            //Foo(0, C64Address.FromLabel("label1"), 0);
            // Foo("X");
            // Colors x = Colors.Black;

            // C64.Interrupt += (s, e) =>
            // {
            //     C64.SetBorderColor(++x);
            // };

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("hello world!");

            // GCTest.Start();
            // Game g = new Game();
            // g.Init();
            // g.Run();
        }
    }
}
