using C64Lib;
using C64Presentation.Helper;

namespace C64Presentation.SlideElements;

class CodeBlock : SlideElement
{
    public uint X, Y, Width, Height;
    public string[] Lines;
    public Colors Color;

    private const uint SPACE = 0x20;

    public override void Draw()
    {
        uint a = X;
        uint b = Y;

        for (uint x = a; x < X+Width; x++)
        {
            for (uint y = b; y < Y+Height; y++)
            {
                C64.SetChar(x,y,SPACE);
            }
        }

        var yy = Y;
        foreach (var line in Lines)
        {
            C64.Write(X, yy++, line, Colors.White);
        }
    }
}