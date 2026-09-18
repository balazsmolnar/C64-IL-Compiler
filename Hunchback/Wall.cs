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
    public void Draw(Colors color, WallType wallType)
    {
        BuildBasicWall(color);
        wallType_ = wallType;
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
        C64.FillMemory(C64Address.FromLabel("screenMemory+$01B8"), WallChar, 0);
        C64.FillMemory(C64Address.FromLabel("screenMemory+$02B8"), WallChar, 0);
        C64.FillMemory(C64Address.FromLabel("screenMemory+$0300"), WallChar, 0);

        C64.FillMemory(C64Address.FromLabel("colorMemory+$01B8"), (uint)color, 0);
        C64.FillMemory(C64Address.FromLabel("colorMemory+$02B8"), (uint)color, 0);
        C64.FillMemory(C64Address.FromLabel("colorMemory+$0300"), (uint)color, 0);

        C64.CopyMemory(C64Address.FromLabel("screenMemory+$0320"), C64Address.FromLabel("tbl_WallBaseChars"), 199);

        uint d = 0;
        for (uint y = 3; y < 11; y++)
        {
            for (uint x = 37; x < 40; x++)
            {
                var b = C64.GetMemory(C64Address.FromLabel("tbl_LevelEndBellChars"), d);

                C64.Screen.SetChar(x, y, b, Colors.Grey3);
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

                C64.Screen.SetChar(x, y, b, (Colors)c);
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

                C64.Screen.SetChar(x, y, b, (Colors)c);
                d++;
            }
        }
    }

    // The row-of-bells decoration isn't just background art: in the original
    // (Restructure/Quasi.asm's Quasi_RowOfBellsCollisionCheck /
    // Quasi_CheckCollision's .CheckBellPitFall, Restructure/Sound.asm's
    // Sound_RowOfBells) touching it grabs the player onto the nearest bell,
    // snaps them into a fixed hang position, and rings that bell's own pitch
    // -- they stay hung there (walking left/right has no further effect
    // while grabbed) until they jump, which releases them. If they instead
    // walk into the bell zone without being close enough to any bell's rope
    // to grab it, they fall through. All 4 X ranges/positions below are
    // decoded directly from the original's tbl_RowOfBellsXPosition/
    // tbl_QuasiBellXPosition/tbl_BellRopeXOffset/tbl_RowOfBellsFreqHi (each
    // 4 entries, one per bell).
    public bool IsRowOfBells => wallType_ == WallType.RowOfBells;

    public bool IsRowOfBellsGrabZone(ulong x) => x >= 88 && x < 224;

    public bool IsRowOfBellsFallZone(ulong x) => x >= 80 && x < 219;

    public ulong GetBellSnapX(ulong x)
    {
        if (x <= 120) return 98;
        if (x <= 152) return 130;
        if (x <= 184) return 162;
        return 194;
    }

    public bool IsNearBellRope(ulong x) =>
        (x >= 94 && x < 105) || (x >= 126 && x < 137) || (x >= 158 && x < 169) || (x >= 190 && x < 201);

    public void PlayBellSound(ulong x)
    {
        if (x <= 120)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1400UL, 0UL, 10, 0, false);
        else if (x <= 152)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x1E00UL, 0UL, 10, 0, false);
        else if (x <= 184)
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x2800UL, 0UL, 10, 0, false);
        else
            C64.Sound.PlayEffectReg2(WaveForm.Triangle, 0x3200UL, 0UL, 10, 0, false);
    }

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
        if (speedCounter_ < 2)
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

        C64.Screen.SetChar(15, 3, BellLedge, Colors.Grey2);
        C64.Screen.SetChar(16, 3, BellLedge, Colors.Grey2);
        C64.Screen.SetChar(19, 3, BellLedge, Colors.Grey2);
        C64.Screen.SetChar(20, 3, BellLedge, Colors.Grey2);
        C64.Screen.SetChar(23, 3, BellLedge, Colors.Grey2);
        C64.Screen.SetChar(24, 3, BellLedge, Colors.Grey2);
    }

    private static void BuildBellColumn(uint x)
    {
        C64.Screen.SetChar(x, 3, BellUL, Colors.Grey2);
        C64.Screen.SetChar(x + 1, 3, BellUR, Colors.Grey2);
        C64.Screen.SetChar(x, 4, BellLL, Colors.Grey2);
        C64.Screen.SetChar(x + 1, 4, BellLR, Colors.Grey2);
        // BellRope's only lit pixel uses multicolor bit-pair "10"
        // (background color 2) -- real VIC-II sprite-background collision
        // only fires for the "11" (individual/foreground) plane in
        // multicolor character mode, so under Grey2 (a multicolor color,
        // bit 3 set) this glyph is visible but never collision-opaque,
        // which is why the player could walk straight through it with no
        // grab at all. White is a hires (non-multicolor) color -- in hires
        // mode any lit bit uses the individual/foreground color directly,
        // so the same glyph becomes genuinely touchable. This is what
        // Player.Move()'s grab logic actually depends on (background
        // collision while in the row-of-bells zone), so it has to be a
        // hires color here regardless of the bell icons above, which are
        // fine as Grey2 since their bitmaps are mostly "11" pixels already.
        for (uint y = 5; y < 11; y++)
            C64.Screen.SetChar(x, y, BellRope, Colors.White);
    }

    private static void BuildRopePit()
    {
        for (uint x = 12; x < 28; x++)
        {
            for (uint y = 11; y < 21; y++)
            {
                C64.Screen.SetChar(x, y, Space, Colors.Black);
            }
            C64.Screen.SetChar(x, 21, FireChar, Colors.Red);
        }

        for (uint y = 12; y < 22; y++)
        {
            C64.Screen.SetChar(11, y, Wall3DChar, Colors.Red);
        }
        C64.Screen.SetChar(11, 11, Wall3DTopChar, Colors.Red);
    }

    private static void KnightPitFrame1(uint x)
    {
        C64.Screen.SetChar(x, 8, 0x20, Colors.LightRed);
        C64.Screen.SetChar(x, 9, 0x20, Colors.LightRed);
        C64.Screen.SetChar(x, 10, 0x45, Colors.LightRed);
        C64.Screen.SetChar(x, 11, 0x47, Colors.LightRed);
        C64.Screen.SetChar(x, 12, 0x4f, Colors.LightRed);
        C64.Screen.SetChar(x, 13, 0x50, Colors.LightRed);
        C64.Screen.SetChar(x, 14, 0x51, Colors.LightRed);
    }

    private static void KnightPitFrame2(uint x)
    {
        C64.Screen.SetChar(x, 8, 0x20, Colors.LightRed);
        C64.Screen.SetChar(x, 9, 0x45, Colors.LightRed);
        C64.Screen.SetChar(x, 10, 0x46, Colors.LightRed);
        C64.Screen.SetChar(x, 11, 0x47, Colors.LightRed);
        C64.Screen.SetChar(x, 12, 0x52, Colors.LightRed);
        C64.Screen.SetChar(x, 13, 0x53, Colors.LightRed);
        C64.Screen.SetChar(x, 14, 0x54, Colors.LightRed);
    }

    private static void KnightPitFrame3(uint x)
    {
        C64.Screen.SetChar(x, 8, 0x45, Colors.LightRed);
        C64.Screen.SetChar(x, 9, 0x46, Colors.LightRed);
        C64.Screen.SetChar(x, 10, 0x46, Colors.LightRed);
        C64.Screen.SetChar(x, 11, 0x56, Colors.LightRed);
        C64.Screen.SetChar(x, 12, 0x55, Colors.LightRed);
        C64.Screen.SetChar(x, 13, 0x57, Colors.LightRed);
        C64.Screen.SetChar(x, 14, 0x58, Colors.LightRed);
    }
}