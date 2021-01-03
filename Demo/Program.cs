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

        static void RunBall()
        {
            var ball1 = new Ball();
            ball1.VX = 1;
            ball1.VY = 1;
            ball1.BallColor = Colors.Red;

            var ball2 = new Ball();
            ball2.VX = 2;
            ball2.VY = 1;
            ball2.BallColor = Colors.White;

            var ball3 = new Ball();
            ball3.VX = 1;
            ball3.VY = 2;
            ball1.BallColor = Colors.Yellow;

            for (; ; )
            {
                ball1.Clear();
                ball2.Clear();
                ball3.Clear();
                ball1.Set();
                ball2.Set();
                ball3.Set();
                for (var ii = 0; ii < 100; ii++)
                    for (var jj=0; jj<3; jj++) ;
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
            RunBall();
        }
    }
}
