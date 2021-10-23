using System;
using C64Lib;
using Hunchback;

namespace Demo
{
    class Program
    {
        static void Main()
        {
            // var ss = new string[] { "hello", "world", "bye" };
            // foreach (var s in ss)
            // {
            //     Console.WriteLine(s);
            // }

            // Console.WriteLine("helloxxx");
            // Console.WriteLine(ss[0]);
            // var g = new Game();
            // g.Start();

            var Lines = new string[]
            {
                    ".3C0F   A4 24      LDY STACKPOINTER",
                    ".3C11   B9 FF C6   LDA LOCALSSTACK-1,Y",
                    ".3C14   48         PHA",
                    ".3C15   A4 24      LDY STACKPOINTER",
                    ".3C17   B9 FE C6   LDA LOCALSSTACK-2,Y",
                    ".3C1A   48         PHA",
                    ".3C1B   68         PLA",
                    ".3C1C   85 32      STA $32",
                    ".3C1E   68         PLA",
                    ".3C1F   18         CLC",
                    ".3C20   65 32      ADC $32",
                    ".3C22   48         PHA",
            };
        }

        static int Add(int a, int b)
        {
            return a + b;
        }
    }
}
