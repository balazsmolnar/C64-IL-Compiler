using C64Lib;

namespace Hunchback;

class Wall : GameObject
{
    const uint WallChar = 62;
    const uint Space = 32;
    const uint Wall3DChar = 60;
    const uint Wall3DTopChar = 59;
    const uint FireChar = 114;
    const uint BellUL = 63;
    const uint BellUR = 64;
    const uint BellLL = 65;
    const uint BellLR = 66;
    const uint BellLedge = 68;
    const uint BellRope = 90;

    private WallType wallType_;
    private uint frameCounter_;

    private uint knightCounter_;
    private uint speedCounter_;
    // Draws the level on whatever Screen is drawing on (see Screen.cs: out
    // of sight, as a rule), over a cleared screen.
    public void Draw(Colors color, WallType wallType, bool tower)
    {
        Screen.Clear(Colors.Black);
        BuildBasicWall(color);
        if (tower)
            BuildTower();
        wallType_ = wallType;
        frameCounter_ = 0;
        knightCounter_ = 0;
        speedCounter_ = 0;
        if (wallType == WallType.KnightPits)
        {
            BuildKnightPit(9);
            BuildKnightPit(18);
            BuildKnightPit(27);
        }

        if (wallType == WallType.EmptyPits)
        {
            BuildEmptyPit(9);
            BuildEmptyPit(18);
            BuildEmptyPit(27);
        }

        if (wallType == WallType.Rope)
        {
            BuildRopePit();
        }

        if (wallType == WallType.RowOfBells)
        {
            // Restructure/Screen.asm's Screen_BuildScreen dispatch loop
            // isn't "highest set bit wins" -- it runs *every* matching
            // handler in sequence (jmp (zpLow) into the handler, which ends
            // in jmp .NextLevelTypeMatch back into the loop). The 8 levels
            // using this WallType all had the original's raw level-type
            // byte $2c: Wall + RopePit + RowOfBells bits all set -- so the
            // original draws the base wall, carves the rope-pit gap, then
            // draws the bells over it. Missing the gap here was the actual
            // root cause of "dies before reaching the first bell": with no
            // visible pit, walking over it looked like dying for no reason.
            // The pit's own swinging-rope *mechanic* stays exclusive to
            // WallType.Rope -- this only reuses its visual gap-carving.
            BuildRopePit();
            BuildRowOfBells();
        }
    }

