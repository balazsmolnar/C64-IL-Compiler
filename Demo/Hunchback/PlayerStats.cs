using C64Lib;

namespace Hunchback;

public class PlayerStats
{
    private const uint BonusX = 2;
    private const uint LivesX = 24;
    private const uint LivesMarkerChar = 61;
    // A bonus marker is a 2x2 tile assembled from 4 quarter-glyphs -- there's
    // no single "bell" character in the charset, unlike the life marker.
    private const uint BellUL = 63;
    private const uint BellUR = 64;
    private const uint BellLL = 65;
    private const uint BellLR = 66;

    public uint Lives;
    public uint Bonus;

    public void Draw()
    {
        uint d = 0;
        for (uint y = 0; y < 2; y++)
        {
            for (uint x = 0; x < 40; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("txt_PlayerStats"), d);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_PlayerStatsColours"), d);

                C64.SetChar(x, y, b, (Colors)c);
                d++;
            }
        }
        DrawLives();
        DrawBonus();
    }

    public void DrawLives()
    {
        var shown = Lives > 0 ? Lives - 1 : 0;
        for (uint i = 0; i < 4; i++)
            C64.SetChar(LivesX + i, 1, i < shown ? LivesMarkerChar : 32, Colors.White);
    }

    public void DrawBonus()
    {
        var x = BonusX;
        for (uint i = 0; i < 4; i++)
        {
            var on = i < Bonus;
            C64.SetChar(x, 0, on ? BellUL : 32, Colors.Cyan);
            C64.SetChar(x + 1, 0, on ? BellUR : 32, Colors.Cyan);
            C64.SetChar(x, 1, on ? BellLL : 32, Colors.Cyan);
            C64.SetChar(x + 1, 1, on ? BellLR : 32, Colors.Cyan);
            x += 2;
        }
    }
}