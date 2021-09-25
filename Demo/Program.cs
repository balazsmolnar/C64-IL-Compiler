using System;
using C64Lib;
using Hunchback;

namespace Demo
{
    class A
    {
        public virtual void Bxxx()
        {
            C64.SetBackgroundColor(Colors.Green);
            Cxxx();
        }

        public virtual void Cxxx()
        {
            C64.SetBackgroundColor(Colors.Red);
            for (; ; );
        }
    }
    class Program
    {

        static void Main()
        {
            (new A()).Bxxx();
            var game = new Game();
            game.Start();

            // Colors x = Colors.Black;
            // C64.Interrupt += (s, e) =>
            // {
            //     C64.SetBorderColor(++x);
            // };

            for (int i = 0; i < 10; i++)
                Console.WriteLine("hello world!");
        }
    }
}
