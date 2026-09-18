using System.ComponentModel;
using C64Lib;
using C64Presentation.Helper;
using Screen = C64Presentation.Helper.Screen;

namespace C64Presentation.SlideElements;

class Title : SlideElement
{
    public string Text;
    public override void Draw()
    {
        int length = Text.Length;
        int startX = (Screen.Width - length) >> 1;
        const uint y = 2;
        C64.Screen.Write((uint)startX, y, Text);
        for (var x = 0; x < length; x++)
        {
            C64.Screen.SetChar((uint)(startX + x), y-1, 0x43);
            C64.Screen.SetChar((uint)(startX + x), y+1, 0x43);
        }
    }
}