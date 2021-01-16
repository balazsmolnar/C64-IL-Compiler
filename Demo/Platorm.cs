

using C64Lib;

class Platform
{
    public uint X;
    public uint Y;
    public uint YPixel;
    public uint XMinPixel;
    public uint XMaxPixel;
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
        YPixel = (y1 << 3) + 33;
        XMinPixel = (min << 3) + 3;
        XMaxPixel = (max << 3) + 20;
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

    public uint DistanceToPlatform(uint x, uint y)
    {
        if (x < XMinPixel)
            return 255;
        if (x > XMaxPixel)
            return 255;
        if (y > YPixel)
            return 255;
        var res = YPixel - y;
        return res;
    }

}