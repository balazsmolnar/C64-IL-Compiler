using System.Formats.Asn1;
using C64Lib;

namespace C64Presentation.Helper;

class Screen
{
    public const int Width = 40;
    public const int Height = 25;
    public static void Clear(Colors charColor = Colors.Black)
    {
        C64.FillMemory(C64Address.FromLabel("(screenMemory)"), 32, 0);
        C64.FillMemory(C64Address.FromLabel("(screenMemory+$100)"), 32, 0);
        C64.FillMemory(C64Address.FromLabel("(screenMemory+$200)"), 32, 0);
        C64.FillMemory(C64Address.FromLabel("(screenMemory+$300)"), 32, 232);

        C64.FillMemory(C64Address.FromLabel("(colorMemory)"), (uint)charColor, 0);
        C64.FillMemory(C64Address.FromLabel("(colorMemory+$100)"), (uint)charColor, 0);
        C64.FillMemory(C64Address.FromLabel("(colorMemory+$200)"), (uint)charColor, 0);
        C64.FillMemory(C64Address.FromLabel("(colorMemory+$300)"), (uint)charColor, 232);
    }

    public static void ClearRect(uint x1, uint x2, uint y1, uint y2)
    {
        for (uint y = y1; y <= y2; y++)
        {
            for (uint x = x1; x <= x2; x++)
            {
                C64.SetChar(x,y,0x20);
            }
        }
    }
}