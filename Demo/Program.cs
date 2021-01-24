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

        class A
        {
            int a;
        }

        static A a1;
        static A a2;

        static void F(A a3)
        {
            var r1 = C64.Debug.GetObjectRoots(a3);
            C64.SetChar(0, 1, 48 + r1);
        }
        static void Main()
        {

            Game g = new Game();
            g.Init();
            g.Run();
            var b = new A();
            var a = new A();
            // F(a);
            var id = C64.Debug.GetObjectId(a);
            // a = null;
            var r1 = C64.Debug.GetObjectRoots(id);
            // uint r1 = 0;
            C64.SetChar(0, 2, 48 + r1);
        }
    }
}