    private static void BuildBasicWall(Colors color)
    {
        C64.FillMemory(Screen.CharsAt(0x01B8), WallChar, 0);
        C64.FillMemory(Screen.CharsAt(0x02B8), WallChar, 0);
        C64.FillMemory(Screen.CharsAt(0x0300), WallChar, 0xE8);

        C64.FillMemory(Screen.ColoursAt(0x01B8), (uint)color, 0);
        C64.FillMemory(Screen.ColoursAt(0x02B8), (uint)color, 0);
        C64.FillMemory(Screen.ColoursAt(0x0300), (uint)color, 0xE8);

        C64.CopyMemory(Screen.CharsAt(0x0320), C64Address.FromLabel("tbl_WallBaseChars"), 200);

        uint d = 0;
        for (uint y = 3; y < 11; y++)
        {
            for (uint x = 37; x < 40; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("tbl_LevelEndBellChars"), d);

                Screen.Put(x, y, b, Colors.Grey3);
                d++;
            }
        }
    }
    // Esmerelda's tower at the end of every sixteenth wall (the original's
    // tbl_EsmereldaTowerChars and tbl_EsmeTowerCharColour: 8 columns, 9
    // rows, from row 2 of column 32).
    private static void BuildTower()
    {
        uint d = 0;
        for (uint y = 2; y < 11; y++)
        {
            for (uint x = 32; x < 40; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("tbl_EsmereldaTowerChars"), d);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_EsmeTowerCharColour"), d);
                Screen.Put(x, y, b, (Colors)c);
                d++;
            }
        }
    }

    private static void BuildKnightPit(uint startX)
    {
        uint d = 0;
        for (uint y = 10; y < 15; y++)
        {
            for (uint x = startX; x < startX + 4; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("tbl_KnightChars"), d);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_KnightCharsColours"), d);

                Screen.Put(x, y, b, (Colors)c);
                d++;
            }
        }
    }

    private static void BuildEmptyPit(uint startX)
    {
        uint d = 0;
        for (uint y = 10; y < 14; y++)
        {
            for (uint x = startX; x < startX + 4; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("tbl_EmptyPitChars"), d);
                var c = C64.GetMemory(C64Address.FromLabel("tbl_EmptyPitCharColours"), d);

                Screen.Put(x, y, b, (Colors)c);
                d++;
            }
        }
    }

    // The row of bells is not just decoration: its four ropes are what
    // carries the player over the pit (see Player.Move). The numbers are
    // the original's (Memory.asm's tbl_RowOfBellsXPosition,
    // tbl_QuasiBellXPosition, tbl_BellRopeXOffset, tbl_RowOfBellsFreqHi and
    // the limits in Quasi.asm), plus 24: the original keeps the player's
    // position 24 less than his sprite's X, this port keeps the sprite's X
    // itself.
    public bool IsRowOfBells => wallType_ == WallType.RowOfBells;

    // Where a rope can be grabbed at all.
    public bool IsBellZone(ulong x) => x >= 112 && x < 248;

    // Which rope that is (they are 32 pixels apart), as the X he hangs at.
    public ulong GetBellSnapX(ulong x)
    {
        if (x <= 144) return 122;
        if (x <= 176) return 154;
        if (x <= 208) return 186;
        return 218;
    }

    // Over the pit with no rope in reach: each rope can be reached from the
    // 11 pixels starting 4 to the left of where he would hang from it.
    public bool IsBellPit(ulong x)
    {
        if (x < 104 || x >= 243)
            return false;
        return !((x >= 118 && x < 129) || (x >= 150 && x < 161) || (x >= 182 && x < 193) || (x >= 214 && x < 225));
    }

    public void PlayBellSound(ulong x)
    {
        if (x <= 144)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1400UL, 0UL, 10, 0, false);
        else if (x <= 176)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1E00UL, 0UL, 10, 0, false);
        else if (x <= 208)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x2800UL, 0UL, 10, 0, false);
        else
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x3200UL, 0UL, 10, 0, false);
    }

#if SCENE
    // Where a scene build's own player jumps: a jump covers 32 pixels, so
    // far enough before each gap to come down on the other side (the first
    // bell rope, the far edge of a pit).
    public bool IsJumpPoint(ulong x)
    {
        if (wallType_ == WallType.RowOfBells)
            return x == 90;
        if (wallType_ == WallType.EmptyPits || wallType_ == WallType.KnightPits)
            return x == 88 || x == 160 || x == 232;
        return x == 100;
    }
