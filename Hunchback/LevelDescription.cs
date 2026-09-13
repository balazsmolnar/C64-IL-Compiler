using C64Lib;

namespace Hunchback;

public enum WallType
{
    Wall,
    EmptyPits,
    KnightPits,
    Rope,
    RowOfBells
}

public enum EnemyType
{
    None,
    FireBall = 1,
    Arrow = 2,

    RightLeft = 4,
    LeftRight = 8,
    Top = 16,
    Bottom = 32
}

public class LevelDescription
{
    public WallType WallType;
    public EnemyType EnemyType;
    // Second enemy for the original's 4 "Duo"/"...Both" obstacle types
    // (None on every other level -- see Enemy.cs's dual-enemy handling in
    // LevelPlay.Play). The original randomly re-picks height (Front/Rear)
    // or direction (the Both variants) each time a missile respawns; this
    // port has no RNG primitive, so both enemies get fixed, deterministic
    // pairings instead (see the per-level comments below) -- same general
    // shape, not frame-accurate to the original's randomization.
    public EnemyType EnemyType2;
    public Colors Color;

    // Decoded from the original disassembly's tbl_LevelType/tbl_LevelObstacleType
    // (C:\temp\HunchBack\Restructure\Memory.asm, 48 entries each) and
    // tbl_WallCharColours (16 entries, cycled 3x across the 48 levels -- the
    // table is indexed by the raw, unmasked level counter in the original,
    // same as the two level tables, so a 3x repeat of the 16-entry palette is
    // the natural reading). Verified against this port's original 8
    // hand-picked entries (now levels 0-7 below) before trusting the decode
    // for the rest -- all 8 reproduce exactly.
    //
    // Two things the original's per-level bitfields can express that this
    // port's simpler WallType/EnemyType model can't, both called out inline
    // below where they occur:
    //   - "Esmerelda's Tower" (1 of the original's 6 tbl_LevelType bits) has
    //     no equivalent WallType -- it's the original's rescue-the-princess
    //     finale (every 16th level), substituted with KnightPits.
    //     Implementing the finale itself is a separate, much bigger feature,
    //     out of scope here. ("Row of bells", the other originally-missing
    //     wall layout, now has a real WallType -- see Wall.cs's
    //     BuildRowOfBells.)
    //   - The original's 4 "Duo"/"...Both" obstacle types (two simultaneous
    //     enemies, e.g. one high + one low fireball) now spawn a real second
    //     Enemy (see EnemyType2 below and LevelPlay.Play's dual-enemy
    //     handling), using a fixed height/direction pairing instead of the
    //     original's per-respawn randomization (no RNG primitive in this
    //     port -- see EnemyType2's own comment).
    public static LevelDescription[] Levels =>
        new[]
        {
            new LevelDescription {
                WallType = WallType.Wall,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.None,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.None,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Brown
            },
            // Obstacle byte $10 (Duo Arrow HiLo Front): two arrows, both
            // moving right-to-left, one high one low.
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey2
            },
            // Obstacle byte $20 (Duo Arrow HiLo Rear): two arrows, both
            // moving left-to-right, one high one low.
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.Grey2
            },
            // Obstacle byte $40 (Duo Fireball Lo Both): two fireballs, both
            // low, moving in opposite directions.
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                EnemyType2 = EnemyType.FireBall | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            // Obstacle byte $80 (Duo Arrow HiLo Both): two arrows, mixed
            // height and direction (one high right-to-left, one low
            // left-to-right).
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.Orange
            },
            // Level 15 (index 15): the original's Esmerelda's Tower finale
            // (every 16th level -- see Restructure/Level.asm's
            // "and #15; cmp #15" check). The finale minigame itself isn't
            // implemented; substituted with KnightPits, since the original's
            // level-type byte ($31) also has the KnightPit bit set alongside
            // EsmereldaTower. Obstacle byte $40 (Duo Fireball Lo Both), as
            // at index 11 above.
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                EnemyType2 = EnemyType.FireBall | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.Orange
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.Wall,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey2
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey2
            },
            // Obstacle byte $80 (Duo Arrow HiLo Both), as at index 14 above.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Orange
            },
            // Level 31 (index 31): second Esmerelda's Tower finale slot --
            // same KnightPits substitute as level 15 above.
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Orange
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey1
            },
            // Level 34 (index 34): the original's level-type byte here is
            // corrupted/anomalous in the disassembly (208 = $D0, breaking
            // the "Wall bit always set" pattern every other entry follows --
            // almost certainly a transcription error in the fan
            // disassembly). Its obstacle byte is a clean $01 (Rope) though,
            // which is what actually drives WallType.Rope in this port (see
            // levels 1/6/19/29/44), so that's used regardless of the garbled
            // type byte.
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.Grey1
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey3
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Brown
            },
            // Obstacle $80 (Duo Arrow HiLo Both), as at index 14 above.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey2
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Grey2
            },
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.Rope,
                EnemyType = EnemyType.None,
                Color = Colors.LightGreen
            },
            // Obstacle $80 (Duo Arrow HiLo Both), as at index 14 above.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                EnemyType2 = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom,
                Color = Colors.LightGreen
            },
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Orange
            },
            // Level 47 (index 47): third Esmerelda's Tower finale slot --
            // same KnightPits substitute as levels 15/31 above.
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Orange
            },
        };
}
