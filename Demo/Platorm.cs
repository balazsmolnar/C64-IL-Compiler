

using C64Lib;

class Platform
{
    public uint X;
    public uint Y;
    public uint Width;
    public Colors Color;

    const uint LUCorner = 0x55;
    const uint RUCorner = 0x49;

    const uint RDCorner = 0x4b;
    const uint LDCorner = 0x4a;

    const uint Line = 0x43;
    public void Draw()
    {
        var min = X + 1;
        var max = X + Width;
        var y1 = Y;
        var y2 = Y + 1;
        for (uint i = min; i < max; i++)
        {
            C64.SetChar(i, y1, Line, Color);
            C64.SetChar(i, y2, Line, Color);
        }
        C64.SetChar(X, y1, LUCorner, Color);
        C64.SetChar(X, y2, LDCorner, Color);
        C64.SetChar(max, y1, RUCorner, Color);
        C64.SetChar(max, y2, RDCorner, Color);
    }

}