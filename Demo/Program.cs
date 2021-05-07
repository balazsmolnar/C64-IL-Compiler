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

            var sprite = C64.Sprites.Sprite1;
            sprite.DataBlock = C64Address.FromLabel("spt_player_right_0");
            sprite.MultiColor = true;
            sprite.Visible = true;
            sprite.Color = Colors.Green;

            sprite.X = 300UL;
            sprite.Y = 100;

            for (ulong x = 0UL; x < 320UL; x++)
            {
                sprite.X = x;
                Delay.Wait(10);
            }

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
