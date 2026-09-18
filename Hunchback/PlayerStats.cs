using C64Lib;

namespace Hunchback;

public class PlayerStats
{
    private const uint BonusX = 1;
    private const uint LivesX = 24;
    private const uint LivesMarkerChar = 61;
    // A bonus marker is a 2x2 tile assembled from 4 quarter-glyphs -- there's
    // no single "bell" character in the charset, unlike the life marker.
    private const uint BellUL = 63;
    private const uint BellUR = 64;
    private const uint BellLL = 65;
    private const uint BellLR = 66;
    // The bonus markers sit on top of the header's decorative top-border
    // glyph (used for columns 0-10 of row 0) -- restore it, in its own
    // color, when a marker is off instead of leaving a blank gap.
    private const uint BorderChar = 68;

    // "Level marker" box at the bottom-left of the screen (rows 21-24,
    // columns 0-6): a static box-drawing frame around a short staircase
    // glyph, with a small dot that sweeps across 4 columns x 4
    // sub-character pixel positions (16 positions total) to show progress
    // through the current 16-level tower -- decoded from the original's
    // Restructure/Screen.asm Screen_DisplayStats/.DisplayLevelMarker and
    // Restructure/Memory.asm's tbl_LevelMarker*/chr_Marker. Matches
    // levelNumber & 15 exactly (the original only ever extracts
    // levelNumber's low 4 bits via two separate "AND 3" masks), so it
    // wraps correctly across this port's 48 levels (3 towers) with no
    // extra masking needed here.
    //
    // The three tables (tbl_LevelMarkerChars/CharCol/Pixel) live in
    // GameData.asm and are read via C64.GetMemory, matching every other
    // static lookup table in this port (tbl_KnightChars etc. in Wall.cs) --
    // NOT a C# static readonly array literal. That IL shape (a
    // RuntimeHelpers.InitializeArray-backed initializer) silently never
    // gets populated by this compiler: the static field stays a null
    // handle, so indexing it reads garbage out of zero page instead of
    // throwing -- confirmed live (a previous version of this code showed
    // random characters/colors instead of the box, the first time this
    // project ever exercised a `= { literal, literal, ... }` array field).
    private const uint LevelMarkerChar = 99;

    public uint Lives;
    public uint Bonus;

    public void Draw(uint levelNumber)
    {
        uint d = 0;
        for (uint y = 0; y < 2; y++)
        {
            for (uint x = 0; x < 40; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("txt_PlayerStats"), d);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_PlayerStatsColours"), d);

                C64.Screen.SetChar(x, y, b, (Colors)c);
                d++;
            }
        }
        DrawLives();
        DrawBonus();
        DrawLevelMarker(levelNumber);
    }

    private void DrawLevelMarker(uint levelNumber)
    {
        uint i = 0;
        for (uint row = 0; row < 4; row++)
        {
            for (uint col = 0; col < 7; col++)
            {
                var ch = C64.GetMemory(C64Address.FromLabel("tbl_LevelMarkerChars"), i);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_LevelMarkerCharCol"), i);
                C64.Screen.SetChar(col, 21 + row, ch, (Colors)c);
                i++;
            }
        }

        var coarse = (levelNumber >> 2) & 3;
        var fine = levelNumber & 3;
        var pixel = C64.GetMemory(C64Address.FromLabel("tbl_LevelMarkerPixel"), fine);
        C64.FillMemory(C64Address.FromLabel("(charset+$31C)"), pixel, 2);
        C64.Screen.SetChar(1 + coarse, 22, LevelMarkerChar, Colors.LightGreen);
    }

    public void DrawLives()
    {
        var shown = Lives > 0 ? Lives - 1 : 0;

        for (uint i = 0; i < 6; i++)
            C64.Screen.SetChar(LivesX + i, 1, i < shown ? LivesMarkerChar : 32, Colors.White);
    }

    public void DrawBonus()
    {
        // The original's Screen_DisplayStats writes only to screen RAM for
        // these cells, never color RAM -- whatever color the static header
        // blit already put there (tbl_PlayerStatsColours: Grey2 across this
        // whole column range, both rows) just shows through underneath.
        var x = BonusX;
        for (uint i = 0; i < 4; i++)
        {
            var on = i < Bonus;
            C64.Screen.SetChar(x, 0, on ? BellUL : BorderChar, Colors.Grey2);
            C64.Screen.SetChar(x + 1, 0, on ? BellUR : BorderChar, Colors.Grey2);
            C64.Screen.SetChar(x, 1, on ? BellLL : 32, Colors.Grey2);
            C64.Screen.SetChar(x + 1, 1, on ? BellLR : 32, Colors.Grey2);
            x += 2;
        }
    }
}