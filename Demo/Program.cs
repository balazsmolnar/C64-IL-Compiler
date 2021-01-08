using System;
using C64Lib;

namespace Demo
{
    class Program
    {
        static void ClearScreen()
        {
            for (int x = 0; x < 40; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    C64.SetChar(x, y, 32);
                }
            }
        }

        static int x;

        static void OnInterrupt(object sender, EventArgs args)
        {
            x = x + 1;
            C64.SetBorderColor((Colors)(x));
        }

        static void Main()
        {
            ClearScreen();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("hello world");
            }

            Ball.RunBalls();
        }
    }
}
