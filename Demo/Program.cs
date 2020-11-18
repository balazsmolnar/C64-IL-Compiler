using System;

namespace Demo
{
    class Program
    {
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
            for (int i=0; i<10; i++) {
                
                WriteSpaces(i);
                Hello("hello world from parameter");
            }
        }
    }
}