#endif

    public bool IsHole(ulong x)
    {
        if (wallType_ == WallType.EmptyPits || wallType_ == WallType.KnightPits)
            return (x > 92 && x < 114) || (x > 164 && x < 186) || (x > 236 && x < 255);

        if (wallType_ == WallType.Rope)
            return x > 106 && x < 238;

        return false;
    }

    public override void Init()
    {
    }

    public override void Move()
    {
        if (wallType_ != WallType.KnightPits)
            return;
        speedCounter_++;
        if (speedCounter_ < 6)
            return;
        speedCounter_ = 0;
        frameCounter_++;
        if (frameCounter_ == 5)
        {
            frameCounter_ = 0;
            knightCounter_++;
            if (knightCounter_ > 6)
                knightCounter_ = 0;
        }

        if (knightCounter_ < 4)
            return;

        uint x = 10;
        if (knightCounter_ == 4)
            x = 28;
        if (knightCounter_ == 5)
            x = 19;

        switch (frameCounter_)
        {
            case 0:
            case 4:
                KnightPitFrame1(x);
                break;
            case 1:
            case 3:
                KnightPitFrame2(x);
                break;
            case 2:
                KnightPitFrame3(x);
                break;
        }
    }

    // A row of 4 hanging bells (each a 2x2 tile like the bonus-marker icon
    // in PlayerStats), spanning x=13..26 y=3..10, matching the original's
    // Screen_BuildRowOfBells (Restructure/Screen.asm + tbl_RowOfBellsChars
    // in Memory.asm -- decoded byte-for-byte: 4 bell icons at columns
    // 13/17/21/25, a horizontal ledge connecting their tops, and a single
    // vertical rope strand hanging from each down to the base wall).
    //
    // Verified against Screen_DrawCharBlocks's exact read/write indexing
    // (source and dest table both indexed by the same Y, so no column
    // reversal): the first bell's UL glyph really is at screen column 13,
    // matching the draw pointer's scn_TemporaryScreen+$0085 offset
    // (=row 3, col 13) directly, since Screen_CopyGameScreen copies that
    // temp buffer to SCREENRAM at identical offsets (no scroll/transform).
    // Cross-checked against the collision logic too: the exact relationship
    // "quasiX = bellColumn*8 - 10" reproduces all 4 entries of
    // tbl_BellRopeXOffset (94/126/158/190) from columns 13/17/21/25 with
    // zero rounding error, confirming the column values below and the
    // pixel constants used in Wall's grab/fall-zone/snap methods agree
    // with each other.
    private static void BuildRowOfBells()
    {
        BuildBellColumn(13);
        BuildBellColumn(17);
        BuildBellColumn(21);
        BuildBellColumn(25);

        Screen.Put(15, 3, BellLedge, Colors.Grey2);
        Screen.Put(16, 3, BellLedge, Colors.Grey2);
        Screen.Put(19, 3, BellLedge, Colors.Grey2);
        Screen.Put(20, 3, BellLedge, Colors.Grey2);
        Screen.Put(23, 3, BellLedge, Colors.Grey2);
        Screen.Put(24, 3, BellLedge, Colors.Grey2);
    }

    private static void BuildBellColumn(uint x)
    {
        Screen.Put(x, 3, BellUL, Colors.Grey2);
        Screen.Put(x + 1, 3, BellUR, Colors.Grey2);
        Screen.Put(x, 4, BellLL, Colors.Grey2);
        Screen.Put(x + 1, 4, BellLR, Colors.Grey2);
        // The rope, in the original's own colour. (Its pixels are the bit
        // pair 10, which the VIC does count as foreground for sprite
        // collisions: only 00 and 01 are background.)
        for (uint y = 5; y < 11; y++)
            Screen.Put(x, y, BellRope, Colors.Grey2);
    }

    private static void BuildRopePit()
    {
        for (uint x = 12; x < 28; x++)
        {
            for (uint y = 11; y < 21; y++)
            {
                Screen.Put(x, y, Space, Colors.Black);
            }
            Screen.Put(x, 21, FireChar, Colors.Red);
        }

        for (uint y = 12; y < 22; y++)
        {
            Screen.Put(11, y, Wall3DChar, Colors.Red);
        }
        Screen.Put(11, 11, Wall3DTopChar, Colors.Red);
    }

    private static void KnightPitFrame1(uint x)
    {
        Screen.Put(x, 8, 0x20, Colors.LightRed);
        Screen.Put(x, 9, 0x20, Colors.LightRed);
        Screen.Put(x, 10, 0x45, Colors.LightRed);
        Screen.Put(x, 11, 0x47, Colors.LightRed);
        Screen.Put(x, 12, 0x4f, Colors.LightRed);
        Screen.Put(x, 13, 0x50, Colors.LightRed);
        Screen.Put(x, 14, 0x51, Colors.LightRed);
    }

    private static void KnightPitFrame2(uint x)
    {
        Screen.Put(x, 8, 0x20, Colors.LightRed);
        Screen.Put(x, 9, 0x45, Colors.LightRed);
        Screen.Put(x, 10, 0x46, Colors.LightRed);
        Screen.Put(x, 11, 0x47, Colors.LightRed);
        Screen.Put(x, 12, 0x52, Colors.LightRed);
        Screen.Put(x, 13, 0x53, Colors.LightRed);
        Screen.Put(x, 14, 0x54, Colors.LightRed);
    }

    private static void KnightPitFrame3(uint x)
    {
        Screen.Put(x, 8, 0x45, Colors.LightRed);
        Screen.Put(x, 9, 0x46, Colors.LightRed);
        Screen.Put(x, 10, 0x46, Colors.LightRed);
        Screen.Put(x, 11, 0x56, Colors.LightRed);
        Screen.Put(x, 12, 0x55, Colors.LightRed);
        Screen.Put(x, 13, 0x57, Colors.LightRed);
        Screen.Put(x, 14, 0x58, Colors.LightRed);
    }
}