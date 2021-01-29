using System;
using C64Lib;

namespace Demo
{
    class Program
    {
        static int x;

        static void OnInterrupt(object sender, EventArgs args)
        {
            x = x + 1;
            C64.SetBorderColor((Colors)(x));
        }


        static void Main()
        {
            // Console.WriteLine("HELLO");
            // C64.Interrupt += OnInterrupt;

            // Game g = new Game();
            // g.Init();
            // g.Run();
            GCTest.Start();
        }
    }
}
