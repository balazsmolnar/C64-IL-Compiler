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
    //     enemies, e.g. one high + one low fireball) can't be represented by
    //     this port's single Enemy instance (see Enemy.cs -- one sprite, one
    //     EnemyType). Substituted with one representative enemy of the same
    //     general kind/direction. Genuine dual-enemy support would be a
    //     separate engine feature.
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
            // Obstacle byte $10 (Duo Arrow HiLo Front) -- substituted with a
            // single high arrow.
            new LevelDescription {
                WallType = WallType.KnightPits,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Brown
            },
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
                Color = Colors.Grey2
            },
            // Obstacle byte $20 (Duo Arrow HiLo Rear) -- substituted with a
            // single high arrow moving left-to-right.
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Top,
                Color = Colors.Grey2
            },
            // Obstacle byte $40 (Duo Fireball Lo Both) -- substituted with a
            // single low fireball.
            new LevelDescription {
                WallType = WallType.EmptyPits,
                EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom,
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
            // Obstacle byte $80 (Duo Arrow HiLo Both) -- substituted with a
            // single high arrow.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
                Color = Colors.Orange
            },
            // Level 15 (index 15): the original's Esmerelda's Tower finale
            // (every 16th level -- see Restructure/Level.asm's
            // "and #15; cmp #15" check). The finale minigame itself isn't
            // implemented; substituted with KnightPits, since the original's
            // level-type byte ($31) also has the KnightPit bit set alongside
            // EsmereldaTower. Obstacle byte $40 (Duo Fireball Lo Both) ->
            // single low fireball, as elsewhere.
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
            // Obstacle byte $80 -> single high arrow.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
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
            // Obstacle $80 -> single high arrow.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
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
            // Obstacle $80 -> single high arrow.
            new LevelDescription {
                WallType = WallType.RowOfBells,
                EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.Top,
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
