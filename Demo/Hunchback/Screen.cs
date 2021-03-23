using C64Lib;

namespace Hunchback
{
    class Screen
    {
        public static void Clear(Colors charColor)
        {
            C64.FillMemory(C64Address.FromLabel("(screenMemory)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$100)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$200)"), 32, 0);
            C64.FillMemory(C64Address.FromLabel("(screenMemory+$300)"), 32, 0);

            C64.FillMemory(C64Address.FromLabel("(colorMemory)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$100)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$200)"), (uint)charColor, 0);
            C64.FillMemory(C64Address.FromLabel("(colorMemory+$300)"), (uint)charColor, 0);
        }

        public static void FillRectangle(int x, int y, ulong sourceChars, ulong sourceColors)
        {

        }
    }
}