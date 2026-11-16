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
    Bottom = 32,

    // Picked again every time the missile is fired: high or low; from the
    // left or from the right. Both together: a high one from the left or a
    // low one from the right.
    RandomHeight = 64,
    RandomDirection = 128
}

// What one of the 48 levels is made of, decoded from the original's two
// level tables (tbl_LevelType and tbl_LevelObstacleType, Restructure/
// Memory.asm, in GameData.asm).
public class LevelDescription
{
    public const uint Count = 48;

    public WallType WallType;
    public bool Tower;          // Esmerelda's tower stands at the end: every 16th level
    public EnemyType EnemyType;
    public EnemyType EnemyType2;
    public Colors Color;

    public static LevelDescription Get(uint level)
    {
        uint type = C64.GetMemory(C64Address.FromLabel("tbl_LevelType"), level);
        uint obstacle = C64.GetMemory(C64Address.FromLabel("tbl_LevelObstacleType"), level);
        var d = new LevelDescription();

        // Level type bits: 1 knights in the pits, 2 empty pits, 4 a row of
        // bells, 8 the rope pit, $10 the tower, $20 the wall. (A row of
        // bells always comes with the rope pit under it. Level 34's byte in
        // the original, 208, has no wall bit at all: it is taken as the
        // plain wall it shares its other bits with.)
        d.WallType = WallType.Wall;
        if ((type & 1) != 0)
            d.WallType = WallType.KnightPits;
        if ((type & 2) != 0)
            d.WallType = WallType.EmptyPits;
        if ((type & 8) != 0)
            d.WallType = WallType.Rope;
        if ((type & 4) != 0)
            d.WallType = WallType.RowOfBells;
        d.Tower = (type & 0x10) != 0;

        // The original reads the wall's colour with the level number itself,
        // past the end of its 16 colours and into the table after them:
        // from level 16 on the walls are mostly light blue.
        d.Color = (Colors)(C64.GetMemory(C64Address.FromLabel("tbl_WallCharColours"), level) & 15);

        // Obstacle bits: 1 the rope (part of the rope pit here), 2 a high
        // fireball from the right, 4 a low arrow from the left, 8 a low
        // fireball from the right; then the pairs: $10 arrows from the
        // right and $20 from the left, each high or low; $40 low fireballs
        // from either side; $80 arrows, high from the left or low from the
        // right.
        d.EnemyType = EnemyType.None;
        d.EnemyType2 = EnemyType.None;
        if (obstacle == 2)
            d.EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Top;
        if (obstacle == 4)
            d.EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.Bottom;
        if (obstacle == 8)
            d.EnemyType = EnemyType.FireBall | EnemyType.RightLeft | EnemyType.Bottom;
        if (obstacle == 0x10)
            d.EnemyType = EnemyType.Arrow | EnemyType.RightLeft | EnemyType.RandomHeight;
        if (obstacle == 0x20)
            d.EnemyType = EnemyType.Arrow | EnemyType.LeftRight | EnemyType.RandomHeight;
        if (obstacle == 0x40)
            d.EnemyType = EnemyType.FireBall | EnemyType.Bottom | EnemyType.RandomDirection;
        if (obstacle == 0x80)
            d.EnemyType = EnemyType.Arrow | EnemyType.RandomHeight | EnemyType.RandomDirection;
        if (obstacle >= 0x10)
            d.EnemyType2 = d.EnemyType;
        return d;
    }
}
