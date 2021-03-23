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

        static void GetReady()
        {
            C64.Write(16, 6, "GET READY", Colors.White);
            for (uint i = 0; i < 255; i++)
            {
                for (uint j = 0; j < 55; j++)
                { }
            }
            C64.Write(16, 6, "         ", Colors.White);
        }
        static void Main()
        {
            C64.SetCharSet(C64Address.FromLabel("charset"));
            C64.SetMultiColor();
            C64.SetBackgroundColor(Colors.Black);
            C64.SetBorderColor(Colors.Black);
            C64.SetCharBackgroundColor(0, Colors.Grey1);
            C64.SetCharBackgroundColor(1, Colors.White);
            C64.Sprites.CommonColor1 = Colors.Brown;
            C64.Sprites.CommonColor2 = Colors.Grey3;

            TitleScreen.Display();
            Screen.Clear(Colors.Grey2);


            Wall.Draw((Colors)11);

            Player player = new Player()
            {
                Sprite = C64.Sprites.Sprite0
            };
            player.Init();
            Knight knight = new Knight()
            {
                Sprite = C64.Sprites.Sprite1
            };
            knight.Init();
            GetReady();
            Enemy enemy = new Enemy()
            {
                Sprite = C64.Sprites.Sprite2
            };
            enemy.Init();

            for (; ; )
            {
                player.Move();
                knight.Move();
                enemy.Move();
                for (uint i = 0; i < 255; i++)
                {
                    var a = 0;
                    // for (uint j = 0; j < 1; j++)
                    // { }
                }
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
            //     Console.WriteLine("HELLO");

            // for (int i = 0; i < 10; i++)
            //     Console.WriteLine("HELLO WORLD!");
            // GCTest.Start();
            // Game g = new Game();
            // g.Init();
            // g.Run();
        }
    }
}
