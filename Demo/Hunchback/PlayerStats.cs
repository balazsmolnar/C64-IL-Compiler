using C64Lib;

namespace Hunchback;

public class PlayerStats
{
    private const uint BonusX = 8;
    private const uint LivesX = 24;
    private const uint LivesMarkerChar = 61;
    private const uint BonusMarkerChar = 63;

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
        for (uint i = 0; i < 4; i++)
            C64.SetChar(BonusX + i, 1, i < Bonus ? BonusMarkerChar : 32, Colors.Cyan);
    }
}