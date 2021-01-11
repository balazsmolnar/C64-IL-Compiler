using System;
using C64Lib;

namespace Demo
{
    class Program
    {
        static void ClearScreen()
        {
            for (uint x = 0; x < 40; x++)
            {
                for (uint y = 0; y < 25; y++)
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

            var platform = new Platform()
            {
                X = 10,
                Y = 10,
                Color = Colors.Yellow,
                Width = 10
            };
            platform.Draw();

            Game g = new Game();
            g.Init();
            g.Run();
        }
    }
}
