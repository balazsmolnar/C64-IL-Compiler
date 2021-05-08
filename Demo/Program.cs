using System;
using C64Lib;
using Hunchback;

namespace Demo
{
    class Program
    {

        static void Main()
        {
            var game = new Game();
            game.Start();

            // Colors x = Colors.Black;
            // C64.Interrupt += (s, e) =>
            // {
            //     C64.SetBorderColor(++x);
            // };

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("hello world!");
        }
    }
}
