using System;
using C64Lib;

namespace Demo
{
    class Program
    {
        static void ClearScreen()
        {
            for (int x=0; x<40; x++)
            {
                for (int y=0; y<40; y++)
                {
                    C64.SetChar(x,y,32);
                }
            }
        }
        static void Hello(string s)
        {
            Console.WriteLine(s);
        }

        static void WriteSpaces(int n)
        {
                for (int j=0; j<n; j++) {
                    Console.Write(" ");
                }
        }

        static void Main()
        {
            ClearScreen();
            for (int i=0; i<10; i++) {
                
                WriteSpaces(i);
                Hello("hello world from parameter");
            }
        }
    }
}
