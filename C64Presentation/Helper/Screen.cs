using C64Lib;

namespace C64Presentation.Helper
{
    class Screen
    {
        public const int Width = 40;
        public const int Height = 25;
        public static void Clear(Colors charColor = Colors.Black)
        {
            C64.FillMemory(C64Address.FromLabel("(screenMemory)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$100)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$200)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$300)"), 32, 128);

            C64.FillMemory(C64Address.FromLabel("(colorMemory)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$100)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$200)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$300)"), (uint)charColor, 0);
        }
    }
}