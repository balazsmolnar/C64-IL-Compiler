using C64Lib;

namespace Demo;

// Small example project -- exercises the compiler end-to-end without the
// size/complexity of a full game. For the Hunchback game (previously here),
// see the Hunchback/ project.
class Program
{
    static void Main()
    {
        C64.SetBackgroundColor(Colors.Blue);
        C64.SetBorderColor(Colors.LightBlue);
        C64.Write(10, 10, "HELLO C64", Colors.White);
    }
}
