using System;
using C64Lib;

namespace Demo
{
    class A
    {

    }

    class Program
    {
        static Colors BallColor => Colors.Orange;
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
        static void Hello(string s)
        {
            Console.WriteLine(s);
        }

        static void WriteSpaces(int n)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write(" ");
            }
        }

        static void RunBall()
        {
            int x = 0;
            int y = 0;
            int vx = 1;
            int vy = 1;
            const int circle = 81;
            const int WIDTH = 39;
            const int HEIGHT = 24;

            var oldChar = C64.GetChar(x, y);
            for (; ; )
            {
                C64.SetChar(x, y, oldChar, BallColor);
                x += vx;
                y += vy;
                oldChar = C64.GetChar(x, y);
                if (x == WIDTH)
                    vx = -1;
                if (x == 0)
                    vx = 1;
                if (y == HEIGHT)
                    vy = -1;
                if (y == 0)
                    vy = 1;

                C64.SetChar(x, y, circle, BallColor);

                for (var ii = 0; ii < 100; ii++) ;
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
            x = 0;
            ClearScreen();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("hello world");
            }
            RunBall();
        }
    }
}
