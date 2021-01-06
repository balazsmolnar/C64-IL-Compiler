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
            ball1.Sprite = C64.Sprites.Sprite0;
            ball1.VX = 1;
            ball1.VY = 1;
            ball1.Y = 70;
            ball1.BallColor = Colors.Red;

            var ball2 = new Ball();
            ball2.Sprite = C64.Sprites.Sprite1;
            ball2.VX = 1;
            ball2.VY = 2;
            ball2.X = 100;
            ball2.BallColor = Colors.Yellow;

            var ball3 = new Ball();
            ball3.Sprite = C64.Sprites.Sprite2;
            ball3.VX = 2;
            ball3.VY = 1;
            ball2.X = 120;
            ball2.BallColor = Colors.Violet;

            for (; ; )
            {
                ball1.Move();
                ball2.Move();
                ball3.Move();

                for (uint i = 0; i < 255; i++) ;
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
